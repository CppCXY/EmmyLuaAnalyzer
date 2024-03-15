using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Index;

public class ProjectIndex(LuaCompilation compilation)
{
    private LuaCompilation Compilation { get; } = compilation;

    private IndexStorage<string, LuaDeclaration> NameDeclaration { get; } = new();

    private IndexStorage<string, LuaDeclaration> Members { get; } = new();

    private IndexStorage<string, LuaDeclaration> GlobalDeclaration { get; } = new();

    private IndexStorage<string, LuaType> Supers { get; } = new();

    private IndexStorage<string, NamedTypeLuaDeclaration> NamedType { get; } = new();

    private IndexStorage<string, LuaType> Id2Type { get; } = new();

    private IndexStorage<string, GenericParameterLuaDeclaration> GenericParam { get; } = new();

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

    public void AddMember(DocumentId documentId, string name, LuaDeclaration luaDeclaration)
    {
        Members.Add(documentId, name, luaDeclaration);
        NameDeclaration.Add(documentId, name, luaDeclaration);
    }

    public void AddGlobal(DocumentId documentId, string name, LuaDeclaration luaDeclaration)
    {
        GlobalDeclaration.Add(documentId, name, luaDeclaration);
        NameDeclaration.Add(documentId, name, luaDeclaration);
    }

    public void AddSuper(DocumentId documentId, string name, LuaType type)
    {
        Supers.Add(documentId, name, type);
    }

    public void AddType(DocumentId documentId, string name, NamedTypeLuaDeclaration luaDeclaration, TypeFeature feature)
    {
        NamedType.Add(documentId, name, luaDeclaration);
        NameDeclaration.Add(documentId, name, luaDeclaration);
        TypeIndex.AddFeature(documentId, name, feature);
    }

    public void AddAlias(DocumentId documentId, string name, LuaType baseType, NamedTypeLuaDeclaration luaDeclaration)
    {
        AddType(documentId, name, luaDeclaration, TypeFeature.Alias);
        Id2Type.Add(documentId, name, baseType);
    }

    public void AddMethod(DocumentId documentId, string id, LuaMethodType methodType)
    {
        Id2Type.Add(documentId, id, methodType);
    }

    public void AddGenericParam(DocumentId documentId, string name, GenericParameterLuaDeclaration luaDeclaration)
    {
        GenericParam.Add(documentId, name, luaDeclaration);
        NameDeclaration.Add(documentId, name, luaDeclaration);
    }

    public void AddExportType(DocumentId documentId, LuaType type)
    {
        ExportTypes[documentId] = type;
    }

    public IEnumerable<LuaDeclaration> GetMembers(string name)
    {
        return Members.Get<LuaDeclaration>(name);
    }

    public IEnumerable<LuaDeclaration> GetGlobal(string name)
    {
        return GlobalDeclaration.Get<LuaDeclaration>(name);
    }

    public IEnumerable<LuaDeclaration> GetGlobals()
    {
        return GlobalDeclaration.GetAll();
    }

    public IEnumerable<LuaType> GetSupers(string name)
    {
        return Supers.Get<LuaType>(name);
    }

    public IEnumerable<NamedTypeLuaDeclaration> GetNamedType(string name)
    {
        return NamedType.Get<NamedTypeLuaDeclaration>(name);
    }

    public IEnumerable<LuaType> GetTypeFromId(string name)
    {
        return Id2Type.Get<LuaType>(name);
    }

    public IEnumerable<GenericParameterLuaDeclaration> GetGenericParam(string name)
    {
        return GenericParam.Get<GenericParameterLuaDeclaration>(name);
    }

    public IEnumerable<LuaDeclaration> GetDeclarations(string name)
    {
        return NameDeclaration.Get<LuaDeclaration>(name);
    }

    public LuaType? GetExportType(DocumentId documentId)
    {
        return ExportTypes.GetValueOrDefault(documentId);
    }

}
