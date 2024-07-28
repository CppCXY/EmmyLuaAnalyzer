using EmmyLua.CodeAnalysis.Compilation;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Container;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Type.Manager.TypeInfo;

namespace EmmyLua.CodeAnalysis.Type.Manager;

public class LuaTypeManager(LuaCompilation compilation)
{
    private LuaCompilation Compilation { get; } = compilation;

    private NamespaceOrTypeInfo RootNamespace { get; } = new();

    private Dictionary<LuaDocumentId, NamespaceIndex> NamespaceIndices { get; } = new();

    private InFileIndex<SyntaxElementId, DocumentElementTypeInfo> DocumentElementTypeInfos { get; } = new();

    private GlobalIndex GlobalIndices { get; } = new();

    private InFileIndex<string, LuaNamedType> GlobalProxyTypes { get; } = new();

    public TypeInfo.TypeInfo? FindTypeInfo(LuaNamedType type)
    {
        if (type.DocumentId != LuaDocumentId.VirtualDocumentId)
        {
            if (NamespaceIndices.TryGetValue(type.DocumentId, out var namespaceIndex))
            {
                if (RootNamespace.FindNamespaceOrType(namespaceIndex.FullName) is { } namespaceInfo)
                {
                    if (namespaceInfo.FindNamespaceOrType(type.Name)?.TypeInfo is { } typeInfo)
                    {
                        return typeInfo;
                    }
                }

                foreach (var usingNamespace in namespaceIndex.UsingNamespaces)
                {
                    if (RootNamespace.FindNamespaceOrType(usingNamespace) is { } usingNamespaceInfo)
                    {
                        if (usingNamespaceInfo.FindNamespaceOrType(type.Name)?.TypeInfo is { } typeInfo)
                        {
                            return typeInfo;
                        }
                    }
                }
            }
        }

        return RootNamespace.FindNamespaceOrType(type.Name)?.TypeInfo;
    }

    public DocumentElementTypeInfo? FindTypeInfo(SyntaxElementId elementId)
    {
        return DocumentElementTypeInfos.Query(elementId);
    }

    public GlobalTypeInfo? FindGlobalInfo(string name)
    {
        return GlobalIndices.Query(name);
    }

    public void Remove(LuaDocumentId documentId)
    {
        if (NamespaceIndices.Remove(documentId, out var namespaceIndex))
        {
            if (RootNamespace.FindNamespaceOrType(namespaceIndex.FullName) is { } namespaceInfo)
            {
                namespaceInfo.Remove(documentId);
            }
        }
        else
        {
            RootNamespace.Remove(documentId);
        }

        DocumentElementTypeInfos.Remove(documentId);
        GlobalIndices.Remove(documentId);
        GlobalProxyTypes.Remove(documentId);
    }

    public bool AddTypeDefinition(LuaDocTagNamedTypeSyntax element, string name, NamedTypeKind kind,
        LuaTypeAttribute attribute)
    {
        if (NamespaceIndices.TryGetValue(element.DocumentId, out var namespaceIndex))
        {
            var namespaceInfo = RootNamespace.FindOrCreate(namespaceIndex.FullName);
            var typeInfo = namespaceInfo.FindOrCreate(name);
            return typeInfo.CreateTypeInfo(element.UniqueId, kind, attribute);
        }
        else
        {
            var typeInfo = RootNamespace.FindOrCreate(name);
            return typeInfo.CreateTypeInfo(element.UniqueId, kind, attribute);
        }
    }

    public void AddDocumentElementType(SyntaxElementId id)
    {
        var typeInfo = new DocumentElementTypeInfo()
        {
            DocumentId = id.DocumentId
        };

        DocumentElementTypeInfos.Add(id.DocumentId, id, typeInfo);
    }

    public void AddGlobal(string name, LuaSymbol symbol)
    {
        GlobalIndices.AddGlobal(name, symbol);
    }

