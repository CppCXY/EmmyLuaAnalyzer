using System.Text.Json.Serialization;
using EmmyLua.CodeAnalysis.Diagnostics;

namespace EmmyLua.Configuration;

[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Metadata)]
[JsonSerializable(typeof(Setting))]
[JsonSerializable(typeof(List<DiagnosticCode>))]
public partial class SettingGenerateContext : JsonSerializerContext;

