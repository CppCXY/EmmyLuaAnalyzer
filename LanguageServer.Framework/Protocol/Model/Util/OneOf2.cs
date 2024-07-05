using System.Text.Json;
using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Model.Util;

public class OneOf2<T1, T2>
{
    private bool IsLeft { get; }
    
    public T1? Item1 { get; }
    public T2? Item2 { get; }

    public OneOf2(T1 item1)
    {
        Item1 = item1;
        IsLeft = true;
    }

    public OneOf2(T2 item2)
    {
        Item2 = item2;
        IsLeft = false;
    }
    
    public static implicit operator OneOf2<T1, T2>(T1 item1) => new(item1);
    
    public static implicit operator OneOf2<T1, T2>(T2 item2) => new(item2);
}

public class OneOf2JsonConverter<T1, T2> : JsonConverter<OneOf2<T1, T2>>
{
    public override OneOf2<T1, T2> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var jsonDoc = JsonDocument.ParseValue(ref reader);
        var root = jsonDoc.RootElement;
        try
        {
            var item1 = JsonSerializer.Deserialize<T1>(root.GetRawText(), options);
            return new OneOf2<T1, T2>(item1);
        }
        catch (JsonException)
        {
            var item2 = JsonSerializer.Deserialize<T2>(root.GetRawText(), options);
            return new OneOf2<T1, T2>(item2);
        }
    }

    public override void Write(Utf8JsonWriter writer, OneOf2<T1, T2> value, JsonSerializerOptions options)
    {
        if (value.Item1 != null)
        {
            JsonSerializer.Serialize(writer, value.Item1, options);
        }
        else if (value.Item2 != null)
        {
            JsonSerializer.Serialize(writer, value.Item2, options);
        }
    }
}