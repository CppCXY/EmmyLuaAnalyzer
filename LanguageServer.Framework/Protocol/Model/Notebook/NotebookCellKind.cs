using System.Text.Json;
using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Model.Notebook;

[JsonConverter(typeof(NotebookCellKindJsonConverter))]
public readonly record struct NotebookCellKind(int Value)
{
    /**
     * A markup-cell is a formatted source that is used for display.
     */
    public static readonly NotebookCellKind Markdown = new(1);

    /**
     * A code-cell is source code.
     */
    public static readonly NotebookCellKind Code = new(2);

    public int Value { get; } = Value;
}

public class NotebookCellKindJsonConverter : JsonConverter<NotebookCellKind>
{
    public override NotebookCellKind Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.Number)
        {
            throw new JsonException();
        }

        return new NotebookCellKind(reader.GetInt32());
    }

    public override void Write(Utf8JsonWriter writer, NotebookCellKind value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.Value);
    }
}
