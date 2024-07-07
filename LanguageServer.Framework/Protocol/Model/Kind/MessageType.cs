using System.Text.Json;
using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Model.Kind;

/**
 * The kind of a message.
 */
[JsonConverter(typeof(MessageTypeJsonConverter))]
public readonly record struct MessageType(int Value)
{
    /**
     * An error message.
     */
    public static readonly MessageType Error = new(1);

    /**
     * A warning message.
     */
    public static readonly MessageType Warning = new(2);

    /**
     * An information message.
     */
    public static readonly MessageType Info = new(3);

    /**
     * A log message.
     */
    public static readonly MessageType Log = new(4);

    /**
     * A debug message.
     *
     * @since 3.18.0
     */
    public static readonly MessageType Debug = new(5);

    public int Value { get; } = Value;
}

public class MessageTypeJsonConverter : JsonConverter<MessageType>
{
    public override MessageType Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.Number)
        {
            throw new JsonException();
        }

        return new MessageType(reader.GetInt32());
    }

    public override void Write(Utf8JsonWriter writer, MessageType value, JsonSerializerOptions options)
    {
        writer.WriteNumberValue(value.Value);
    }
}
