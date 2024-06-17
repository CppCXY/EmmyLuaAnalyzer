namespace EmmyLua.CodeAnalysis.Compilation.Type;

[Flags]
public enum LuaTypeAttribute
{
    None,
    HasMember,
    CanIndex,
    CanCall,
}
