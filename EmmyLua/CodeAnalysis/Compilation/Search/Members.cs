using EmmyLua.CodeAnalysis.Common;
using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Type;

namespace EmmyLua.CodeAnalysis.Compilation.Search;

public class Members(SearchContext context)
{
    private Dictionary<string, List<IDeclaration>> NamedTypeMemberCaches { get; } = new();

    private Dictionary<LuaType, List<IDeclaration>> GenericMemberCaches { get; } = new();

    private Dictionary<string, List<IDeclaration>> BaseMemberCaches { get; } = new();

    public IEnumerable<IDeclaration> GetRawMembers(string name)
    {
        if (context.Features.Cache && NamedTypeMemberCaches.TryGetValue(name, out var members))
        {
            return members;
        }

        members = name switch
        {
            "global" => context.Compilation.Db.QueryAllGlobal().ToList(),
            _ => context.Compilation.Db.QueryMembers(name).ToList()
        };

        if (context.Features.Cache)
        {
            NamedTypeMemberCaches[name] = members;
        }

        return members;
    }

    private void CollectSupers(string name, HashSet<LuaType> hashSet, List<LuaNamedType> result)
    {
        var supers = context.Compilation.Db.QuerySupers(name).ToList();
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
            if (namedType.GetTypeKind(context) == NamedTypeKind.Class)
            {
                CollectSupers(namedType.Name, hashSet, result);
            }
        }
    }

    public IEnumerable<IDeclaration> GetSupersMembers(string name)
    {
        if (context.Features.Cache && BaseMemberCaches.TryGetValue(name, out var members))
        {
            return members;
        }

        var hashSet = new HashSet<LuaType>();
        var result = new List<LuaNamedType>();
        CollectSupers(name, hashSet, result);
        members = [];
        foreach (var namedType in result)
        {
            if (namedType.Name != name)
            {
                members.AddRange(GetRawMembers(namedType.Name));
            }
        }

        if (context.Features.Cache)
        {
            BaseMemberCaches[name] = members;
        }

        return members;
    }

    private IEnumerable<IDeclaration> GetMembers(string name)
    {
        var selfMembers = GetRawMembers(name);
        var supersMembers = GetSupersMembers(name);
        var allMembers = selfMembers.Concat(supersMembers);
        var distinctMembers = allMembers.GroupBy(m => m.Name).Select(g => g.First());
        return distinctMembers;
    }

    public IEnumerable<IDeclaration> GetMembers(LuaType luaType)
    {
        if (luaType is LuaGenericType genericType)
        {
            return GetGenericMembers(genericType);
        }

        if (luaType is LuaNamedType namedType)
        {
            var namedTypeKind = namedType.GetTypeKind(context);
            if (namedTypeKind == NamedTypeKind.Alias)
            {
                var originType = context.Compilation.Db.QueryAliasOriginTypes(namedType.Name).FirstOrDefault();
                // TODO 防止错误递归 ---@alias a a
                if (originType is LuaNamedType { Name: { } originName } && originName == namedType.Name)
                {
                    return [];
                }
                else if (originType is not null)
                {
                    return GetMembers(originType);
                }
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

        return [];
    }

    private IEnumerable<IDeclaration> GetGenericMembers(LuaGenericType genericType)
    {
        if (context.Features.Cache && GenericMemberCaches.TryGetValue(genericType, out var instanceMembers))
        {
            return instanceMembers;
        }

        var members = GetMembers(genericType.Name);
        var genericParams = context.Compilation.Db.QueryGenericParams(genericType.Name).ToList();
        var genericArgs = genericType.GenericArgs;

        var genericMap = new Dictionary<string, LuaType>();
        for (var i = 0; i < genericParams.Count && i < genericArgs.Count; i++)
        {
            genericMap[genericParams[i].Name] = genericArgs[i];
        }

        instanceMembers = [];
        foreach (var member in members)
        {
            instanceMembers.Add(member.Instantiate(genericMap));
        }

        if (context.Features.Cache)
        {
            GenericMemberCaches.TryAdd(genericType, instanceMembers);
        }

        return instanceMembers;
    }

    public IEnumerable<IDeclaration> FindMember(LuaType luaType, string memberName)
    {
        if (luaType is LuaNamedType namedType)
        {
            if (namedType is { Name: "table" })
            {
                return FindTableMember(namedType, memberName);
            }

            return GetMembers(namedType)
                .Where(it => string.Equals(it.Name, memberName, StringComparison.CurrentCulture));
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

        return [];
    }

    private IEnumerable<IDeclaration> FindTableMember(LuaNamedType namedType, string memberName)
    {
        if (namedType is LuaGenericType genericTable)
        {
            var args = genericTable.GenericArgs;
            if (args.Count != 2)
            {
                return [];
            }

            var firstType = genericTable.GenericArgs[0];
            var secondType = genericTable.GenericArgs[1];

            if ((firstType.Equals(Builtin.Integer) || firstType.Equals(Builtin.Number))
                && memberName.StartsWith("["))
            {
                return [new LuaDeclaration(memberName, new VirtualInfo(secondType))];
            }

            if (firstType.Equals(Builtin.String) && !memberName.StartsWith("["))
            {
                return [new LuaDeclaration(memberName, new VirtualInfo(secondType))];
            }
        }

        return [];
    }

    private IEnumerable<LuaDeclaration> FindTableMember(LuaNamedType namedType, LuaType keyType)
    {
        if (namedType is LuaGenericType genericTable)
        {
            var args = genericTable.GenericArgs;
            if (args.Count != 2)
            {
                return [];
            }

            var firstType = genericTable.GenericArgs[0];
            var secondType = genericTable.GenericArgs[1];

            if (keyType.SubTypeOf(firstType, context))
            {
                return [new LuaDeclaration(string.Empty, new VirtualInfo(secondType))];
            }
        }

        return [];
    }

    private IEnumerable<IDeclaration> FindIndexMember(LuaType luaType, LuaType keyType)
    {
        if (luaType is LuaNamedType namedType)
        {
            if (namedType is { Name: "table" })
            {
                return FindTableMember(namedType, keyType);
            }

            var op = context.GetBestMatchedIndexOperator(luaType, keyType);
            if (op is not null)
            {
                return [op.Declaration];
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
                return [new LuaDeclaration(string.Empty, new VirtualInfo(arrayType.BaseType))];
            }
        }

        return [];
    }

    public IEnumerable<IDeclaration> FindMember(LuaType luaType, LuaIndexExprSyntax indexExpr)
    {
        if (luaType.Equals(Builtin.Unknown))
        {
            yield break;
        }

        var notFounded = true;
        if (indexExpr is { Name: { } name })
        {
            foreach (var declaration in FindMember(luaType, name))
            {
                notFounded = false;
                yield return declaration;
            }
        }
        else if (indexExpr is { IndexKeyExpr: { } keyExpr })
        {
            var keyExprType = context.Infer(keyExpr);
            foreach (var declaration in FindIndexMember(luaType, keyExprType))
            {
                notFounded = false;
                yield return declaration;
            }
        }

        if (notFounded)
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
                keyType = context.Infer(indexExpr.KeyElement);
            }

            var op = context.GetBestMatchedIndexOperator(luaType, keyType);
            if (op != null)
            {
                yield return op.Declaration;
            }
        }
    }

    public IEnumerable<IDeclaration> FindSuperMember(LuaType luaType, string member)
    {
        if (luaType is LuaNamedType namedType)
        {
            var members = GetSupersMembers(namedType.Name);
            return members.Where(it => string.Equals(it.Name, member, StringComparison.CurrentCulture));
        }

        return [];
    }

    public void Clear()
    {
        NamedTypeMemberCaches.Clear();
        GenericMemberCaches.Clear();
        BaseMemberCaches.Clear();
    }

    public void ClearMember(string name)
    {
        NamedTypeMemberCaches.Remove(name);
        BaseMemberCaches.Remove(name);
    }
}
