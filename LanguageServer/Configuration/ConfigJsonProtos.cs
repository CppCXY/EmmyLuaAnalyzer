namespace LanguageServer.Configuration;

// ReSharper disable once ClassNeverInstantiated.Global
class LuaRc
{
    public Diagnostics? Diagnostics { get; set; }

    public Runtime? Runtime { get; set; }

    public Workspace? Workspace { get; set; }

    public Type? Type { get; set; }

    public Doc? Doc { get; set; }
}

// ReSharper disable once ClassNeverInstantiated.Global
class Diagnostics
{
    public List<string>? Disable { get; set; }

    public Dictionary<string, string>? GroupFileStatus { get; set; }

    public string? IgnoredFiles { get; set; }

    public string? LibraryFiles { get; set; }
}

// ReSharper disable once ClassNeverInstantiated.Global
class Runtime
{
    public string? Version { get; set; } = "5.4";

    public List<string>? Path { get; set; }

    public bool? PathStrict { get; set; } = true;
}

// ReSharper disable once ClassNeverInstantiated.Global
class Workspace
{
    public int? MaxPreload { get; set; } = 10000;

    public int? PreloadFileSize { get; set; } = 204800;

    public List<string>? IgnoreDirs { get; set; } =
    [
        ".git",
        ".svn",
        ".idea",
        ".vs",
        ".vscode"
    ];

    public bool CheckThirdParty { get; set; } = false;
}

// ReSharper disable once ClassNeverInstantiated.Global
class Type
{
    public bool CastNumberToInteger { get; set; } = false;
}

// ReSharper disable once ClassNeverInstantiated.Global
class Doc
{
    public List<string>? PrivateName { get; set; } = ["_"];
}
