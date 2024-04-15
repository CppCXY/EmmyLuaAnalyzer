namespace EmmyLua.CodeAnalysis.Configuration;

public class LuaConfig
{
    private ConfigMap? Root { get; set; }

    public static LuaConfig From(string text)
    {
        return new LuaConfig();
    }

    public void Parse(string text)
    {
        Root = new ConfigMap(new List<(string, ConfigElement)>());
    }
}
