namespace EmmyLua.CodeAnalysis.Compilation.Type;

public static class Builtin
{
    public static LuaNamedType Unknown { get; } = new("unknown", TypeKind.Unknown);
    public static LuaNamedType Any { get; } = new("any", TypeKind.Any);
    public static LuaNamedType Nil { get; } = new("nil", TypeKind.Nil);
    public static LuaNamedType Boolean { get; } = new("boolean");
    public static LuaNamedType Number { get; } = new("number");
    public static LuaNamedType Integer { get; } = new("integer");
    public static LuaNamedType String { get; } = new("string");
    public static LuaNamedType Table { get; } = new("table");
    public static LuaNamedType Thread { get; } = new("thread");
    public static LuaNamedType UserData { get; } = new("userdata");

    public static LuaNamedType Self { get; } = new("self");

    public static LuaNamedType? FromName(string name)
    {
        return name switch
        {
            "unknown" => Unknown,
            "any" => Any,
            "nil" or "void" => Nil,
            "boolean" or "bool" => Boolean,
            "number" => Number,
            "integer" or "int" => Integer,
            "string" => String,
            "table" => Table,
            "thread" => Thread,
            "userdata" => UserData,
            "self" => Self,
            _ => null,
        };
    }
}
