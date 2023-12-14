namespace LuaLanguageServer.CodeAnalysis.Compile;

public enum LuaLanguageLevel : short
{
    Lua51 = 510,
    LuaJIT = 515,
    Lua52 = 520,
    Lua53 = 530,
    Lua54 = 540,
}

public class LuaLanguage(LuaLanguageLevel languageLevel = LuaLanguageLevel.Lua54)
{
    public static LuaLanguage Default { get; } = new();

    public LuaLanguageLevel LanguageLevel { get; set; } = languageLevel;
}
