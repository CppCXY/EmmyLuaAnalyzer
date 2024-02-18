using System.Reflection;

namespace EmmyLua.Plugin;

public class PluginLoader
{
    public IPlugin? LoadPlugin(string path)
    {
        var assembly = Assembly.LoadFrom(path);
        var types = assembly.GetTypes();
        foreach (var type in types)
        {
            if (type.GetInterface(nameof(IPlugin)) != null)
            {
                return (IPlugin?) Activator.CreateInstance(type);
            }
        }

        return null;
    }
}
