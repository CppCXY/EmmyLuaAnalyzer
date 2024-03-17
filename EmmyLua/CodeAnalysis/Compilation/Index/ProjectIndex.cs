using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Compilation.Type.DetailType;
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

    public TypeOperatorStorage TypeOperatorStorage { get; } = new();

    private IndexStorage<string, NamedTypeKind> NamedTypeKinds { get; } = new();

    public void Remove(DocumentId documentId)
    {
        NameDeclaration.Remove(documentId);
        Members.Remove(documentId);
        GlobalDeclaration.Remove(documentId);
        Supers.Remove(documentId);
        NamedType.Remove(documentId);
        GenericParam.Remove(documentId);
        TypeOperatorStorage.Remove(documentId);
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

    public void AddType(DocumentId documentId, string name, NamedTypeLuaDeclaration luaDeclaration, NamedTypeKind kind)
    {
        NamedType.Add(documentId, name, luaDeclaration);
        NameDeclaration.Add(documentId, name, luaDeclaration);
        NamedTypeKinds.Add(documentId, name, kind);
    }

    public void AddAlias(DocumentId documentId, string name, LuaType baseType, NamedTypeLuaDeclaration luaDeclaration)
    {
        AddType(documentId, name, luaDeclaration, NamedTypeKind.Alias);
        Id2Type.Add(documentId, name, baseType);
    }

    public void AddEnum(DocumentId documentId, string name, LuaType? baseType, NamedTypeLuaDeclaration luaDeclaration)
    {
        AddType(documentId, name, luaDeclaration, NamedTypeKind.Enum);
        if (baseType != null)
        {
            Supers.Add(documentId, name, baseType);
        }
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

    public IEnumerable<LuaDeclaration> GetDeclarations(string name)
    {
        return NameDeclaration.Get<LuaDeclaration>(name);
    }

    public LuaType? GetExportType(DocumentId documentId)
    {
        return ExportTypes.GetValueOrDefault(documentId);
    }

    public BasicDetailType GetDetailNamedType(string name)
    {
        var kind = NamedTypeKinds.GetOne(name);
        switch (kind)
        {
            case NamedTypeKind.Alias:
            {
                var baseType = Id2Type.GetOne(name);
                return new AliasDetailType(name, baseType);
            }
            case NamedTypeKind.Class:
            {
                var supers = Supers.Get(name).ToList();
                var generics = GenericParam.Get(name).ToList();
                var declaration = NamedType.GetOne(name);
                return new ClassDetailType(name, supers, generics, declaration);
            }
            case NamedTypeKind.Enum:
            {
                var baseType = Supers.GetOne(name);
                return new EnumDetailType(name, baseType);
            }
            case NamedTypeKind.Interface:
            {
                var supers = Supers.Get(name).ToList();
                var generics = GenericParam.Get(name).ToList();
                var declaration = NamedType.GetOne(name);
                return new InterfaceDetailType(name, supers, generics, declaration);
            }
        }

        return new BasicDetailType(name, NamedTypeKind.Class);
    }
}
