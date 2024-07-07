using System.Text.Json;
using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Model.Union;

[JsonConverter(typeof(TextOrAnnotatedOrSnippetEditListConverter))]
public class TextOrAnnotatedOrSnippetEditList
{
    public List<TextEdit>? TextEditList { get; }

    public List<AnnotatedTextEdit>? AnnotatedTextEditList { get; }

    public List<SnippetTextEdit>? SnippetTextEditList { get; }

    public TextOrAnnotatedOrSnippetEditList(List<TextEdit> textEditList)
    {
        TextEditList = textEditList;
    }

    public TextOrAnnotatedOrSnippetEditList(List<AnnotatedTextEdit> annotatedTextEditList)
    {
        AnnotatedTextEditList = annotatedTextEditList;
    }

    public TextOrAnnotatedOrSnippetEditList(List<SnippetTextEdit> snippetTextEditList)
    {
        SnippetTextEditList = snippetTextEditList;
    }

    public static implicit operator TextOrAnnotatedOrSnippetEditList(List<TextEdit> item1) => new(item1);

    public static implicit operator TextOrAnnotatedOrSnippetEditList(List<AnnotatedTextEdit> item2) => new(item2);

    public static implicit operator TextOrAnnotatedOrSnippetEditList(List<SnippetTextEdit> item3) => new(item3);
}

public class TextOrAnnotatedOrSnippetEditListConverter : JsonConverter<TextOrAnnotatedOrSnippetEditList>
{
    // TODO: this is a error implementation, need to fix it
    public override TextOrAnnotatedOrSnippetEditList Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType == JsonTokenType.StartArray)
        {
            var textEditList = JsonSerializer.Deserialize<List<TextEdit>>(ref reader, options);
            return new TextOrAnnotatedOrSnippetEditList(textEditList!);
        }

        if (reader.TokenType == JsonTokenType.StartArray)
        {
            var annotatedTextEditList = JsonSerializer.Deserialize<List<AnnotatedTextEdit>>(ref reader, options);
            return new TextOrAnnotatedOrSnippetEditList(annotatedTextEditList!);
        }

        var snippetTextEditList = JsonSerializer.Deserialize<List<SnippetTextEdit>>(ref reader, options);
        return new TextOrAnnotatedOrSnippetEditList(snippetTextEditList!);
    }

    public override void Write(Utf8JsonWriter writer, TextOrAnnotatedOrSnippetEditList value, JsonSerializerOptions options)
    {
        if (value.TextEditList != null)
        {
            JsonSerializer.Serialize(writer, value.TextEditList, options);
        }
        else if (value.AnnotatedTextEditList != null)
        {
            JsonSerializer.Serialize(writer, value.AnnotatedTextEditList, options);
        }
        else
        {
            JsonSerializer.Serialize(writer, value.SnippetTextEditList, options);
        }
    }
}
