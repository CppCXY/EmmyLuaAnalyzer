using EmmyLua.CodeAnalysis.Diagnostics;
using EmmyLua.CodeAnalysis.Document;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EmmyLua.Configuration;

/// <summary>
/// Setting of EmmyLua
/// </summary>
public class Setting
{
    [JsonProperty("completion", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public Completion Completion { get; set; } = new();

    [JsonProperty("diagnostics", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public Diagnostics Diagnostics { get; set; } = new();

    [JsonProperty("hint", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public Hint Hint { get; set; } = new();

    [JsonProperty("runtime", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public Runtime Runtime { get; set; } = new();

    [JsonProperty("workspace", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public Workspace Workspace { get; set; } = new();
}

public class Completion
{
    [JsonProperty("autoRequire", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public bool AutoRequire { get; set; } = true;

    [JsonProperty("callSnippet", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public bool CallSnippet { get; set; } = false;

    [JsonProperty("postfix", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public string Postfix { get; set; } = "@";
}

public class Diagnostics
{
    [JsonProperty("disable", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore,
        ItemConverterType = typeof(StringEnumConverter))]
    public List<DiagnosticCode> Disable { get; set; } = [];

    [JsonProperty("enable", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public bool? Enable { get; set; }

    [JsonProperty("globals", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public List<string> Globals { get; set; } = [];
}

public class Hint
{
    [JsonProperty("paramHint", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public bool ParamHint { get; set; } = true;

    [JsonProperty("indexHint", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public bool IndexHint { get; set; } = true;

    [JsonProperty("localHint", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public bool LocalHint { get; set; } = false;

    [JsonProperty("overrideHint", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public bool OverrideHint { get; set; } = true;
}

public class Runtime
{
    [JsonProperty("version", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    [JsonConverter(typeof(StringEnumConverter))]
    public LuaLanguageLevel Version { get; set; } = LuaLanguageLevel.Lua54;
}

public class Workspace
{
    [JsonProperty("ignoreDir", Required = Required.Default,
        NullValueHandling = NullValueHandling.Ignore)]
    public List<string> IgnoreDir { get; set; } = new();

    [JsonProperty("library", Required = Required.Default,
        NullValueHandling = NullValueHandling.Ignore)]
    public List<string> Library { get; set; } = new();

    [JsonProperty("workspaceRoots", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public List<string> WorkspaceRoots { get; set; } = new();

    [JsonProperty("preloadFileSize", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public int PreloadFileSize { get; set; } = 2048000;
}
