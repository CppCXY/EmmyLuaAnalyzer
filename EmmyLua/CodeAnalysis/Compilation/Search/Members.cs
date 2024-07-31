using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Type;
using EmmyLua.CodeAnalysis.Type.Manager.TypeInfo;


namespace EmmyLua.CodeAnalysis.Compilation.Search;

public class Members(SearchContext context)
{
    private HashSet<ITypeInfo> _visitedTypes = new();

    public List<LuaSymbol> GetTypeMembers(LuaType luaType)
    {
        return luaType switch
        {
            LuaGenericType genericType => GetGenericTypeMembers(genericType),
            LuaNamedType namedType => GetNamedTypeMembers(namedType),
            LuaElementType elementType => GetElementTypeMembers(elementType),
            GlobalNameType globalType => GetGlobalTypeMembers(globalType),
            LuaUnionType unionType => unionType.UnionTypes.SelectMany(GetTypeMembers).ToList(),
            LuaTupleType tupleType => GetTupleTypeMembers(tupleType),
            _ => []
        };
    }

    private List<LuaSymbol> GetNamedTypeMembers(LuaNamedType namedType)
    {
        var typeInfo = context.Compilation.TypeManager.FindTypeInfo(namedType);
        if (typeInfo is null)
        {
            return [];
        }

        if (!_visitedTypes.Add(typeInfo))
        {
            return [];
        }

        var members = new Dictionary<string, LuaSymbol>();
        try
        {
            switch (typeInfo.Kind)
            {
                case NamedTypeKind.Class:
                {
                    if (typeInfo.Declarations is { } declarations)
                    {
                        foreach (var (name, symbol) in declarations)
                        {
                            members.TryAdd(name, symbol);
                        }
                    }

                    break;
                }
                case NamedTypeKind.Interface:
                {
                    if (typeInfo.Declarations is { } declarations)
                    {
                        foreach (var (name, symbol) in declarations)
                        {
                            members.TryAdd(name, symbol);
                        }
                    }

                    break;
                }
                case NamedTypeKind.Enum:
                {
                    if (typeInfo.Declarations is { } declarations)
                    {
                        foreach (var (name, symbol) in declarations)
                        {
                            members.TryAdd(name, symbol);
                        }
                    }

                    break;
                }
                case NamedTypeKind.Alias:
                {
                    if (typeInfo.BaseType is not null)
                    {
                        var originMembers = GetTypeMembers(typeInfo.BaseType);
                        foreach (var member in originMembers)
                        {
                            members.TryAdd(member.Name, member);
                        }
                    }

                    break;
                }
            }

            // base
            if (typeInfo.Kind is NamedTypeKind.Class && typeInfo.BaseType is not null)
            {
                var baseMembers = GetTypeMembers(typeInfo.BaseType);
                foreach (var member in baseMembers)
                {
                    members.TryAdd(member.Name, member);
                }
            }

            // supers
            if (typeInfo.Kind is NamedTypeKind.Class or NamedTypeKind.Interface && typeInfo.Supers is { } supers)
            {
                foreach (var super in supers)
                {
                    var superMembers = GetTypeMembers(super);
                    foreach (var member in superMembers)
                    {
                        members.TryAdd(member.Name, member);
                    }
                }
            }
        }
        finally
        {
            _visitedTypes.Remove(typeInfo);
        }

        return members.Values.ToList();
    }

    private List<LuaSymbol> GetElementTypeMembers(LuaElementType elementType)
    {
        var typeInfo = context.Compilation.TypeManager.FindTypeInfo(elementType.Id);
        if (typeInfo is null)
        {
            return [];
        }

        if (!_visitedTypes.Add(typeInfo))
        {
            return [];
        }

        var members = new Dictionary<string, LuaSymbol>();
        try
        {
            if (typeInfo.Declarations is { } declarations)
            {
                foreach (var (name, symbol) in declarations)
                {
                    members.TryAdd(name, symbol);
                }
            }

            if (typeInfo.BaseType is not null)
            {
                var baseMembers = GetTypeMembers(typeInfo.BaseType);
                foreach (var member in baseMembers)
                {
                    members.TryAdd(member.Name, member);
                }
            }
        }
        finally
        {
            _visitedTypes.Remove(typeInfo);
        }

        return members.Values.ToList();
    }

