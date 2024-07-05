using System.Text.Json;
using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Model;

[JsonConverter(typeof(DocumentUriConverter))]
public record struct DocumentUri(Uri Uri)
{
    public Uri Uri { get; } = Uri;
}

public class DocumentUriConverter : JsonConverter<DocumentUri>
{
    public override DocumentUri Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        var uri = reader.GetString() ?? string.Empty;
        return new DocumentUri(new Uri(uri));
    }

    public override void Write(Utf8JsonWriter writer, DocumentUri value, JsonSerializerOptions options)
    {
        writer.WriteStringValue(value.Uri.ToString());
    }
}