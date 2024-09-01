using EmmyLua.CodeAnalysis.Compilation.Type.Types;
using EmmyLua.CodeAnalysis.Document;

namespace EmmyLua.CodeAnalysis.Compilation.Type;

public static class Builtin
{
    public static LuaNamedType Unknown { get; } = new(LuaDocumentId.VirtualDocumentId, "unknown");
    public static LuaNamedType Any { get; } = new(LuaDocumentId.VirtualDocumentId, "any");
    public static LuaNamedType Nil { get; } = new(LuaDocumentId.VirtualDocumentId, "nil");
    public static LuaNamedType Boolean { get; } = new(LuaDocumentId.VirtualDocumentId, "boolean");
    public static LuaNamedType Number { get; } = new(LuaDocumentId.VirtualDocumentId, "number");
    public static LuaNamedType Integer { get; } = new(LuaDocumentId.VirtualDocumentId, "integer");
    public static LuaNamedType String { get; } = new(LuaDocumentId.VirtualDocumentId, "string");
    public static LuaNamedType Table { get; } = new(LuaDocumentId.VirtualDocumentId, "table");

    public static LuaNamedType Thread { get; } = new(LuaDocumentId.VirtualDocumentId, "thread");
    public static LuaNamedType UserData { get; } = new(LuaDocumentId.VirtualDocumentId, "userdata");

    public static LuaNamedType Self { get; } = new(LuaDocumentId.VirtualDocumentId, "self");

    public static LuaNamedType Global { get; } = new(LuaDocumentId.VirtualDocumentId, "global");

    public static LuaNamedType Namespace { get; } = new(LuaDocumentId.VirtualDocumentId, "namespace");
}
