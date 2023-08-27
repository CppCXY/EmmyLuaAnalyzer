namespace LuaLanguageServer.LuaCore.Syntax.Location;

public enum LuaLocationKind : byte
{
    None = 0,

    SourceFile = 1,

    DllFile = 2,

    ExternalFile = 3,
}
