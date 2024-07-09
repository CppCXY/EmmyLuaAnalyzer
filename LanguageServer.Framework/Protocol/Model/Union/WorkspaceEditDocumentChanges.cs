using System.Text.Json;
using System.Text.Json.Serialization;
using EmmyLua.LanguageServer.Framework.Protocol.Model.TextEdit;

namespace EmmyLua.LanguageServer.Framework.Protocol.Model.Union;

[JsonConverter(typeof(WorkspaceEditDocumentChangesConverter))]
public class WorkspaceEditDocumentChanges
{
    public List<TextDocumentEdit>? TextDocumentEditList { get; }

    // AnyOf<TextDocumentEdit, CreateFile, RenameFile, DeleteFile>
    public List<object>? EditFileList { get; }

    public WorkspaceEditDocumentChanges(List<TextDocumentEdit> textDocumentEditList)
    {
        TextDocumentEditList = textDocumentEditList;
    }

    public WorkspaceEditDocumentChanges(List<object> editFileList)
    {
        EditFileList = editFileList;
    }
}

public class WorkspaceEditDocumentChangesConverter : JsonConverter<WorkspaceEditDocumentChanges>
{
    public override WorkspaceEditDocumentChanges Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            var textDocumentEditList = JsonSerializer.Deserialize<List<TextDocumentEdit>>(ref reader, options);
            return new WorkspaceEditDocumentChanges(textDocumentEditList!);
        }

        var editFileList = JsonSerializer.Deserialize<List<object>>(ref reader, options);
        return new WorkspaceEditDocumentChanges(editFileList!);
    }

    public override void Write(Utf8JsonWriter writer, WorkspaceEditDocumentChanges value, JsonSerializerOptions options)
    {
        if (value.TextDocumentEditList != null)
        {
            JsonSerializer.Serialize(writer, value.TextDocumentEditList, options);
        }
        else
        {
            JsonSerializer.Serialize(writer, value.EditFileList, options);
        }
    }
}
