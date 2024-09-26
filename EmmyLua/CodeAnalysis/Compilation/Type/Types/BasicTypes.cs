using EmmyLua.CodeAnalysis.Compilation.Search;
using EmmyLua.CodeAnalysis.Compilation.Signature;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;

namespace EmmyLua.CodeAnalysis.Compilation.Type.Types;

public abstract class LuaBasicType : LuaType
{
    public override IEnumerable<LuaType> ChildrenTypes => [];

    public override IEnumerable<LuaType> DescendantTypes => [];
}

public class LuaNamedType(LuaDocumentId documentId, string name)
    : LuaBasicType
{
    public LuaDocumentId DocumentId { get; } = documentId;

    public string Name { get; } = name;

    public override string ToString() => $"({Name},{DocumentId})";
}

public class LuaStringLiteralType(string content)
    : LuaBasicType
{
    public string Content { get; } = content;

    public override string ToString() => $"\"{Content}\"";
}

public class LuaIntegerLiteralType(long value)
    : LuaBasicType
{
    public long Value { get; } = value;

    public override string ToString() => $"{Value}";
}

public class LuaBooleanLiteralType(bool value)
    : LuaBasicType
{
    public bool Value { get; } = value;

    public override string ToString() => $"{Value}";
}

public class LuaMethodType(LuaSignatureId id)
    : LuaBasicType
{
    public LuaSignatureId SignatureId { get; } = id;

    public override string ToString() => $"LuaMethodType {SignatureId}";
}

public class LuaTypeRef(LuaTypeId id) : LuaBasicType
{
    public LuaTypeId Id { get; } = id;

    public LuaDocumentId DocumentId => Id.Id.DocumentId;

    public override string ToString() => $"LuaTypeRef {Id}";
}

public class LuaElementRef(SyntaxElementId id)
    : LuaBasicType
{
    public SyntaxElementId Id { get; } = id;

    public LuaSyntaxElement? ToSyntaxElement(SearchContext context)
    {
        var document = context.Compilation.Project.GetDocument(Id.DocumentId);
        return document?.SyntaxTree.GetElement(Id.ElementId);
    }

    public override string ToString() => $"LuaElementRef {Id}";
}

// AAAA.`T`
public class LuaStrTplType(string prefixName, string name)
    : LuaBasicType
{
    public string Name { get; } = name;

    public string PrefixName { get; } = prefixName;

    public override string ToString() => $"{PrefixName}.`{Name}`";
}

// T...
public class LuaExpandTplType(string baseName)
    : LuaBasicType
{
    public string Name { get; } = baseName;

    public override string ToString() => $"{Name}...";
}

