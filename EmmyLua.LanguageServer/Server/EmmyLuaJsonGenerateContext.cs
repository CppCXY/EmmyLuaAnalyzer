using System.Text.Json.Serialization;
using EmmyLua.LanguageServer.DocumentRender;
using EmmyLua.LanguageServer.Server.ClientConfig;
using EmmyLua.LanguageServer.Server.Monitor;

namespace EmmyLua.LanguageServer.Server;

[JsonSourceGenerationOptions(GenerationMode = JsonSourceGenerationMode.Metadata)]
[JsonSerializable(typeof(EmmyAnnotatorRequestParams))]
[JsonSerializable(typeof(EmmyAnnotatorResponse))]
[JsonSerializable(typeof(List<EmmyAnnotatorResponse>))]
[JsonSerializable(typeof(ProgressReport))]
[JsonSerializable(typeof(ServerStatusParams))]
[JsonSerializable(typeof(FilesConfig))]
public partial class EmmyLuaJsonGenerateContext : JsonSerializerContext;