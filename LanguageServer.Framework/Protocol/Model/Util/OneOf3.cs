using System.Text.Json;
using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Model.Util;

public class OneOf3<T1, T2, T3>
{
    private readonly T1? _value1;
    private readonly T2? _value2;
    private readonly T3? _value3;
    private readonly int _index;

    public OneOf3(T1 value)
    {
        _value1 = value;
        _index = 0;
    }

    public OneOf3(T2 value)
    {
        _value2 = value;
        _index = 1;
    }

    public OneOf3(T3 value)
    {
        _value3 = value;
        _index = 2;
    }

    public T1 Value1 => _value1 ?? throw new InvalidOperationException("No value");
    public T2 Value2 => _value2 ?? throw new InvalidOperationException("No value");
    public T3 Value3 => _value3 ?? throw new InvalidOperationException("No value");

    public int Index => _index;

    public static implicit operator OneOf3<T1, T2, T3>(T1 value) => new OneOf3<T1, T2, T3>(value);
    public static implicit operator OneOf3<T1, T2, T3>(T2 value) => new OneOf3<T1, T2, T3>(value);
    public static implicit operator OneOf3<T1, T2, T3>(T3 value) => new OneOf3<T1, T2, T3>(value);
}

public class OneOf3JsonConverter<T1, T2, T3> : JsonConverter<OneOf3<T1, T2, T3>>
{
    public override OneOf3<T1, T2, T3> Read(ref Utf8JsonReader reader, Type typeToConvert,
        JsonSerializerOptions options)
    {
        using var jsonDoc = JsonDocument.ParseValue(ref reader);
        var root = jsonDoc.RootElement;
        try
        {
            var item1 = JsonSerializer.Deserialize<T1>(root.GetRawText(), options);
            return new OneOf3<T1, T2, T3>(item1);
        }
        catch (JsonException)
        {
            try
            {
                var item2 = JsonSerializer.Deserialize<T2>(root.GetRawText(), options);
                return new OneOf3<T1, T2, T3>(item2);
            }
            catch (JsonException)
            {
                var item3 = JsonSerializer.Deserialize<T3>(root.GetRawText(), options);
                return new OneOf3<T1, T2, T3>(item3);
            }
        }
    }

    public override void Write(Utf8JsonWriter writer, OneOf3<T1, T2, T3> value, JsonSerializerOptions options)
    {
        switch (value.Index)
        {
            case 0:
                JsonSerializer.Serialize(writer, value.Value1, options);
                break;
            case 1:
                JsonSerializer.Serialize(writer, value.Value2, options);
                break;
            case 2:
                JsonSerializer.Serialize(writer, value.Value3, options);
                break;
            default:
                throw new InvalidOperationException("Invalid index");
        }
    }
}