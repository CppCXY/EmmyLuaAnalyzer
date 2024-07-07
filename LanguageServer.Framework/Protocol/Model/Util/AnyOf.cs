using System.Text.Json;
using System.Text.Json.Serialization;

namespace EmmyLua.LanguageServer.Framework.Protocol.Model.Util;

public class AnyOf<T1, T2>
{
    private readonly object? _value;
    
    public AnyOf(T1 value)
    {
        _value = value;
    }
    
    public AnyOf(T2 value)
    {
        _value = value;
    }
    
    public void Match(Action<T1> f1, Action<T2> f2)
    {
        switch (_value)
        {
            case T1 t1:
                f1(t1);
                break;
            case T2 t2:
                f2(t2);
                break;
            default:
                throw new InvalidOperationException();
        }
    }
}

public class AnyOf<T1, T2, T3>
{
    private readonly object? _value;
    
    public AnyOf(T1 value)
    {
        _value = value;
    }
    
    public AnyOf(T2 value)
    {
        _value = value;
    }
    
    public AnyOf(T3 value)
    {
        _value = value;
    }
    
    public void Match(Action<T1> f1, Action<T2> f2, Action<T3> f3)
    {
        switch (_value)
        {
            case T1 t1:
                f1(t1);
                break;
            case T2 t2:
                f2(t2);
                break;
            case T3 t3:
                f3(t3);
                break;
            default:
                throw new InvalidOperationException();
        }
    }
}

public class AnyOf<T1, T2, T3, T4>
{
    private readonly object? _value;
    
    public AnyOf(T1 value)
    {
        _value = value;
    }
    
    public AnyOf(T2 value)
    {
        _value = value;
    }
    
    public AnyOf(T3 value)
    {
        _value = value;
    }
    
    public AnyOf(T4 value)
    {
        _value = value;
    }
    
    public void Match(Action<T1> f1, Action<T2> f2, Action<T3> f3, Action<T4> f4)
    {
        switch (_value)
        {
            case T1 t1:
                f1(t1);
                break;
            case T2 t2:
                f2(t2);
                break;
            case T3 t3:
                f3(t3);
                break;
            case T4 t4:
                f4(t4);
                break;
            default:
                throw new InvalidOperationException();
        }
    }
}

public class AnyOf<T1, T2, T3, T4, T5>
{
    private readonly object? _value;
    
    public AnyOf(T1 value)
    {
        _value = value;
    }
    
    public AnyOf(T2 value)
    {
        _value = value;
    }
    
    public AnyOf(T3 value)
    {
        _value = value;
    }
    
    public AnyOf(T4 value)
    {
        _value = value;
    }
    
    public AnyOf(T5 value)
    {
        _value = value;
    }
    
    public void Match(Action<T1> f1, Action<T2> f2, Action<T3> f3, Action<T4> f4, Action<T5> f5)
    {
        switch (_value)
        {
            case T1 t1:
                f1(t1);
                break;
            case T2 t2:
                f2(t2);
                break;
            case T3 t3:
                f3(t3);
                break;
            case T4 t4:
                f4(t4);
                break;
            case T5 t5:
                f5(t5);
                break;
            default:
                throw new InvalidOperationException();
        }
    }
}

public class AnyOfJsonConverter2<T1, T2> : JsonConverter<AnyOf<T1, T2>>
{
    public override AnyOf<T1, T2> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var jsonDoc = JsonDocument.ParseValue(ref reader);
        var root = jsonDoc.RootElement;
        try
        {
            var item1 = JsonSerializer.Deserialize<T1>(root.GetRawText(), options);
            return new AnyOf<T1, T2>(item1);
        }
        catch (JsonException)
        {
            var item2 = JsonSerializer.Deserialize<T2>(root.GetRawText(), options);
            return new AnyOf<T1, T2>(item2);
        }
    }

    public override void Write(Utf8JsonWriter writer, AnyOf<T1, T2> value, JsonSerializerOptions options)
    {
        value.Match(
            v1 => JsonSerializer.Serialize(writer, v1, options),
            v2 => JsonSerializer.Serialize(writer, v2, options)
        );
    }
}

