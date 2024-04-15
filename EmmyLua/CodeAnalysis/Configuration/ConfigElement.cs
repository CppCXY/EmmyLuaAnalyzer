namespace EmmyLua.CodeAnalysis.Configuration;

public class ConfigElement(ConfigElementKind kind)
{
    public ConfigElementKind Kind { get; } = kind;
}

public class ConfigMap(List<(string, ConfigElement)> map) : ConfigElement(ConfigElementKind.Map)
{
    public List<(string, ConfigElement)> Map { get; } = map;
}

public class ConfigArray(List<ConfigElement> elements) : ConfigElement(ConfigElementKind.Array)
{
    public List<ConfigElement> Elements { get; } = elements;
}

public abstract class ConfigValue() : ConfigElement(ConfigElementKind.Value)
{
    public sealed class ConfigString(string value) : ConfigValue
    {
        public string Value { get; } = value;
    }

    public sealed class ConfigNumber(double value) : ConfigValue
    {
        public double Value { get; } = value;
    }

    public sealed class ConfigBoolean(bool value) : ConfigValue
    {
        public bool Value { get; } = value;
    }
}
