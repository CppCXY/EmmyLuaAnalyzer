using EmmyLua.CodeAnalysis.Document;

namespace EmmyLua.CodeAnalysis.Compilation.Type.Types;

// T
public class LuaTemplateRef(string name, LuaType? baseType) : LuaType
{
    public LuaType? BaseType { get; } = baseType;

    public string Name { get; } = name;

    public override LuaType Instantiate(TypeSubstitution substitution)
    {
        return substitution.Substitute(Name, this);
    }
}

// AAAA.`T`
public class LuaStringTemplate(string prefixName, string templateName)
    : LuaType
{
    public string TemplateName { get; } = templateName;

    public string PrefixName { get; } = prefixName;
}

// T...
public class LuaExpandTemplate(string baseName)
    : LuaType
{
    public string Name { get; } = baseName;
}

public class LuaGenericType(
    LuaDocumentId documentId,
    string baseName,
    List<LuaType> genericArgs)
    : LuaNamedType(documentId, baseName)
{
    public List<LuaType> GenericArgs { get; } = genericArgs;

    public override LuaType Instantiate(TypeSubstitution substitution)
    {
        var newName = Name;
        if (substitution.Substitute(Name) is LuaNamedType namedType)
        {
            newName = namedType.Name;
        }

        var newGenericArgs = GenericArgs.Select(t => t.Instantiate(substitution)).ToList();
        return new LuaGenericType(DocumentId, newName, newGenericArgs);
    }
}
