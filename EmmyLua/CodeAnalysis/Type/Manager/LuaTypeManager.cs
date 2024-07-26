using EmmyLua.CodeAnalysis.Compilation;
using EmmyLua.CodeAnalysis.Compilation.Declaration;
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

    private InFileIndex<SyntaxElementId, DocumentElementTypeInfo> ElementTypeInfos { get; } = new();

    private Dictionary<string, GlobalTypeInfo> GlobalInfos { get; } = new();

    private Dictionary<LuaDocumentId, List<string>> DocumentGlobalNames { get; } = new();

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

    public void AddElementType(SyntaxElementId id)
    {
        var typeInfo = new DocumentElementTypeInfo()
        {
            DocumentId = id.DocumentId
        };

        ElementTypeInfos.Add(id.DocumentId, id, typeInfo);
    }

    public void AddGlobal(string name, LuaSymbol symbol)
    {
        if (GlobalInfos.TryGetValue(name, out var globalInfo))
        {
            globalInfo.DefinedDeclarations.Add(symbol.DocumentId, symbol);
        }
        else
        {
            globalInfo = new GlobalTypeInfo()
            {
                Name = name
            };
            globalInfo.DefinedDeclarations.Add(symbol.DocumentId, symbol);
            GlobalInfos[name] = globalInfo;
        }

        if (DocumentGlobalNames.TryGetValue(symbol.DocumentId, out var globalNames))
        {
            globalNames.Add(name);
        }
        else
        {
            globalNames = [name];
            DocumentGlobalNames[symbol.DocumentId] = globalNames;
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

    public void SetGlobalProxyType(string name, LuaNamedType type)
    {
        if (GlobalInfos.TryGetValue(name, out var globalTypeInfo))
        {
            // globalTypeInfo.MainDocumentId = symbol.DocumentId;
            // globalTypeInfo.DefinedDeclarations.TryAdd(symbol.DocumentId, symbol);
        }
    }

    public void SetGlobalBaseType(string name, LuaType baseType)
    {
        if (GlobalInfos.TryGetValue(name, out var globalTypeInfo))
        {
            // globalTypeInfo.BaseType = baseType;
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
        if (ElementTypeInfos.Query(elementId.DocumentId, elementId) is { } typeInfo)
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
        if (GlobalInfos.TryGetValue(name, out var globalInfo))
        {
            globalInfo.Declarations ??= new();
            globalInfo.Declarations.TryAdd(member.Name, member);
        }
    }

    public void AddElementMember(SyntaxElementId elementId, LuaSymbol member)
    {
        if (ElementTypeInfos.Query(elementId.DocumentId, elementId) is { } typeInfo)
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
                typeInfo.Operators.Add(op);
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
    }

    public LuaType? GetBaseType(LuaNamedType type)
    {
        var typeInfo = FindTypeInfo(type);
        return typeInfo?.BaseType;
    }

    public LuaType? GetBaseType(SyntaxElementId id)
    {
        if (ElementTypeInfos.Query(id.DocumentId, id) is { } typeInfo)
        {
            return typeInfo.BaseType;
        }

        return null;
    }
}
