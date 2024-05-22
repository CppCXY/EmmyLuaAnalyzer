using System.Runtime.Serialization;
using EmmyLua.CodeAnalysis.Diagnostics;
using EmmyLua.CodeAnalysis.Util.FilenameConverter;
using Newtonsoft.Json;
using Newtonsoft.Json.Converters;

namespace EmmyLua.Configuration;

/// <summary>
/// Setting of EmmyLua
/// </summary>
public class Setting
{
    [JsonProperty("$schema", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public string? Schema { get; set; } = null;

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

    [JsonProperty("resource", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public Resource Resource { get; set; } = new();

    [JsonProperty("codeLens", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public CodeLens CodeLens { get; set; } = new();
}

public class Completion
{
    [JsonProperty("autoRequire", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public bool AutoRequire { get; set; } = true;

    [JsonProperty("autoRequireFunction", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public string AutoRequireFunction { get; set; } = "require";

    [JsonProperty("autoRequireNamingConvention", Required = Required.Default,
        NullValueHandling = NullValueHandling.Ignore)]
    [JsonConverter(typeof(StringEnumConverter))]
    public FilenameConvention AutoRequireFilenameConvention { get; set; } = FilenameConvention.SnakeCase;

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

    [JsonProperty("globalsRegex", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public List<string> GlobalsRegex { get; set; } = [];

    [JsonProperty("severity", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore),
     JsonConverter(typeof(DiagnosticSeverityConverter))]
    public Dictionary<DiagnosticCode, DiagnosticSeverity> Severity { get; set; } = [];
}

public class DiagnosticSeverityConverter : JsonConverter<Dictionary<DiagnosticCode, DiagnosticSeverity>>
{
    public override void WriteJson(JsonWriter writer, Dictionary<DiagnosticCode, DiagnosticSeverity>? value,
        JsonSerializer serializer)
    {
        writer.WriteStartObject();
        if (value is not null)
        {
            foreach (var (key, val) in value)
            {
                writer.WritePropertyName(key.ToString());
                writer.WriteValue(val.ToString());
            }
        }

        writer.WriteEndObject();
    }

    public override Dictionary<DiagnosticCode, DiagnosticSeverity> ReadJson(
        JsonReader reader, Type objectType,
        Dictionary<DiagnosticCode, DiagnosticSeverity>? existingValue, bool hasExistingValue,
        JsonSerializer serializer)
    {
        var dictionary = new Dictionary<DiagnosticCode, DiagnosticSeverity>();
        while (reader.Read())
        {
            if (reader.TokenType == JsonToken.PropertyName)
            {
                var key = DiagnosticCodeHelper.GetCode(reader.Value?.ToString() ?? string.Empty);
                if (!reader.Read()) throw new JsonSerializationException("Unexpected end when reading dictionary.");
                var value = DiagnosticSeverityHelper.GetSeverity(reader.Value?.ToString() ?? string.Empty);
                dictionary[key] = value;
            }
            else if (reader.TokenType == JsonToken.EndObject)
            {
                return dictionary;
            }
        }
        throw new JsonSerializationException("Unexpected end when reading dictionary.");
    }
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
    public LuaVersion Version { get; set; } = LuaVersion.LuaLatest;

    [JsonProperty("requireLikeFunction", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public List<string> RequireLikeFunction { get; set; } = [];

    [JsonProperty("frameworkVersions", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public List<string> FrameworkVersions { get; set; } = [];

    [JsonProperty("extensions", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public List<string> Extensions { get; set; } = [];

    [JsonProperty("requirePattern", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public List<string> RequirePattern { get; set; } = [];
}

public enum LuaVersion
{
    [EnumMember(Value = "Lua5.1")]
    Lua51,
    [EnumMember(Value = "LuaJIT")]
    // ReSharper disable once InconsistentNaming
    LuaJIT,
    [EnumMember(Value = "Lua5.2")]
    Lua52,
    [EnumMember(Value = "Lua5.3")]
    Lua53,
    [EnumMember(Value = "Lua5.4")]
    Lua54,
    [EnumMember(Value = "LuaLatest")]
    LuaLatest
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
    public int PreloadFileSize { get; set; } = 1048576; // 1Mb
}

public class Resource
{
    [JsonProperty("paths", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public List<string> Paths { get; set; } = [];
}

public class CodeLens
{
    [JsonProperty("enable", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public bool Enable { get; set; } = false;
}
