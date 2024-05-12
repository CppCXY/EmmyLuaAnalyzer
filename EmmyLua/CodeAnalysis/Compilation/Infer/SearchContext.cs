using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Compilation.Type.DetailType;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Infer;

public class SearchContext(LuaCompilation compilation, SearchContextFeatures features)
{
    public LuaCompilation Compilation { get; } = compilation;

    public SearchContextFeatures Features { get; set; } = features;

    private Dictionary<long, LuaType> Caches { get; } = new();

    private Dictionary<LuaType, List<LuaDeclaration>> MemberCaches { get; } = new();

    private Dictionary<string, List<LuaDeclaration>> BaseMemberCaches { get; } = new();

    private Dictionary<LuaType, Dictionary<TypeOperatorKind, List<TypeOperator>>> TypeOperatorCaches { get; } = new();

    private Dictionary<long, LuaDeclaration?> DeclarationCaches { get; } = new();

    private HashSet<long> InferGuard { get; } = new();

    private const int MaxDepth = 1000;

    // 推断深度
    private int _currentDepth;

    public LuaType Infer(LuaSyntaxElement? element)
    {
        if (element is null)
        {
            return Builtin.Unknown;
        }

        if (Features.Cache)
        {
            if (Caches.TryGetValue(element.UniqueId, out var luaType))
            {
                return luaType;
            }

            luaType = InferCore(element);
            if (Features.CacheUnknown || !luaType.Equals(Builtin.Unknown))
            {
                Caches[element.UniqueId] = luaType;
            }

            return luaType;
        }

        return InferCore(element);
    }

    public void ClearCache()
    {
        Caches.Clear();
        MemberCaches.Clear();
        BaseMemberCaches.Clear();
        TypeOperatorCaches.Clear();
        DeclarationCaches.Clear();
    }

    private LuaType InferCore(LuaSyntaxElement element)
    {
        if (_currentDepth > MaxDepth)
        {
            return Builtin.Unknown;
        }

        if (!InferGuard.Add(element.UniqueId))
        {
            return Builtin.Unknown;
        }

        try
        {
            _currentDepth++;
            return element switch
            {
                LuaExprSyntax expr => ExpressionInfer.InferExpr(expr, this),
                LuaLocalNameSyntax localName => DeclarationInfer.InferLocalName(localName, this),
                LuaParamDefSyntax paramDef => DeclarationInfer.InferParam(paramDef, this),
                LuaSourceSyntax source => DeclarationInfer.InferSource(source, this),
                LuaDocTypeSyntax ty => TypeInfer.InferType(ty, this),
                _ => Builtin.Unknown
            };
        }
        finally
        {
            _currentDepth--;
            InferGuard.Remove(element.UniqueId);
        }
    }

    public IEnumerable<LuaDeclaration> GetRawMembers(string name)
    {
        if (name is "_G" or "_ENV" or "global")
        {
            return Compilation.DbManager.GetGlobals();
        }

        return Compilation.DbManager.GetMembers(name);
    }

    private void CollectSupers(string name, HashSet<LuaType> hashSet, List<LuaNamedType> result)
    {
        var supers = Compilation.DbManager.GetSupers(name).ToList();
        var namedTypes = new List<LuaNamedType>();
        foreach (var super in supers)
        {
            if (hashSet.Add(super))
            {
                if (super is LuaNamedType namedType)
                {
                    result.Add(namedType);
                    namedTypes.Add(namedType);
                }
            }
        }

        foreach (var namedType in namedTypes)
        {
            var detailType = namedType.GetDetailType(this);
            if (detailType.IsClass)
            {
                CollectSupers(namedType.Name, hashSet, result);
            }
        }
    }

    public IEnumerable<LuaDeclaration> GetBaseMembers(string name)
    {
        if (Features is {Cache: true, CacheBaseMember: true} && BaseMemberCaches.TryGetValue(name, out var members))
        {
            return members;
        }

        var hashSet = new HashSet<LuaType>();
        var result = new List<LuaNamedType>();
        CollectSupers(name, hashSet, result);
        members = new List<LuaDeclaration>();
        foreach (var namedType in result)
        {
            if (namedType.Name != name)
            {
                members.AddRange(GetRawMembers(namedType.Name));
            }
        }

        if (Features is {Cache: true, CacheBaseMember: true})
        {
            BaseMemberCaches[name] = members;
        }

        return members;
    }

    private IEnumerable<LuaDeclaration> GetMembers(string name)
    {
        var selfMembers = GetRawMembers(name);
        var baseMembers = GetBaseMembers(name);
        var allMembers = selfMembers.Concat(baseMembers);
        var distinctMembers = allMembers.GroupBy(m => m.Name).Select(g => g.First());
        return distinctMembers;
    }