    public void SetBaseType(LuaNamedType type, LuaType baseType)
    {
        var typeInfo = FindTypeInfo(type);
        if (typeInfo is null)
        {
            return;
        }

        if (typeInfo.ResolvedMainDocumentId && typeInfo.MainDocumentId != type.DocumentId)
        {
            return;
        }

        typeInfo.ResolvedMainDocumentId = true;
        typeInfo.MainDocumentId = type.DocumentId;

        typeInfo.BaseType = baseType;
    }

    public void SetBaseType(SyntaxElementId elementId, LuaType baseType)
    {
        if (DocumentElementTypeInfos.Query(elementId) is { } typeInfo)
        {
            typeInfo.BaseType = baseType;
        }
    }


    public void SetExprType(LuaNamedType namedType, LuaType type)
    {
        if (type is LuaElementType elementType)
        {
            var elementTypeInfo = FindTypeInfo(elementType.Id);
            if (elementTypeInfo?.Declarations is { } members)
            {
                AddMemberImplementations(namedType, members.Values);
            }
        }
    }

    public void SetExprType(LuaDocumentId documentId, GlobalNameType globalNameType, LuaType baseType)
    {
        if (GlobalProxyTypes.Query(globalNameType.Name) is { } namedType)
        {
            SetExprType(namedType, baseType);
        }
        else
        {
            SetGlobalBaseType(documentId, globalNameType, baseType);
        }
    }

    public void SetGlobalTypeSymbol(string name, LuaNamedType namedType)
    {
        if (GlobalIndices.Query(name) is { } globalTypeInfo)
        {
            globalTypeInfo.MainDocumentId = namedType.DocumentId;
            GlobalProxyTypes.Add(namedType.DocumentId, name, namedType);
            if (globalTypeInfo.Declarations is { } members)
            {
                AddMemberImplementations(namedType, members.Values);
            }
        }
    }

    public void SetGlobalBaseType(LuaDocumentId documentId, GlobalNameType globalNameType, LuaType baseType)
    {
        if (GlobalIndices.Query(globalNameType.Name) is { BaseType: null } globalTypeInfo)
        {
            globalTypeInfo.MainDocumentId = documentId;
            globalTypeInfo.BaseType = baseType;
        }
    }

    public void AddSupers(LuaNamedType type, IEnumerable<LuaNamedType> supers)
    {
        var typeInfo = FindTypeInfo(type);
        if (typeInfo is null)
        {
            return;
        }

        if (typeInfo.ResolvedMainDocumentId && typeInfo.MainDocumentId != type.DocumentId)
        {
            return;
        }

        typeInfo.ResolvedMainDocumentId = true;
        typeInfo.MainDocumentId = type.DocumentId;

        typeInfo.Supers ??= new();
        typeInfo.Supers.AddRange(supers);
    }

    public void AddMemberDeclarations(LuaNamedType type, IEnumerable<LuaSymbol> members)
    {
        var typeInfo = FindTypeInfo(type);
        if (typeInfo is null)
        {
            return;
        }

        typeInfo.Declarations ??= new();
        foreach (var member in members)
        {
            if (typeInfo.IsDefinedInDocument(member.DocumentId))
            {
                typeInfo.Declarations[member.Name] = member;
            }
        }
    }

    public void AddMemberImplementations(LuaNamedType type, IEnumerable<LuaSymbol> members)
    {
        var typeInfo = FindTypeInfo(type);
        if (typeInfo is null)
        {
            return;
        }

        typeInfo.Declarations ??= new();
        typeInfo.Implements ??= new();
        foreach (var member in members)
        {
            if (typeInfo.IsDefinedInDocument(member.DocumentId))
            {
                if (typeInfo.Exact)
                {
                    if (typeInfo.Declarations.ContainsKey(member.Name))
                    {
                        typeInfo.Implements.TryAdd(member.Name, member);
                    }
                }
                else
                {
                    typeInfo.Implements.TryAdd(member.Name, member);
                    typeInfo.Declarations.TryAdd(member.Name, member);
                }
            }
        }
    }

