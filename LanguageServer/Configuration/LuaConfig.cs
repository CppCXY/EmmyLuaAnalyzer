using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace LanguageServer.Configuration;

public class LuaConfig
{
    private ILogger<LuaConfig> Logger { get; }
    
    private JsonSerializerSettings SerializerSettings { get; } = new()
    {
        NullValueHandling = NullValueHandling.Ignore,
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        Formatting = Formatting.Indented
    };

    private string LuaRcPath { get; set; } = string.Empty;

    private LuaRc? DotLuaRc { get; set; }

    private FileSystemWatcher Watcher { get; } = new();

    public LuaConfig(ILogger<LuaConfig> logger)
    {
        Watcher.Changed += OnChanged;
        Logger = logger;
    }

    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        if (e.ChangeType == WatcherChangeTypes.Changed)
        {
            LoadLuaRc(LuaRcPath);
        }
    }

    public void Watch(string path)
    {
        LuaRcPath = path;
        if (Path.GetDirectoryName(path) is { } directoryName)
        {
            Watcher.Path = directoryName;
            Watcher.Filter = Path.GetFileName(path);
            Watcher.EnableRaisingEvents = true;
        }

        LoadLuaRc(path);
    }

    private void LoadLuaRc(string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                return;
            }

            var fileText = File.ReadAllText(path);
            DotLuaRc = JsonConvert.DeserializeObject<LuaRc>(fileText, SerializerSettings);
        }
        catch (Exception exception)
        {
            Logger.LogError(exception.ToString());
        }
    }
    
    private void SaveLuaRc(string path)
    {
        try
        {
            if (DotLuaRc is null)
            {
                return;
            }

            var json = JsonConvert.SerializeObject(DotLuaRc, SerializerSettings);
            File.WriteAllText(path, json);
        }
        catch (Exception exception)
        {
            Logger.LogError(exception.ToString());
        }
    }
}