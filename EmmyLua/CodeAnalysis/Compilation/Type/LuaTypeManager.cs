using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Compilation.Type.TypeInfo;
using EmmyLua.CodeAnalysis.Compilation.Type.Types;
using EmmyLua.CodeAnalysis.Container;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Type;

public class LuaTypeManager(LuaCompilation compilation)
{
    private LuaCompilation Compilation { get; } = compilation;

    private NamespaceOrTypeInfo RootNamespace { get; } = new();

    private Dictionary<LuaDocumentId, NamespaceIndex> NamespaceIndices { get; } = new();

    private InFileIndex<SyntaxElementId, LuaLocalTypeInfo> LocalTypeInfos { get; } = new();

    private GlobalIndex GlobalIndices { get; } = new();

    private InFileIndex<string, LuaNamedType> GlobalProxyTypes { get; } = new();

    // left super, right sub
    private List<(LuaNamedType, LuaNamedType)> WaitBuildSubtypes { get; } = new();

    public LuaTypeInfo? FindTypeInfo(LuaNamedType type)
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

    public LuaLocalTypeInfo? FindElementTypeInfo(SyntaxElementId elementId)
    {
        return LocalTypeInfos.Query(elementId);
    }

    public LuaGlobalTypeInfo? FindGlobalTypeInfo(string name)
    {
        return GlobalIndices.Query(name);
    }

    public void Remove(LuaDocumentId documentId)
    {
        if (NamespaceIndices.Remove(documentId, out var namespaceIndex))
        {
            if (RootNamespace.FindNamespaceOrType(namespaceIndex.FullName) is { } namespaceInfo)
            {
                namespaceInfo.Remove(documentId, this);
                if (namespaceInfo.Children is null && namespaceInfo.TypeInfo is null)
                {
                    RootNamespace.RemoveChildNamespace(namespaceIndex.FullName);
                }
            }
        }
        else
        {
            RootNamespace.Remove(documentId, this);
        }

        LocalTypeInfos.Remove(documentId);
        GlobalIndices.Remove(documentId, this);
        GlobalProxyTypes.Remove(documentId);
    }

    public LuaTypeInfo? AddTypeDefinition(
        LuaDocTagNamedTypeSyntax element,
        string name,
        NamedTypeKind kind,
        LuaTypeAttribute attribute)
    {

        if (NamespaceIndices.TryGetValue(element.DocumentId, out var namespaceIndex))
        {
            var namespaceInfo = RootNamespace.FindOrCreate(namespaceIndex.FullName);
            var typeInfo = namespaceInfo.FindOrCreate(name);
            return typeInfo.CreateTypeInfo(name, element.UniqueId, kind, attribute);
        }
        else
        {
            var typeInfo = RootNamespace.FindOrCreate(name);
            return typeInfo.CreateTypeInfo(name, element.UniqueId, kind, attribute);
        }
    }

    public LuaLocalTypeInfo AddLocalTypeInfo(SyntaxElementId id)
    {
        if (LocalTypeInfos.Query(id) is { } typeInfo)
        {
            return typeInfo;
        }

        var newTypeInfo = new LuaLocalTypeInfo(id, );
        LocalTypeInfos.Add(id.DocumentId, id, newTypeInfo);
        return newTypeInfo;
    }

    public void AddGlobal(string name, LuaSymbol symbol)
    {
        GlobalIndices.AddGlobal(name, symbol);
    }

    public void SetExprType(LuaNamedType namedType, LuaType type)
    {
        if (type is LuaElementType elementType)
        {
            var elementTypeInfo = FindElementTypeInfo(elementType.Id);
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
            var typeInfo = FindTypeInfo(namedType);
            if (typeInfo is not null)
            {
                typeInfo.Attribute |= LuaTypeAttribute.Global;
            }

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

    public void BuildSubTypes()
    {
        foreach (var (left, right) in WaitBuildSubtypes)
        {
            if (FindTypeInfo(left) is { } leftTypeInfo)
            {
                leftTypeInfo.SubTypes ??= new();
                leftTypeInfo.SubTypes.Add(right);
            }
        }

        WaitBuildSubtypes.Clear();
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

    public IEnumerable<LuaGlobalTypeInfo> GetAllGlobalInfos()
    {
        return GlobalIndices.QueryAll();
    }

    public record struct NamespaceOrType(string Name, bool IsNamespace, NamedTypeKind Kind, SyntaxElementId Id);

    public IEnumerable<NamespaceOrType> GetNamespaceOrTypeInfos(string prefixNamespace, LuaDocumentId documentId)
    {
        if (documentId != LuaDocumentId.VirtualDocumentId)
        {
            if (NamespaceIndices.TryGetValue(documentId, out var namespaceIndex))
            {
                if (namespaceIndex.FullName.Length != 0)
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
                                    child.TypeInfo?.ElementId ?? SyntaxElementId.Empty
                                );
                            }
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
                                    child.TypeInfo?.ElementId ?? SyntaxElementId.Empty
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
                    child.TypeInfo?.ElementId ?? SyntaxElementId.Empty
                );
            }
        }
    }

    public NamespaceOrType? FindNamespaceOrType(string fullName, string member)
    {
        if (RootNamespace.FindNamespaceOrType(fullName) is { } namespaceInfo)
        {
            if (namespaceInfo.FindNamespaceOrType(member) is { } child)
            {
                return new NamespaceOrType(member, child.TypeInfo is null, child.TypeInfo?.Kind ?? NamedTypeKind.None,
                    child.TypeInfo?.ElementId ?? SyntaxElementId.Empty);
            }
        }

        return null;
    }

    public bool HasNamespace(LuaDocumentId documentId)
    {
        return NamespaceIndices.ContainsKey(documentId);
    }
}