    private List<LuaSymbol> GetGlobalTypeMembers(GlobalNameType globalType)
    {
        var namedType = context.Compilation.TypeManager.GetGlobalProxyNameType(globalType.Name);
        if (namedType is not null)
        {
            return GetTypeMembers(namedType);
        }

        var typeInfo = context.Compilation.TypeManager.FindGlobalInfo(globalType.Name);
        if (typeInfo is null)
        {
            return [];
        }

        if (!_visitedTypes.Add(typeInfo))
        {
            return [];
        }

        var members = new Dictionary<string, LuaSymbol>();
        try
        {
            if (typeInfo.Declarations is { } declarations)
            {
                foreach (var (name, symbol) in declarations)
                {
                    members.TryAdd(name, symbol);
                }
            }

            if (typeInfo.BaseType is not null)
            {
                var baseMembers = GetTypeMembers(typeInfo.BaseType);
                foreach (var member in baseMembers)
                {
                    members.TryAdd(member.Name, member);
                }
            }
        }
        finally
        {
            _visitedTypes.Remove(typeInfo);
        }

        return members.Values.ToList();
    }

    private List<LuaSymbol> GetTupleTypeMembers(LuaTupleType tupleType)
    {
        var members = new List<LuaSymbol>();
        members.AddRange(tupleType.TupleDeclaration);
        return members;
    }

    private List<LuaSymbol> GetGenericTypeMembers(LuaGenericType genericType)
    {
        if (genericType.Name == "table")
        {
            return [];
        }

        var typeInfo = context.Compilation.TypeManager.FindTypeInfo(genericType);
        if (typeInfo is null)
        {
            return [];
        }

        if (!_visitedTypes.Add(typeInfo))
        {
            return [];
        }

        var members = new Dictionary<string, LuaSymbol>();
        try
        {
            if (typeInfo.Kind is NamedTypeKind.Class or NamedTypeKind.Interface)
            {
                var substitute = new TypeSubstitution();
                var genericArgs = genericType.GenericArgs;
                if (typeInfo.GenericParams is not null)
                {
                    for (var i = 0; i < typeInfo.GenericParams.Count && i < genericArgs.Count; i++)
                    {
                        substitute.Add(typeInfo.GenericParams[i].Name , genericArgs[i], true);
                    }
                }

                if (typeInfo.Declarations is { } declarations)
                {
                    foreach (var (name, symbol) in declarations)
                    {
                        members.TryAdd(name, symbol.Instantiate(substitute));
                    }
                }

                if (typeInfo.BaseType is not null)
                {
                    var baseMembers = GetTypeMembers(typeInfo.BaseType);
                    foreach (var member in baseMembers)
                    {
                        members.TryAdd(member.Name, member);
                    }
                }

                if (typeInfo.Supers is { } supers)
                {
                    foreach (var super in supers)
                    {
                        var superMembers = GetTypeMembers(super);
                        foreach (var member in superMembers)
                        {
                            members.TryAdd(member.Name, member);
                        }
                    }
                }
            }
        }
        finally
        {
            _visitedTypes.Remove(typeInfo);
        }

        return members.Values.ToList();
    }

    public List<LuaSymbol> GetSuperMembers(LuaNamedType namedType)
    {
        var typeInfo = context.Compilation.TypeManager.FindTypeInfo(namedType);
        if (typeInfo is null)
        {
            return [];
        }

        if (!_visitedTypes.Add(typeInfo))
        {
            return [];
        }

        var members = new Dictionary<string, LuaSymbol>();
        try
        {
            if (typeInfo.Kind is NamedTypeKind.Class or NamedTypeKind.Interface)
            {
                if (typeInfo.Supers is { } supers)
                {
                    foreach (var super in supers)
                    {
                        var superMembers = GetTypeMembers(super);
                        foreach (var member in superMembers)
                        {
                            members.TryAdd(member.Name, member);
                        }
                    }
                }
            }
        }
        finally
        {
            _visitedTypes.Remove(typeInfo);
        }

        return members.Values.ToList();
    }


}
