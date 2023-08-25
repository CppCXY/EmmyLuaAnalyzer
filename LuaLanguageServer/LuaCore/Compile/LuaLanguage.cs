namespace LuaLanguageServer.LuaCore.Compile;

public enum LuaLanguageLevel : byte
{
    LuaJIT = 50,
    Lua51 = 51,
    Lua52 = 52,
    Lua53 = 53,
    Lua54 = 54,
}

public class LuaLanguage
{
    public LuaLanguageLevel LanguageLevel { get; set; }

    public LuaLanguage(LuaLanguageLevel languageLevel = LuaLanguageLevel.Lua54)
    {
        LanguageLevel = languageLevel;
    }

    public bool IsRequireLike(string methodName)
    {
        return methodName == "require";
    }
}