    public IEnumerable<LuaDeclaration> GetMembers(LuaType luaType)
    {
        if (luaType is LuaGenericType genericType)
        {
            return GetGenericMembers(genericType);
        }

        if (luaType is LuaNamedType namedType)
        {
            var detailType = namedType.GetDetailType(this);
            if (detailType.IsAlias && detailType is AliasDetailType {OriginType: { } originType, Name: { } name})
            {
                // TODO 防止错误递归 ---@alias a a
                if (originType is LuaNamedType {Name: { } originName} && originName == name)
                {
                    return Enumerable.Empty<LuaDeclaration>();
                }

                return GetMembers(originType);
            }

            return GetMembers(namedType.Name);
        }

        if (luaType is LuaUnionType unionType)
        {
            return unionType.UnionTypes.SelectMany(GetMembers);
        }

        if (luaType is LuaTupleType tupleType)
        {
            return tupleType.TupleDeclaration;
        }

        return Enumerable.Empty<LuaDeclaration>();
    }

    private IEnumerable<LuaDeclaration> GetGenericMembers(LuaGenericType genericType)
    {
        if (Features.Cache && MemberCaches.TryGetValue(genericType, out var instanceMembers))
        {
            return instanceMembers;
        }

        var members = GetMembers(genericType.Name);
        var genericParams = Compilation.DbManager.GetGenericParams(genericType.Name).ToList();
        var genericArgs = genericType.GenericArgs;

        var genericMap = new Dictionary<string, LuaType>();
        for (var i = 0; i < genericParams.Count && i < genericArgs.Count; i++)
        {
            genericMap[genericParams[i].Name] = genericArgs[i];
        }

        instanceMembers = new List<LuaDeclaration>();
        foreach (var member in members)
        {
            instanceMembers.Add(member.Instantiate(genericMap));
        }

        if (Features.Cache)
        {
            MemberCaches.TryAdd(genericType, instanceMembers);
        }

        return instanceMembers;
    }

    public IEnumerable<LuaDeclaration> FindMember(LuaType luaType, string memberName)
    {
        if (luaType is LuaNamedType namedType)
        {
            if (namedType is {Name: "table"})
            {
                return FindTableMember(namedType, memberName);
            }

            var fieldDeclarations = GetMembers(namedType)
                .Where(it => string.Equals(it.Name, memberName, StringComparison.CurrentCulture));

            return fieldDeclarations;
        }

        if (luaType is LuaUnionType unionType)
        {
            return unionType.UnionTypes.SelectMany(it => FindMember(it, memberName));
        }

        if (luaType is LuaTupleType tupleType)
        {
            return GetMembers(tupleType)
                .Where(it => string.Equals(it.Name, memberName, StringComparison.CurrentCulture));
        }

        return Enumerable.Empty<LuaDeclaration>();
    }

    private IEnumerable<LuaDeclaration> FindTableMember(LuaNamedType namedType, string memberName)
    {
        if (namedType is LuaGenericType genericTable)
        {
            var args = genericTable.GenericArgs;
            if (args.Count != 2)
            {
                return Enumerable.Empty<LuaDeclaration>();
            }

            var firstType = genericTable.GenericArgs[0];
            var secondType = genericTable.GenericArgs[1];

            if ((firstType.Equals(Builtin.Integer) || firstType.Equals(Builtin.Number))
                && memberName.StartsWith("["))
            {
                return [new LuaDeclaration(memberName, 0, new VirtualInfo(secondType))];
            }

            if (firstType.Equals(Builtin.String) && !memberName.StartsWith("["))
            {
                return [new LuaDeclaration(memberName, 0, new VirtualInfo(secondType))];
            }
        }

        return Enumerable.Empty<LuaDeclaration>();
    }

    private IEnumerable<LuaDeclaration> FindTableMember(LuaNamedType namedType, LuaType keyType)
    {
        if (namedType is LuaGenericType genericTable)
        {
            var args = genericTable.GenericArgs;
            if (args.Count != 2)
            {
                return Enumerable.Empty<LuaDeclaration>();
            }

            var firstType = genericTable.GenericArgs[0];
            var secondType = genericTable.GenericArgs[1];

            if (keyType.SubTypeOf(firstType, this))
            {
                return [new LuaDeclaration(string.Empty, 0, new VirtualInfo(secondType))];
            }
        }

        return Enumerable.Empty<LuaDeclaration>();
    }

    private IEnumerable<LuaDeclaration> FindIndexMember(LuaType luaType, LuaType keyType)
    {
        if (luaType is LuaNamedType namedType)
        {
            if (namedType is {Name: "table"})
            {
                return FindTableMember(namedType, keyType);
            }

            var op = GetBestMatchedIndexOperator(luaType, keyType);
            if (op is not null)
            {
                return [op.LuaDeclaration];
            }
        }
        else if (luaType is LuaUnionType unionType)
        {
            return unionType.UnionTypes.SelectMany(it => FindIndexMember(it, keyType));
        }
        else if (luaType is LuaArrayType arrayType)
        {
            if (keyType.Equals(Builtin.Integer) || keyType.Equals(Builtin.Number))
            {
                return [new LuaDeclaration(string.Empty, 0, new VirtualInfo(arrayType.BaseType))];
            }
        }

        return Enumerable.Empty<LuaDeclaration>();
    }

