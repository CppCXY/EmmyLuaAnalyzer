namespace EmmyLua.CodeAnalysis.Compilation.Type;

public class Builtin
{
    public LuaNamedType Unknown { get; } = new("unknown", TypeKind.Unknown);
    public LuaNamedType Any { get; } = new("any");
    public LuaNilType Nil { get; } = new();
    public LuaNamedType Boolean { get; } = new("boolean");
    public LuaNamedType Number { get; } = new("number");
    public LuaNamedType Integer { get; } = new("integer");
    public LuaNamedType String { get; } = new("string");
    public LuaNamedType Function { get; } = new("function");
    public LuaNamedType Table { get; } = new("table");
    public LuaNamedType Thread { get; } = new("thread");
    public LuaNamedType UserData { get; } = new("userdata");

    public LuaNamedType? FromName(string name)
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
            "function" => Function,
            "table" => Table,
            "thread" => Thread,
            "userdata" => UserData,
            _ => null,
        };
    }
}
