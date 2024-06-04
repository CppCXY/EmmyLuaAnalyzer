using EmmyLua.CodeAnalysis.Common;
using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Type;

namespace EmmyLua.CodeAnalysis.Compilation.Search;

public class SearchContext
{
    public LuaCompilation Compilation { get; }

    internal SearchContextFeatures Features { get; set; }

    private Dictionary<SyntaxElementId, LuaType> InferCaches { get; } = new();

    private Dictionary<LuaType, Dictionary<TypeOperatorKind, List<TypeOperator>>> TypeOperatorCaches { get; } = new();

    private Declarations Declarations { get; }

    private Members Members { get; }

    private References References { get; }

    private HashSet<SyntaxElementId> InferGuard { get; } = [];

    private const int MaxDepth = 1000;

    // 推断深度
    private int _currentDepth;

    public SearchContext(LuaCompilation compilation, SearchContextFeatures features)
    {
        Compilation = compilation;
        Declarations = new Declarations(this);
        Members = new Members(this);
        References = new References(this);
        Features = features;
    }

    public LuaType Infer(LuaSyntaxElement? element)
    {
        if (element is null)
        {
            return Builtin.Unknown;
        }

        if (Features.Cache)
        {
            if (InferCaches.TryGetValue(element.UniqueId, out var luaType))
            {
                return luaType;
            }

            luaType = InferCore(element);
            if (Features.CacheUnknown || !luaType.Equals(Builtin.Unknown))
            {
                InferCaches[element.UniqueId] = luaType;
            }

            return luaType;
        }

        return InferCore(element);
    }

    public void ClearCache()
    {
        InferCaches.Clear();
        TypeOperatorCaches.Clear();
    }

    public void ClearMemberCache(string name)
    {
        Members.ClearMember(name);
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

            var originOperators = Compilation.Db.QueryTypeOperators(left.Name)
                .Where(it => it.Kind == kind).ToList();

            var genericParams = Compilation.Db.QueryGenericParams(genericType.Name).ToList();
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

        return Compilation.Db.QueryTypeOperators(left.Name)
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

    public IDeclaration? FindDeclaration(LuaSyntaxElement? element)
    {
        return Declarations.FindDeclaration(element);
    }

    public IEnumerable<LuaDeclaration> GetDocumentLocalDeclarations(LuaDocumentId documentId)
    {
        return Compilation.Db.QueryDocumentLocalDeclarations(documentId);
    }

    public LuaType InferExprShouldBeType(LuaExprSyntax expr)
    {
        return ExpressionShouldBeInfer.InferExprShouldBe(expr, this);
    }

    public void FindMethodsForType(LuaType type, Action<LuaMethodType> action)
    {
        switch (type)
        {
            case LuaUnionType unionType:
            {
                foreach (var t in unionType.UnionTypes)
                {
                    InnerFindMethods(t, action);
                }

                break;
            }
            default:
            {
                InnerFindMethods(type, action);
                break;
            }
        }
    }

    private void InnerFindMethods(LuaType type, Action<LuaMethodType> action)
    {
        switch (type)
        {
            case LuaMethodType methodType:
            {
                action(methodType);
                break;
            }
            case LuaNamedType namedType:
            {
                var founded = false;
                var overloads = Compilation.Db.QueryTypeOverloads(namedType.Name);
                foreach (var methodType in overloads)
                {
                    founded = true;
                    action(methodType);
                }

                if (!founded && !Compilation.Workspace.Features.TypeCallStrict)
                {
                    var luaMethod = new LuaMethodType(namedType, [], false);
                    action(luaMethod);
                }
                break;
            }
        }
    }

    public IEnumerable<IDeclaration> GetMembers(LuaType type)
    {
        return Members.GetMembers(type);
    }

    public IEnumerable<IDeclaration> FindMember(LuaType type, string name)
    {
        return Members.FindMember(type, name);
    }

    public IEnumerable<IDeclaration> FindMember(LuaType type, LuaIndexExprSyntax indexExpr)
    {
        return Members.FindMember(type, indexExpr);
    }

    public IEnumerable<IDeclaration> FindSuperMember(LuaType type, string name)
    {
        return Members.FindSuperMember(type, name);
    }

    public IEnumerable<ReferenceResult> FindReferences(IDeclaration declaration)
    {
        return References.FindReferences(declaration);
    }

    public bool IsUpValue(LuaNameExprSyntax nameExpr, LuaDeclaration declaration)
    {
        return Declarations.IsUpValue(nameExpr, declaration);
    }
}
