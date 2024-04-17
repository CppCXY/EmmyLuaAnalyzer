using System.Runtime.Serialization;

namespace EmmyLua.CodeAnalysis.Document;

public enum LuaLanguageLevel : short
{
    [EnumMember(Value = "Lua5.1")]
    Lua51 = 510,
    [EnumMember(Value = "LuaJIT")]
    // ReSharper disable once InconsistentNaming
    LuaJIT = 515,
    [EnumMember(Value = "Lua5.2")]
    Lua52 = 520,
    [EnumMember(Value = "Lua5.3")]
    Lua53 = 530,
    [EnumMember(Value = "Lua5.4")]
    Lua54 = 540,
}

public class LuaLanguage(LuaLanguageLevel languageLevel = LuaLanguageLevel.Lua54)
{
    public static LuaLanguage Default { get; } = new();

    public LuaLanguageLevel LanguageLevel { get; set; } = languageLevel;
}