    public IEnumerable<LuaDeclaration> FindMember(LuaType luaType, LuaIndexExprSyntax indexExpr)
    {
        var declarations = new List<LuaDeclaration>();
        if (indexExpr is {Name: { } name})
        {
            declarations.AddRange(FindMember(luaType, name));
        }
        else if (indexExpr is {IndexKeyExpr: { } keyExpr})
        {
            var keyExprType = Infer(keyExpr);
            declarations.AddRange(FindIndexMember(luaType, keyExprType));
        }

        if (declarations.Count == 0)
        {
            LuaType keyType = Builtin.Unknown;
            if (indexExpr.DotOrColonIndexName != null)
            {
                keyType = Builtin.String;
            }
            else if (indexExpr.IndexKeyExpr is LuaLiteralExprSyntax literal)
            {
                if (literal.Literal is LuaStringToken)
                {
                    keyType = Builtin.String;
                }
                else if (literal.Literal is LuaIntegerToken luaIntegerToken)
                {
                    keyType = Builtin.Integer;
                }
            }
            else
            {
                keyType = Infer(indexExpr.KeyElement);
            }

            var op = GetBestMatchedIndexOperator(luaType, keyType);
            if (op != null)
            {
                declarations.Add(op.LuaDeclaration);
            }
        }

        return declarations;
    }

    public IEnumerable<LuaDeclaration> FindSuperMember(LuaType luaType, string member)
    {
        if (luaType is LuaNamedType namedType)
        {
            var members = GetBaseMembers(namedType.Name);
            return members.Where(it => it.Name == member);
        }

        return Enumerable.Empty<LuaDeclaration>();
    }

    private IEnumerable<TypeOperator> GetOperators(TypeOperatorKind kind, LuaNamedType left)
    {
        if (left is LuaGenericType genericType)
        {
            if (Features.Cache)
            {
                if (TypeOperatorCaches.TryGetValue(genericType, out var cache))
                {
                    if (cache.TryGetValue(kind, out var operators))
                    {
                        return operators;
                    }
                }
            }

            var originOperators = Compilation.DbManager.GetTypeOperators(left.Name)
                .Where(it => it.Kind == kind).ToList();

            var genericParams = Compilation.DbManager.GetGenericParams(genericType.Name).ToList();
            var genericArgs = genericType.GenericArgs;

            var genericMap = new Dictionary<string, LuaType>();
            for (var i = 0; i < genericParams.Count && i < genericArgs.Count; i++)
            {
                genericMap[genericParams[i].Name] = genericArgs[i];
            }

            var instanceOperators = originOperators.Select(op => op.Instantiate(genericMap)).ToList();
            if (Features.Cache)
            {
                if (!TypeOperatorCaches.TryGetValue(genericType, out var cache))
                {
                    cache = new Dictionary<TypeOperatorKind, List<TypeOperator>>();
                    TypeOperatorCaches.Add(genericType, cache);
                }

                cache[kind] = instanceOperators;
            }

            return instanceOperators;
        }

        return Compilation.DbManager.GetTypeOperators(left.Name)
            .Where(it => it.Kind == kind);
    }

    public BinaryOperator? GetBestMatchedBinaryOperator(TypeOperatorKind kind, LuaType left, LuaType right)
    {
        if (left is not LuaNamedType namedType)
        {
            return null;
        }

        var operators = GetOperators(kind, namedType);

        var bestMatched = operators
            .OfType<BinaryOperator>()
            .FirstOrDefault(it => it.Right.Equals(right));

        return bestMatched;
    }

    public UnaryOperator? GetBestMatchedUnaryOperator(TypeOperatorKind kind, LuaType type)
    {
        if (type is not LuaNamedType namedType)
        {
            return null;
        }

        var operators = GetOperators(kind, namedType);

        return operators.OfType<UnaryOperator>().FirstOrDefault();
    }

    public IndexOperator? GetBestMatchedIndexOperator(LuaType type, LuaType key)
    {
        if (type is not LuaNamedType namedType)
        {
            return null;
        }

        var operators = GetOperators(TypeOperatorKind.Index, namedType);
        var bestMatched = operators
            .OfType<IndexOperator>()
            .FirstOrDefault(it => it.Key.Equals(key));
        return bestMatched;
    }

    public LuaDeclaration? FindDeclaration(LuaSyntaxElement? element)
    {
        if (element is null)
        {
            return null;
        }

        if (Features.Cache && DeclarationCaches.TryGetValue(element.UniqueId, out var declaration))
        {
            return declaration;
        }

        var declarationTree = Compilation.GetDeclarationTree(element.DocumentId);
        declaration = declarationTree?.FindDeclaration(element, this);
        if (Features.Cache)
        {
            DeclarationCaches[element.UniqueId] = declaration;
        }

        return declaration;
    }

    public LuaType InferExprShouldBeType(LuaExprSyntax expr)
    {
        return ExpressionShouldBeInfer.InferExprShouldBe(expr, this);
    }
}
