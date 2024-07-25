using EmmyLua.CodeAnalysis.Compilation;
using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Container;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Type.Manager;

public class LuaTypeManager(LuaCompilation compilation)
{
    private LuaCompilation Compilation { get; } = compilation;

    private NamespaceOrTypeInfo RootNamespace { get; } = new();

    private Dictionary<LuaDocumentId, NamespaceIndex> NamespaceIndices { get; } = new();

    private InFileIndex<SyntaxElementId, ElementTypeInfo> ElementTypeInfos { get; } = new();

    private Dictionary<string, GlobalTypeInfo> GlobalInfos { get; } = new();

    private Dictionary<LuaDocumentId, List<string>> DocumentGlobalNames { get; } = new();

    public TypeInfo? FindTypeInfo(LuaNamedType type)
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

    public ElementTypeInfo? FindTypeInfo(SyntaxElementId elementId)
    {
        return ElementTypeInfos.Query(elementId.DocumentId, elementId);
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

        ElementTypeInfos.Remove(documentId);

        if (DocumentGlobalNames.Remove(documentId, out var globalNames))
        {
            foreach (var globalName in globalNames)
            {
                if (GlobalInfos.TryGetValue(globalName, out var globalInfo))
                {
                    if (globalInfo.RemovePartial(documentId))
                    {
                        GlobalInfos.Remove(globalName);
                    }
                }
            }
        }
    }

    public bool AddTypeDefinition(LuaDocTagNamedTypeSyntax element, string name, NamedTypeKind kind, LuaTypeAttribute attribute)
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

    public void AddElementType(SyntaxElementId id)
    {
        var typeInfo = new ElementTypeInfo()
        {
            MainDocumentId = id.DocumentId
        };

        ElementTypeInfos.Add(id.DocumentId, id, typeInfo);
    }

    public void AddGlobal(SyntaxElementId id, string name)
    {
        if (GlobalInfos.TryGetValue(name, out var globalInfo))
        {
            globalInfo.DefinedElementIds.Add(id);
        }
        else
        {
            globalInfo = new GlobalTypeInfo()
            {
                Name = name
            };
            globalInfo.DefinedElementIds.Add(id);
            GlobalInfos[name] = globalInfo;
        }

        if (DocumentGlobalNames.TryGetValue(id.DocumentId, out var globalNames))
        {
            globalNames.Add(name);
        }
        else
        {
            globalNames = [name];
            DocumentGlobalNames[id.DocumentId] = globalNames;
        }
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
        if (ElementTypeInfos.Query(elementId.DocumentId, elementId) is { } typeInfo)
        {
            typeInfo.BaseType = baseType;
        }
    }

    public void SetGlobalType(SyntaxElementId elementId, string name, LuaType baseType)
    {
        if (GlobalInfos.TryGetValue(name, out var globalTypeInfo))
        {
            globalTypeInfo.MainDocumentId = elementId.DocumentId;
            globalTypeInfo.DefinedElementIds.Add(elementId);
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

    public void AddMemberDeclarations(LuaNamedType type, IEnumerable<LuaDeclaration> members)
    {
        var typeInfo = FindTypeInfo(type);
        if (typeInfo is null)
        {
            return;
        }

        typeInfo.Declarations ??= new();
        foreach (var member in members)
        {
            typeInfo.Declarations[member.Name] = member;
        }
    }

    public void AddMemberImplementations(LuaNamedType type, IEnumerable<LuaDeclaration> members)
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

    public void AddElementMember(SyntaxElementId elementId, IEnumerable<LuaDeclaration> members)
    {
        if (ElementTypeInfos.Query(elementId.DocumentId, elementId) is { } typeInfo)
        {
            typeInfo.Declarations ??= new();
            foreach (var member in members)
            {
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
        typeInfo.Operators.AddRange(operators);
    }

    public void AddOverloads(LuaNamedType type, IEnumerable<TypeInfo.OverloadStub> overloads)
    {
        var typeInfo = FindTypeInfo(type);
        if (typeInfo is null)
        {
            return;
        }

        typeInfo.Overloads ??= new();
        typeInfo.Overloads.AddRange(overloads);
    }

    public void AddGenericParams(LuaNamedType type, IEnumerable<LuaDeclaration> genericParams)
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
    }
}
