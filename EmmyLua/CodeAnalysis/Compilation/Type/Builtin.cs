namespace EmmyLua.CodeAnalysis.Compilation.Type;

public static class Builtin
{
    public static LuaNamedType Unknown { get; } = new("unknown", LuaTypeAttribute.None);
    public static LuaNamedType Any { get; } = new("any", LuaTypeAttribute.None);
    public static LuaNamedType Nil { get; } = new("nil", LuaTypeAttribute.None);
    public static LuaNamedType Boolean { get; } = new("boolean", LuaTypeAttribute.None);
    public static LuaNamedType Number { get; } = new("number", LuaTypeAttribute.None);
    public static LuaNamedType Integer { get; } = new("integer", LuaTypeAttribute.None);
    public static LuaNamedType String { get; } = new("string", LuaTypeAttribute.HasMember | LuaTypeAttribute.CanIndex);
    public static LuaNamedType Table { get; } = new("table", LuaTypeAttribute.None);
    public static LuaNamedType Thread { get; } = new("thread", LuaTypeAttribute.None);
    public static LuaNamedType UserData { get; } = new("userdata", LuaTypeAttribute.None);

    public static LuaNamedType Self { get; } = new("self", LuaTypeAttribute.None);

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
