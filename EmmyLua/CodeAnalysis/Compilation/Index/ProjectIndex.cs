using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Index;

public class ProjectIndex(LuaCompilation compilation)
{
    private LuaCompilation Compilation { get; } = compilation;

    private IndexStorage<string, Declaration> NameDeclaration { get; } = new();

    private IndexStorage<string, Declaration> Members { get; } = new();

    private IndexStorage<string, Declaration> GlobalDeclaration { get; } = new();

    private IndexStorage<string, LuaType> Supers { get; } = new();

    private IndexStorage<string, NamedTypeDeclaration> NamedType { get; } = new();

    private IndexStorage<string, LuaType> Id2Type { get; } = new();

    private IndexStorage<string, GenericParameterDeclaration> GenericParam { get; } = new();

    private Dictionary<DocumentId, LuaType> ExportTypes { get; } = new();

    public TypeIndex TypeIndex { get; } = new(compilation);

    public void Remove(DocumentId documentId)
    {
        NameDeclaration.Remove(documentId);
        Members.Remove(documentId);
        GlobalDeclaration.Remove(documentId);
        Supers.Remove(documentId);
        NamedType.Remove(documentId);
        GenericParam.Remove(documentId);
        TypeIndex.Remove(documentId);
        ExportTypes.Remove(documentId);
        Id2Type.Remove(documentId);
    }

    public void AddMember(DocumentId documentId, string name, Declaration declaration)
    {
        Members.Add(documentId, name, declaration);
        NameDeclaration.Add(documentId, name, declaration);
    }

    public void AddGlobal(DocumentId documentId, string name, Declaration declaration)
    {
        GlobalDeclaration.Add(documentId, name, declaration);
        NameDeclaration.Add(documentId, name, declaration);
    }

    public void AddSuper(DocumentId documentId, string name, LuaType type)
    {
        Supers.Add(documentId, name, type);
    }

    public void AddType(DocumentId documentId, string name, NamedTypeDeclaration declaration, TypeFeature feature)
    {
        NamedType.Add(documentId, name, declaration);
        NameDeclaration.Add(documentId, name, declaration);
        TypeIndex.AddFeature(documentId, name, feature);
    }

    public void AddAlias(DocumentId documentId, string name, LuaType baseType, NamedTypeDeclaration declaration)
    {
        AddType(documentId, name, declaration, TypeFeature.Alias);
        Id2Type.Add(documentId, name, baseType);
    }

    public void AddMethod(DocumentId documentId, string id, LuaMethodType methodType)
    {
        Id2Type.Add(documentId, id, methodType);
    }

    public void AddGenericParam(DocumentId documentId, string name, GenericParameterDeclaration declaration)
    {
        GenericParam.Add(documentId, name, declaration);
        NameDeclaration.Add(documentId, name, declaration);
    }

    public void AddExportType(DocumentId documentId, LuaType type)
    {
        ExportTypes[documentId] = type;
    }

    public IEnumerable<Declaration> GetMembers(string name)
    {
        return Members.Get<Declaration>(name);
    }

    public IEnumerable<Declaration> GetGlobal(string name)
    {
        return GlobalDeclaration.Get<Declaration>(name);
    }

    public IEnumerable<LuaType> GetSupers(string name)
    {
        return Supers.Get<LuaType>(name);
    }

    public IEnumerable<NamedTypeDeclaration> GetNamedType(string name)
    {
        return NamedType.Get<NamedTypeDeclaration>(name);
    }

    public IEnumerable<LuaType> GetTypeFromId(string name)
    {
        return Id2Type.Get<LuaType>(name);
    }

    public IEnumerable<GenericParameterDeclaration> GetGenericParam(string name)
    {
        return GenericParam.Get<GenericParameterDeclaration>(name);
    }

    public IEnumerable<Declaration> GetDeclarations(string name)
    {
        return NameDeclaration.Get<Declaration>(name);
    }

    public LuaType? GetExportType(DocumentId documentId)
    {
        return ExportTypes.GetValueOrDefault(documentId);
    }

}
