using System.Text.Json;
using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Model.Kind;


[JsonConverter(typeof(InsertTextModeJsonConverter))]
public readonly record struct InsertTextMode(int Value)
{
    /**
    * The insertion or replace strings are taken as-is. If the
    * value is multiline, the lines below the cursor will be
    * inserted using the indentation defined in the string value.
    * The client will not apply any kind of adjustments to the
    * string.
    */
    public static readonly InsertTextMode AsIs = new(1);

    /**
    * The editor adjusts leading whitespace of new lines so that
    * they match the indentation up to the cursor of the line for
    * which the item is accepted.
    *
    * Consider a line like this: <2tabs><cursor><3tabs>foo. Accepting a
    * multi line completion item is indented using 2 tabs and all
    * following lines inserted will be indented using 2 tabs as well.
    */
    public static readonly InsertTextMode AdjustIndentation = new(2);

    public int Value { get; } = Value;
}

public class InsertTextModeJsonConverter : JsonConverter<InsertTextMode>
{
    public override InsertTextMode Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.Number)
        {
            throw new JsonException();
        }

        return new InsertTextMode(reader.GetInt32());
    }

    public override void Write(Utf8JsonWriter writer, InsertTextMode value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.Value);
    }
}