    public void AddMemberImplementation(LuaNamedType type, LuaSymbol member)
    {
        var typeInfo = FindTypeInfo(type);
        if (typeInfo is null)
        {
            return;
        }

        if (typeInfo.IsDefinedInDocument(member.DocumentId))
        {
            typeInfo.Declarations ??= new();
            typeInfo.Implements ??= new();
            if (typeInfo.Exact)
            {
                if (typeInfo.Declarations.ContainsKey(member.Name))
                {
                    typeInfo.Implements.TryAdd(member.Name, member);
                }
            }
            else
            {
                typeInfo.Implements.TryAdd(member.Name, member);
                typeInfo.Declarations.TryAdd(member.Name, member);
            }
        }
    }

    public void AddElementMembers(SyntaxElementId elementId, IEnumerable<LuaSymbol> members)
    {
        if (DocumentElementTypeInfos.Query(elementId) is { } typeInfo)
        {
            typeInfo.Declarations ??= new();
            foreach (var member in members)
            {
                typeInfo.Declarations.TryAdd(member.Name, member);
            }
        }
    }

    public void AddGlobalMember(string name, LuaSymbol member)
    {
        GlobalIndices.AddGlobalMember(name, member);
        if (GlobalProxyTypes.Query(name) is { } namedType)
        {
            AddMemberImplementation(namedType, member);
        }
    }

    public void AddElementMember(SyntaxElementId elementId, LuaSymbol member)
    {
        if (DocumentElementTypeInfos.Query(elementId) is { } typeInfo)
        {
            if (member.DocumentId == elementId.DocumentId)
            {
                typeInfo.Declarations ??= new();
                typeInfo.Declarations.TryAdd(member.Name, member);
            }
        }
    }

    public void AddOperators(LuaNamedType type, IEnumerable<TypeOperator> operators)
    {
        var typeInfo = FindTypeInfo(type);
        if (typeInfo is null)
        {
            return;
        }

        typeInfo.Operators ??= new();
        foreach (var op in operators)
        {
            if (typeInfo.IsDefinedInDocument(op.LuaSymbol.DocumentId))
            {
                if (!typeInfo.Operators.TryGetValue(op.Kind, out var list))
                {
                    list = new();
                    typeInfo.Operators[op.Kind] = list;
                }

                list.Add(op);
            }
        }
    }

    public void AddOverloads(LuaNamedType type, IEnumerable<TypeInfo.TypeInfo.OverloadStub> overloads)
    {
        var typeInfo = FindTypeInfo(type);
        if (typeInfo is null)
        {
            return;
        }

        typeInfo.Overloads ??= new();
        foreach (var overload in overloads)
        {
            if (typeInfo.IsDefinedInDocument(overload.DocumentId))
            {
                typeInfo.Overloads.Add(overload);
            }
        }
    }

    public void AddGenericParams(LuaNamedType type, IEnumerable<LuaSymbol> genericParams)
    {
        var typeInfo = FindTypeInfo(type);
        if (typeInfo is null)
        {
            return;
        }

        if (typeInfo.ResolvedMainDocumentId && typeInfo.MainDocumentId != type.DocumentId)
        {
            return;
        }

        typeInfo.ResolvedMainDocumentId = true;
        typeInfo.MainDocumentId = type.DocumentId;

        typeInfo.GenericParams ??= new();
        typeInfo.GenericParams.AddRange(genericParams);
    }

    public void SetNamespace(LuaDocumentId documentId, string fullName)
    {
        var namespaceIndex = new NamespaceIndex()
        {
            FullName = fullName
        };
        NamespaceIndices[documentId] = namespaceIndex;
    }

    public void AddUsingNamespace(LuaDocumentId documentId, string usingNamespace)
    {
        if (NamespaceIndices.TryGetValue(documentId, out var namespaceIndex))
        {
            namespaceIndex.UsingNamespaces.Add(usingNamespace);
        }
        else
        {
            namespaceIndex = new NamespaceIndex()
            {
                FullName = string.Empty
            };
            namespaceIndex.UsingNamespaces.Add(usingNamespace);
            NamespaceIndices[documentId] = namespaceIndex;
        }
    }

    public LuaType? GetBaseType(SyntaxElementId id)
    {
        if (DocumentElementTypeInfos.Query(id) is { } typeInfo)
        {
            return typeInfo.BaseType;
        }

        return null;
    }

