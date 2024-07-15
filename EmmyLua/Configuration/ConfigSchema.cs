using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using EmmyLua.CodeAnalysis.Diagnostics;
using EmmyLua.CodeAnalysis.Workspace.Module.FilenameConverter;

namespace EmmyLua.Configuration;

/// <summary>
/// Setting of EmmyLua
/// </summary>
public class Setting
{
    [JsonPropertyName("$schema")]
    public string? Schema { get; set; } = null;

    [JsonPropertyName("completion")]
    public Completion Completion { get; set; } = new();

    [JsonPropertyName("signature")]
    public Signature Signature { get; set; } = new();

    [JsonPropertyName("diagnostics")]
    public Diagnostics Diagnostics { get; set; } = new();

    [JsonPropertyName("hint")]
    public Hint Hint { get; set; } = new();

    [JsonPropertyName("runtime")]
    public Runtime Runtime { get; set; } = new();

    [JsonPropertyName("workspace")]
    public Workspace Workspace { get; set; } = new();

    [JsonPropertyName("resource")]
    public Resource Resource { get; set; } = new();

    [JsonPropertyName("codeLens")]
    public CodeLens CodeLens { get; set; } = new();

    [JsonPropertyName("strict")]
    public Strict Strict { get; set; } = new();
}

public class Completion
{
    [JsonPropertyName("autoRequire")]
    public bool AutoRequire { get; set; } = true;

    [JsonPropertyName("autoRequireFunction")]
    public string AutoRequireFunction { get; set; } = "require";

    [JsonPropertyName("autoRequireNamingConvention")]
    public FilenameConvention AutoRequireFilenameConvention { get; set; } = FilenameConvention.SnakeCase;

    [JsonPropertyName("callSnippet")]
    public bool CallSnippet { get; set; } = false;

    [JsonPropertyName("postfix")]
    public string Postfix { get; set; } = "@";
}

public class Diagnostics
{
    [JsonPropertyName("disable")]
    public List<DiagnosticCode> Disable { get; set; } = [];

    [JsonPropertyName("enable")]
    public bool? Enable { get; set; }

    [JsonPropertyName("globals")]
    public List<string> Globals { get; set; } = [];

    [JsonPropertyName("globalsRegex")]
    public List<string> GlobalsRegex { get; set; } = [];

    [JsonPropertyName("severity")]
    public Dictionary<DiagnosticCode, DiagnosticSeverity> Severity { get; set; } = [];

    [JsonPropertyName("enables")]
    public List<DiagnosticCode> Enables { get; set; } = [];
}

// public class DiagnosticSeverityConverter : System.Text.Json.Serialization.JsonConverter<Dictionary<DiagnosticCode, DiagnosticSeverity>>
// {
//     public override void WriteJson(Utf8JsonWriter writer, Dictionary<DiagnosticCode, DiagnosticSeverity>? value,
//         JsonSerializer serializer)
//     {
//         writer.WriteStartObject();
//         if (value is not null)
//         {
//             foreach (var (key, val) in value)
//             {
//                 writer.WritePropertyName(key.ToString());
//                 writer.WriteValue(val.ToString());
//             }
//         }
//
//         writer.WriteEndObject();
//     }
//
//     public override Dictionary<DiagnosticCode, DiagnosticSeverity> ReadJson(
//         JsonReader reader, Type objectType,
//         Dictionary<DiagnosticCode, DiagnosticSeverity>? existingValue, bool hasExistingValue,
//         JsonSerializer serializer)
//     {
//         var dictionary = new Dictionary<DiagnosticCode, DiagnosticSeverity>();
//         while (reader.Read())
//         {
//             if (reader.TokenType == JsonToken.PropertyName)
//             {
//                 var key = DiagnosticCodeHelper.GetCode(reader.Value?.ToString() ?? string.Empty);
//                 if (!reader.Read()) throw new JsonSerializationException("Unexpected end when reading dictionary.");
//                 var value = DiagnosticSeverityHelper.GetSeverity(reader.Value?.ToString() ?? string.Empty);
//                 dictionary[key] = value;
//             }
//             else if (reader.TokenType == JsonToken.EndObject)
//             {
//                 return dictionary;
//             }
//         }
//         throw new JsonSerializationException("Unexpected end when reading dictionary.");
//     }
// }

public class Hint
{
    [JsonPropertyName("paramHint")]
    public bool ParamHint { get; set; } = true;

    [JsonPropertyName("indexHint")]
    public bool IndexHint { get; set; } = true;

    [JsonPropertyName("localHint")]
    public bool LocalHint { get; set; } = false;

    [JsonPropertyName("overrideHint")]
    public bool OverrideHint { get; set; } = true;
}

public class Runtime
{
    [JsonPropertyName("version")]
    public LuaVersion Version { get; set; } = LuaVersion.LuaLatest;

    [JsonPropertyName("requireLikeFunction")]
    public List<string> RequireLikeFunction { get; set; } = [];

    [JsonPropertyName("frameworkVersions")]
    public List<string> FrameworkVersions { get; set; } = [];

    [JsonPropertyName("extensions")]
    public List<string> Extensions { get; set; } = [];

    [JsonPropertyName("requirePattern")]
    public List<string> RequirePattern { get; set; } = [];
}

[JsonConverter(typeof(LuaVersionJsonConverter))]
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

public class LuaVersionJsonConverter : JsonConverter<LuaVersion>
{
    public override LuaVersion Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var value = reader.GetString();
        return value switch
        {
            "Lua5.1" => LuaVersion.Lua51,
            "LuaJIT" => LuaVersion.LuaJIT,
            "Lua5.2" => LuaVersion.Lua52,
            "Lua5.3" => LuaVersion.Lua53,
            "Lua5.4" => LuaVersion.Lua54,
            "LuaLatest" => LuaVersion.LuaLatest,
            _ => LuaVersion.Lua54
        };
    }

    public override void Write(Utf8JsonWriter writer, LuaVersion value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value switch
        {
            LuaVersion.Lua51 => "Lua5.1",
            LuaVersion.LuaJIT => "LuaJIT",
            LuaVersion.Lua52 => "Lua5.2",
            LuaVersion.Lua53 => "Lua5.3",
            LuaVersion.Lua54 => "Lua5.4",
            LuaVersion.LuaLatest => "LuaLatest",
            _ => "Lua5.4"
        });
    }
}

public class Workspace
{
    [JsonPropertyName("ignoreDir")]
    public List<string> IgnoreDir { get; set; } =
    [
        ".idea",
        ".vs",
        ".vscode"
    ];

    [JsonPropertyName("library")]
    public List<string> Library { get; set; } = [];

    [JsonPropertyName("workspaceRoots")]
    public List<string> WorkspaceRoots { get; set; } = [];

    [JsonPropertyName("preloadFileSize")]
    public int PreloadFileSize { get; set; } = 1048576; // 1Mb

    [JsonPropertyName("encoding")]
    public string Encoding { get; set; } = string.Empty;
}

public class Resource
{
    [JsonPropertyName("paths")]
    public List<string> Paths { get; set; } = [];
}

public class CodeLens
{
    [JsonPropertyName("enable")]
    public bool Enable { get; set; } = true;
}

public class Strict
{
    [JsonPropertyName("requirePath")]
    public bool RequirePath { get; set; } = true;

    [JsonPropertyName("typeCall")]
    public bool TypeCall { get; set; } = true;
}

public class Signature
{
    [JsonPropertyName("detailSignatureHelper")]
    public bool DetailSignatureHelper { get; set; } = false;
}
