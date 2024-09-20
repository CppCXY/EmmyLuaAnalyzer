using EmmyLua.CodeAnalysis.Compilation.Search;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;

namespace EmmyLua.CodeAnalysis.Compilation.Type.Types;

public class LuaNamedType(LuaDocumentId documentId, string name)
    : LuaType
{
    public LuaDocumentId DocumentId { get; } = documentId;

    public string Name { get; } = name;

    public override bool IsUnknown => Name == "unknown";
}

public class LuaStringLiteralType(string content)
    : LuaType
{
    public string Content { get; } = content;
}

public class LuaIntegerLiteralType(long value)
    : LuaType
{
    public long Value { get; } = value;
}



// public class GlobalNameType(string name)
//     : LuaType
// {
//     public string Name { get; } = name;
// }