    public IEnumerable<GlobalTypeInfo> GetAllGlobalInfos()
    {
        return GlobalIndices.QueryAll();
    }

    public LuaSymbol? GetGlobalSymbol(string name)
    {
        var globalInfo = GlobalIndices.Query(name);
        if (globalInfo?.DefinedDeclarations.TryGetValue(globalInfo.MainDocumentId, out var symbol) == true)
        {
            var proxyType = GlobalProxyTypes.Query(name);
            if (proxyType is not null)
            {
                return new LuaSymbol(symbol.Name, proxyType, symbol.Info);
            }

            return symbol;
        }

        return null;
    }

    public LuaSymbol? GetTypeDefinedSymbol(LuaNamedType namedType)
    {
        var typeInfo = FindTypeInfo(namedType);
        if (typeInfo is null)
        {
            return null;
        }

        var id = SyntaxElementId.Empty;
        if (typeInfo.DefinedElementIds.Count == 1)
        {
            id = typeInfo.DefinedElementIds.First();
        }
        else if (typeInfo.DefinedElementIds?.FirstOrDefault(it => it.DocumentId == typeInfo.MainDocumentId) is
                 { } elementId)
        {
            id = elementId;
        }

        var info = new NamedTypeInfo(new LuaElementPtr<LuaDocTagNamedTypeSyntax>(id), typeInfo.Kind);
        return new LuaSymbol(typeInfo.Name, namedType, info);
    }

    public LuaNamedType? GetGlobalProxyNameType(string name)
    {
        if (GlobalProxyTypes.Query(name) is { } namedType)
        {
            return namedType;
        }

        return null;
    }

    public record struct NamespaceOrType(string Name, bool IsNamespace, NamedTypeKind Kind, SyntaxElementId Id);

    public IEnumerable<NamespaceOrType> GetNamespaceOrTypeInfos(string prefix, LuaDocumentId documentId)
    {
        var prefixNamespace = string.Empty;
        var dotIndex = prefix.LastIndexOf('.');
        if (dotIndex != -1)
        {
            prefixNamespace = prefix[..dotIndex];
        }

        if (documentId != LuaDocumentId.VirtualDocumentId)
        {
            if (NamespaceIndices.TryGetValue(documentId, out var namespaceIndex))
            {
                if (RootNamespace.FindNamespaceOrType(namespaceIndex.FullName) is { } namespaceInfo)
                {
                    if (namespaceInfo.FindNamespaceOrType(prefixNamespace) is { Children: { } children1 })
                    {
                        foreach (var (name, child) in children1)
                        {
                            yield return new NamespaceOrType(
                                name,
                                child.TypeInfo is null,
                                child.TypeInfo?.Kind ?? NamedTypeKind.None,
                                child.TypeInfo?.MainElementId ?? SyntaxElementId.Empty
                            );
                        }
                    }
                }

                foreach (var usingNamespace in namespaceIndex.UsingNamespaces)
                {
                    if (RootNamespace.FindNamespaceOrType(usingNamespace) is { } usingNamespaceInfo)
                    {
                        if (usingNamespaceInfo.FindNamespaceOrType(prefixNamespace) is { Children: { } children2 })
                        {
                            foreach (var (name, child) in children2)
                            {
                                yield return new NamespaceOrType(
                                    name,
                                    child.TypeInfo is null,
                                    child.TypeInfo?.Kind ?? NamedTypeKind.None,
                                    child.TypeInfo?.MainElementId ?? SyntaxElementId.Empty
                                );
                            }
                        }
                    }
                }
            }
        }

        if (RootNamespace.FindNamespaceOrType(prefixNamespace) is { Children: { } children3 })
        {
            foreach (var (name, child) in children3)
            {
                yield return new NamespaceOrType(
                    name,
                    child.TypeInfo is null,
                    child.TypeInfo?.Kind ?? NamedTypeKind.None,
                    child.TypeInfo?.MainElementId ?? SyntaxElementId.Empty
                );
            }
        }
    }
}
