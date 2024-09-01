using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Compilation.Type.TypeInfo;
using EmmyLua.CodeAnalysis.Compilation.Type.Types;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;


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
        if (namedType.IsSameType(Builtin.Global, context))
        {
            var list = new List<LuaSymbol>();
            foreach (var globalInfo in context.Compilation.TypeManager.GetAllGlobalInfos())
            {
                if (globalInfo.MainLuaSymbol is { } mainLuaSymbol)
                {
                    list.Add(mainLuaSymbol);
                }
            }

            return list;
        }

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

        var typeInfo = context.Compilation.TypeManager.FindTypeInfo(globalType.Name);
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
        return tupleType.TypeList.Select(VirtualInfo.CreateTypeSymbol).ToList();
    }

    private List<LuaSymbol> GetGenericTypeMembers(LuaGenericType genericType)
    {
        if (genericType.Name == "table")
        {
            return [];
        }
        else if (genericType.Name == "namespace" &&
                 genericType.GenericArgs.FirstOrDefault() is LuaStringLiteralType namespaceString)
        {
            var namespaces = new List<LuaSymbol>();
            var namespaceOrTypeIndexs = context.Compilation.TypeManager.GetNamespaceOrTypeInfos(
                namespaceString.Content, LuaDocumentId.VirtualDocumentId);
            foreach (var namespaceOrTypeInfo in namespaceOrTypeIndexs)
            {
                if (namespaceOrTypeInfo.IsNamespace)
                {
                    var namespaceType = new LuaGenericType(LuaDocumentId.VirtualDocumentId, "namespace", [
                        new LuaStringLiteralType($"{namespaceString.Content}.{namespaceOrTypeInfo.Name}")
                    ]);
                    namespaces.Add(new LuaSymbol(namespaceOrTypeInfo.Name,
                        namespaceType,
                        new NamespaceInfo()
                    ));
                }
                else
                {
                    var namedType = new LuaNamedType(LuaDocumentId.VirtualDocumentId,
                        $"{namespaceString.Content}.{namespaceOrTypeInfo.Name}");
                    namespaces.Add(new LuaSymbol(namespaceOrTypeInfo.Name, namedType, new NamedTypeInfo(
                        new LuaElementPtr<LuaDocTagNamedTypeSyntax>(namespaceOrTypeInfo.Id),
                        namespaceOrTypeInfo.Kind
                    )));
                }
            }

            return namespaces;
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
                if (typeInfo.GenericParameters is not null)
                {
                    for (var i = 0; i < typeInfo.GenericParameters.Count && i < genericArgs.Count; i++)
                    {
                        substitute.Add(typeInfo.GenericParameters[i].Name, genericArgs[i], true);
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
