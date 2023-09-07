using LuaLanguageServer.CodeAnalysis.Compile;

namespace LuaLanguageServer.CodeAnalysis.Workspace;

public class LuaFeatures
{
    public LuaLanguage Language { get; set; } = new LuaLanguage();

    public string Extensions { get; set; } = "*.lua";
}
