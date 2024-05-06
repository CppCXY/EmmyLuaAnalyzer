using EmmyLua.CodeAnalysis.Diagnostics;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Document.Version;

namespace EmmyLua.CodeAnalysis.Workspace;

public class LuaFeatures
{
    public LuaLanguage Language { get; set; } = new(LuaLanguageLevel.LuaLatest);

    public DiagnosticConfig DiagnosticConfig { get; set; } = new();

    public List<string> WorkspaceRoots { get; set; } = new();

    public List<string> ThirdPartyRoots { get; set; } = new();

    public List<FrameworkVersion> FrameworkVersions { get; set; } = new();

    public List<string> Extensions { get; set; } = [
        "*.lua", "*.lua.txt"
    ];

    public HashSet<string> ExcludeFolders { get; set; } =
    [
        ".git",
        ".svn",
        ".idea",
        ".vs",
        ".vscode"
    ];

    public List<string> RequirePattern { get; set; } = [
        "?/init.lua",
        "?.lua"
    ];

    public HashSet<string> RequireLikeFunction { get; set; } =
    [
        "require"
    ];

    public bool InitStdLib { get; set; } = true;

    public int DontIndexMaxFileSize { get; set; } = 1048576; // 1MB
}
