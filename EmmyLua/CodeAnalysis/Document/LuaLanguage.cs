using EmmyLua.CodeAnalysis.Document.Version;

namespace EmmyLua.CodeAnalysis.Document;

public static class LuaLanguageLevel
{
    public static VersionNumber Lua51 { get; } = new(5, 1, 0, 0);

    // ReSharper disable once InconsistentNaming
    public static VersionNumber LuaJIT { get; } = new(5, 1, 999, 0);

    public static VersionNumber Lua52 { get; } = new(5, 2, 0, 0);

    public static VersionNumber Lua53 { get; } = new(5, 3, 0, 0);

    public static VersionNumber Lua54 { get; } = new(5, 4, 0, 0);

    public static VersionNumber LuaLatest { get; } = Lua54;
}

public class LuaLanguage(VersionNumber languageLevel)
{
    public static LuaLanguage Default { get; } = new(LuaLanguageLevel.Lua54);

    public VersionNumber LanguageLevel { get; set; } = languageLevel;
}
