using System.Text.Json.Serialization;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Interface;
using EmmyLua.LanguageServer.Framework.Protocol.Model.TextDocument;

namespace EmmyLua.LanguageServer.Framework.Protocol.Message.Declaration;

public class DeclarationParams : TextDocumentPositionParams, IWorkDoneProgressParams, IPartialResultParams
{
    [JsonPropertyName("workDoneToken")]
    public string? WorkDoneToken { get; set; }

    [JsonPropertyName("partialResultToken")]
    public string? PartialResultToken { get; set; }
}
