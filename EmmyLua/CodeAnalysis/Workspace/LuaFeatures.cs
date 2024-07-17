using System.Text;
using EmmyLua.CodeAnalysis.Diagnostics;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Document.Version;

namespace EmmyLua.CodeAnalysis.Workspace;

public class LuaFeatures
{
    public LuaLanguage Language { get; set; } = new(LuaLanguageLevel.LuaLatest);

    public DiagnosticConfig DiagnosticConfig { get; set; } = new();

    public List<string> WorkspaceRoots { get; set; } = [];

    public List<string> ThirdPartyRoots { get; set; } = [];

    public List<FrameworkVersion> FrameworkVersions { get; set; } = [];

    public HashSet<string> Includes { get; set; } =
    [
        "**/*.lua"
    ];

    public HashSet<string> ExcludeFolders { get; set; } =
    [
        ".git",
        ".svn",
        ".p4",
    ];

    public HashSet<string> ExcludeGlobs { get; set; } = [];

    public List<string> RequirePattern { get; set; } =
    [
        "?/init.lua",
        "?.lua"
    ];

    public HashSet<string> RequireLikeFunction { get; set; } =
    [
        "require"
    ];

    public bool RequirePathStrict { get; set; } = true;

    public bool TypeCallStrict { get; set; } = true;

    public bool InitStdLib { get; set; } = true;

    public int DontIndexMaxFileSize { get; set; } = 1048576; // 1MB

    public Encoding Encoding { get; set; } = Encoding.UTF8;
}
