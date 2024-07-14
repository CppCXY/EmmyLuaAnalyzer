using System.Text.Json.Serialization;

namespace EmmyLua.Configuration;

[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Metadata)]
[JsonSerializable(typeof(Setting))]
public partial class SettingGenerateContext : JsonSerializerContext;

