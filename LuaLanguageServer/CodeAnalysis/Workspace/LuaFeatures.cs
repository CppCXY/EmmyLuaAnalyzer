using LuaLanguageServer.CodeAnalysis.Compile;

namespace LuaLanguageServer.CodeAnalysis.Workspace;

public class LuaFeatures
{
    public LuaLanguage Language { get; set; } = new();

    public string Extensions { get; set; } = "*.lua";

    public List<string> ExcludeFolders { get; set; } = new()
    {
        ".git",
        ".svn",
        ".idea",
        ".vs",
        ".vscode"
    };
}
