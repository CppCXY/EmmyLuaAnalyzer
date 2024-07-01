using EmmyLua.CodeAnalysis.Common;
using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Search;

public class Members(SearchContext context)
{
    private Dictionary<LuaType, List<IDeclaration>> TypeMemberCaches { get; } = new();

    private Dictionary<LuaType, List<IDeclaration>> GenericMemberCaches { get; } = new();

    private Dictionary<LuaType, List<IDeclaration>> BaseMemberCaches { get; } = new();

    private HashSet<LuaType> MemberGuard { get; } = new();

    public IEnumerable<IDeclaration> GetRawMembers(LuaType luaType)
    {
        if (!luaType.HasMember)
        {
            return [];
        }

        if (context.Features.Cache && TypeMemberCaches.TryGetValue(luaType, out var members))
        {
            return members;
        }

        members = context.Compilation.Db.QueryMembers(luaType).ToList();

        if (context.Features.Cache)
        {
            TypeMemberCaches[luaType] = members;
        }

        return members;
    }

    private void CollectSupers(string name, HashSet<LuaType> hashSet, List<LuaType> result)
    {
        var supers = context.Compilation.Db.QuerySupers(name);
        var namedSupers = new List<string>();
        foreach (var super in supers)
        {
            if (hashSet.Add(super))
            {
                result.Add(super);
                if (super is LuaNamedType namedType)
                {
                    namedSupers.Add(namedType.Name);
                }
            }
        }

        foreach (var super in namedSupers)
        {
            CollectSupers(super, hashSet, result);
        }
    }

    public IEnumerable<IDeclaration> GetSupersMembers(LuaType luaType)
    {
        if (context.Features.Cache && BaseMemberCaches.TryGetValue(luaType, out var members))
        {
            return members;
        }

        var hashSet = new HashSet<LuaType>();
        var result = new List<LuaType>();
        if (luaType is LuaNamedType namedType)
        {
            CollectSupers(namedType.Name, hashSet, result);
        }

        members = [];
        foreach (var superType in result)
        {
            if (!superType.Equals(luaType))
            {
                members.AddRange(GetRawMembers(superType));
            }
        }

        if (context.Features.Cache)
        {
            BaseMemberCaches[luaType] = members;
        }

        return members;
    }

    private IEnumerable<IDeclaration> GetRelatedMembers(LuaType luaType)
    {
        if (luaType is LuaVariableRefType variableRefType)
        {
            var relatedType = context.Compilation.Db.QueryTypeFromId(variableRefType.Id);
            if (relatedType is not null && !relatedType.Equals(luaType))
            {
                return GetMembers(relatedType);
            }
        }
        else if (luaType is GlobalNameType globalNameType)
        {
            var relatedType = context.Compilation.Db.QueryRelatedGlobalType(globalNameType.Name);
            if (relatedType is not null && !relatedType.Equals(luaType))
            {
                return GetMembers(relatedType);
            }
        }

        return [];
    }

    private IEnumerable<IDeclaration> GetNormalTypeMembers(LuaType luaType)
    {
        var selfMembers = GetRawMembers(luaType);
        var supersMembers = GetSupersMembers(luaType);
        var relatedMembers = GetRelatedMembers(luaType);
        var allMembers = selfMembers.Concat(supersMembers).Concat(relatedMembers);
        return allMembers;
    }

    private IEnumerable<IDeclaration> InnerGetMembers(LuaType luaType)
    {
        if (luaType is LuaGenericType genericType)
        {
            return GetGenericMembers(genericType);
        }
        else if (luaType is LuaNamedType namedType)
        {
            var namedTypeKind = namedType.GetTypeKind(context);
            if (namedTypeKind == NamedTypeKind.Alias)
            {
                var originType = context.Compilation.Db.QueryAliasOriginTypes(namedType.Name);
                if (originType is not null)
                {
                    return GetMembers(originType);
                }
            }

            return GetNormalTypeMembers(namedType);
        }
        else if (luaType is LuaUnionType unionType)
        {
            // 这样是为了防止错误递归 ---@alias aaa aaa | global
            return unionType.UnionTypes.SelectMany(GetMembers).ToList();
        }
        else if (luaType is LuaTupleType tupleType)
        {
            return tupleType.TupleDeclaration;
        }

        return GetNormalTypeMembers(luaType);
    }

    public IEnumerable<IDeclaration> GetMembers(LuaType luaType)
    {
        if (MemberGuard.Add(luaType))
        {
            try
            {
                return InnerGetMembers(luaType);
            }
            finally
            {
                MemberGuard.Remove(luaType);
            }
        }

        return [];
    }

    private IEnumerable<IDeclaration> GetGenericMembers(LuaGenericType genericType)
    {
        if (context.Features.Cache && GenericMemberCaches.TryGetValue(genericType, out var instanceMembers))
        {
            return instanceMembers;
        }

        var members = GetNormalTypeMembers(genericType);
        var genericParams = context.Compilation.Db.QueryGenericParams(genericType.Name).ToList();
        var genericArgs = genericType.GenericArgs;

        var substitution = new TypeSubstitution();
        for (var i = 0; i < genericParams.Count && i < genericArgs.Count; i++)
        {
            substitution.Add(genericParams[i].Name, genericArgs[i], true);
        }

        instanceMembers = [];
        foreach (var member in members)
        {
            instanceMembers.Add(member.Instantiate(substitution));
        }

        if (context.Features.Cache)
        {
            GenericMemberCaches.TryAdd(genericType, instanceMembers);
        }

        return instanceMembers;
    }

    public IEnumerable<IDeclaration> FindMember(LuaType luaType, string memberName)
    {
        switch (luaType)
        {
            case LuaNamedType namedType when namedType is { Name: "table" }:
            {
                return FindTableMember(namedType, memberName);
            }
            case LuaNamedType namedType when namedType.GetTypeKind(context) == NamedTypeKind.Alias:
            {
                var originType = context.Compilation.Db.QueryAliasOriginTypes(namedType.Name);
                if (originType is not null)
                {
                    return FindMember(originType, memberName);
                }

                return [];
            }
            case LuaArrayType arrayType when memberName.StartsWith('['):
            {
                return [new LuaDeclaration(memberName, new VirtualInfo(arrayType.BaseType))];
            }
            default:
            {
                return GetMembers(luaType)
                    .Where(it => string.Equals(it.Name, memberName, StringComparison.CurrentCulture));
            }
        }
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
            var keyExprType = context.InferAndUnwrap(keyExpr);
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
        var members = GetSupersMembers(luaType);
        return members.Where(it => string.Equals(it.Name, member, StringComparison.CurrentCulture));
    }

    public void ClearMember(LuaType luaType)
    {
        TypeMemberCaches.Remove(luaType);
        GenericMemberCaches.Remove(luaType);
        BaseMemberCaches.Remove(luaType);
    }
}