public class AnyOfJsonConverter3<T1, T2, T3> : JsonConverter<AnyOf<T1, T2, T3>>
{
    public override AnyOf<T1, T2, T3> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var jsonDoc = JsonDocument.ParseValue(ref reader);
        var root = jsonDoc.RootElement;
        try
        {
            var item1 = JsonSerializer.Deserialize<T1>(root.GetRawText(), options);
            return new AnyOf<T1, T2, T3>(item1);
        }
        catch (JsonException)
        {
            try
            {
                var item2 = JsonSerializer.Deserialize<T2>(root.GetRawText(), options);
                return new AnyOf<T1, T2, T3>(item2);
            }
            catch (JsonException)
            {
                var item3 = JsonSerializer.Deserialize<T3>(root.GetRawText(), options);
                return new AnyOf<T1, T2, T3>(item3);
            }
        }
    }

    public override void Write(Utf8JsonWriter writer, AnyOf<T1, T2, T3> value, JsonSerializerOptions options)
    {
        value.Match(
            v1 => JsonSerializer.Serialize(writer, v1, options),
            v2 => JsonSerializer.Serialize(writer, v2, options),
            v3 => JsonSerializer.Serialize(writer, v3, options)
        );
    }
}

public class AnyOfJsonConverter4<T1, T2, T3, T4> : JsonConverter<AnyOf<T1, T2, T3, T4>>
{
    public override AnyOf<T1, T2, T3, T4> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var jsonDoc = JsonDocument.ParseValue(ref reader);
        var root = jsonDoc.RootElement;
        try
        {
            var item1 = JsonSerializer.Deserialize<T1>(root.GetRawText(), options);
            return new AnyOf<T1, T2, T3, T4>(item1);
        }
        catch (JsonException)
        {
            try
            {
                var item2 = JsonSerializer.Deserialize<T2>(root.GetRawText(), options);
                return new AnyOf<T1, T2, T3, T4>(item2);
            }
            catch (JsonException)
            {
                try
                {
                    var item3 = JsonSerializer.Deserialize<T3>(root.GetRawText(), options);
                    return new AnyOf<T1, T2, T3, T4>(item3);
                }
                catch (JsonException)
                {
                    var item4 = JsonSerializer.Deserialize<T4>(root.GetRawText(), options);
                    return new AnyOf<T1, T2, T3, T4>(item4);
                }
            }
        }
    }

    public override void Write(Utf8JsonWriter writer, AnyOf<T1, T2, T3, T4> value, JsonSerializerOptions options)
    {
        value.Match(
            v1 => JsonSerializer.Serialize(writer, v1, options),
            v2 => JsonSerializer.Serialize(writer, v2, options),
            v3 => JsonSerializer.Serialize(writer, v3, options),
            v4 => JsonSerializer.Serialize(writer, v4, options)
        );
    }
}

public class AnyOfJsonConverter5<T1, T2, T3, T4, T5> : JsonConverter<AnyOf<T1, T2, T3, T4, T5>>
{
    public override AnyOf<T1, T2, T3, T4, T5> Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var jsonDoc = JsonDocument.ParseValue(ref reader);
        var root = jsonDoc.RootElement;
        try
        {
            var item1 = JsonSerializer.Deserialize<T1>(root.GetRawText(), options);
            return new AnyOf<T1, T2, T3, T4, T5>(item1);
        }
        catch (JsonException)
        {
            try
            {
                var item2 = JsonSerializer.Deserialize<T2>(root.GetRawText(), options);
                return new AnyOf<T1, T2, T3, T4, T5>(item2);
            }
            catch (JsonException)
            {
                try
                {
                    var item3 = JsonSerializer.Deserialize<T3>(root.GetRawText(), options);
                    return new AnyOf<T1, T2, T3, T4, T5>(item3);
                }
                catch (JsonException)
                {
                    try
                    {
                        var item4 = JsonSerializer.Deserialize<T4>(root.GetRawText(), options);
                        return new AnyOf<T1, T2, T3, T4, T5>(item4);
                    }
                    catch (JsonException)
                    {
                        var item5 = JsonSerializer.Deserialize<T5>(root.GetRawText(), options);
                        return new AnyOf<T1, T2, T3, T4, T5>(item5);
                    }
                }
            }
        }
    }

    public override void Write(Utf8JsonWriter writer, AnyOf<T1, T2, T3, T4, T5> value, JsonSerializerOptions options)
    {
        value.Match(
            v1 => JsonSerializer.Serialize(writer, v1, options),
            v2 => JsonSerializer.Serialize(writer, v2, options),
            v3 => JsonSerializer.Serialize(writer, v3, options),
            v4 => JsonSerializer.Serialize(writer, v4, options),
            v5 => JsonSerializer.Serialize(writer, v5, options)
        );
    }
}