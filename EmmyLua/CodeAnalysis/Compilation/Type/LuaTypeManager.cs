using EmmyLua.CodeAnalysis.Compilation.Index;
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

    private InFileIndex<SyntaxElementId, LuaType> RefDetailType { get; } = new();

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

    public LuaLocalTypeInfo? FindTypeInfo(SyntaxElementId elementId)
    {
        return LocalTypeInfos.Query(elementId);
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
        RefDetailType.Remove(documentId);
        RefDetailType.Remove(documentId);
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
            return typeInfo.CreateTypeInfo(element.UniqueId, kind, attribute);
        }
        else
        {
            var typeInfo = RootNamespace.FindOrCreate(name);
            return typeInfo.CreateTypeInfo(element.UniqueId, kind, attribute);
        }
    }

    public LuaLocalTypeInfo AddLocalTypeInfo(SyntaxElementId id)
    {
        if (LocalTypeInfos.Query(id) is { } typeInfo)
        {
            return typeInfo;
        }

        var newTypeInfo = new LuaLocalTypeInfo(id, NamedTypeKind.None, LuaTypeAttribute.Exact);
        LocalTypeInfos.Add(id.DocumentId, id, newTypeInfo);
        return newTypeInfo;
    }

    public LuaTypeInfo? AddTypeComputer(LuaDocTagClassSyntax docTagClassSyntax, string name)
    {
        if (NamespaceIndices.TryGetValue(docTagClassSyntax.DocumentId, out var namespaceIndex))
        {
            var namespaceInfo = RootNamespace.FindOrCreate(namespaceIndex.FullName);
            var typeInfo = namespaceInfo.FindOrCreate(name);
            return typeInfo.CreateComputerTypeInfo(docTagClassSyntax.UniqueId, docTagClassSyntax);
        }
        else
        {
            var typeInfo = RootNamespace.FindOrCreate(name);
            return typeInfo.CreateComputerTypeInfo(docTagClassSyntax.UniqueId, docTagClassSyntax);
        }
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

    public record struct NamespaceOrType(string Name, bool IsNamespace, NamedTypeKind Kind);

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
                                    child.TypeInfo?.Kind ?? NamedTypeKind.None
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
                                    child.TypeInfo?.Kind ?? NamedTypeKind.None
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
                    child.TypeInfo?.Kind ?? NamedTypeKind.None
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
                return new NamespaceOrType(member, child.TypeInfo is null, child.TypeInfo?.Kind ?? NamedTypeKind.None);
            }
        }

        return null;
    }

    public bool HasNamespace(LuaDocumentId documentId)
    {
        return NamespaceIndices.ContainsKey(documentId);
    }

    public void AddRefDetailType(SyntaxElementId id, LuaType type)
    {
        RefDetailType.Add(id.DocumentId, id, type);
    }

    public LuaType? GetRefDetailType(SyntaxElementId id)
    {
        return RefDetailType.Query(id);
    }
}
