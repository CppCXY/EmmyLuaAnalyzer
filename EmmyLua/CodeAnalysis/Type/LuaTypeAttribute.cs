namespace EmmyLua.CodeAnalysis.Type;

[Flags]
public enum LuaTypeAttribute
{
    None = 0,
    Partial = 0x01,
    Exact = 0x02,
    Global = 0x04,
}
