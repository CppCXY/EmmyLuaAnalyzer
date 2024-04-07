using Newtonsoft.Json;

namespace LanguageServer.Configuration.Json;

// ReSharper disable once ClassNeverInstantiated.Global
public class LuaRc
{
    public DiagnosticsConfig? Diagnostics { get; set; }

    public RuntimeConfig? Runtime { get; set; }

    public WorkspaceConfig? Workspace { get; set; }

    public TypeConfig? Type { get; set; }

    [JsonProperty("doc")]
    public DocumentConfig? Document { get; set; }
    
    public CompletionConfig? Completion { get; set; }
}

// ReSharper disable once ClassNeverInstantiated.Global
public class DiagnosticsConfig
{
    public List<string>? Disable { get; set; }

    public Dictionary<string, string>? GroupFileStatus { get; set; }

    public string? IgnoredFiles { get; set; }

    public string? LibraryFiles { get; set; }
}

// ReSharper disable once ClassNeverInstantiated.Global
public class RuntimeConfig
{
    public string? Version { get; set; } = "5.4";

    public List<string>? Path { get; set; }

    public bool? PathStrict { get; set; } = true;
}

// ReSharper disable once ClassNeverInstantiated.Global
public class WorkspaceConfig
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
public class TypeConfig
{
    public bool CastNumberToInteger { get; set; } = false;
}

// ReSharper disable once ClassNeverInstantiated.Global
public class DocumentConfig
{
    public List<string>? PrivateName { get; set; } = ["_"];
}

// ReSharper disable once ClassNeverInstantiated.Global
public class CompletionConfig
{
    public bool AutoFillArguments { get; set; } = false;
}
