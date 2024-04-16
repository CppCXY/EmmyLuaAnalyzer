using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Workspace;
using EmmyLua.Configuration;
using LanguageServer.Server;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Path = System.IO.Path;

namespace LanguageServer.Configuration;

public class LuaConfig
{
    private ILogger<ServerContext> Logger { get; }
    
    private JsonSerializerSettings SerializerSettings { get; } = new()
    {
        NullValueHandling = NullValueHandling.Ignore,
        ContractResolver = new CamelCasePropertyNamesContractResolver(),
        Formatting = Formatting.Indented
    };

    private string LuaRcPath { get; set; } = string.Empty;

    private Setting _setting;
    
    private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
    
    public Setting Setting
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return _setting;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
        private set
        {
            _lock.EnterWriteLock();
            try
            {
                _setting = value;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }

    public void UpdateConfig(Action<Setting> updateAction)
    {
        _lock.EnterWriteLock();
        try
        {
            updateAction(Setting!);
            SaveLuaRc(LuaRcPath);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    private FileSystemWatcher Watcher { get; } = new();

    public LuaConfig(ILogger<ServerContext> logger)
    {
        Watcher.Changed += OnChanged;
        Logger = logger;
        _setting = new();
    }

    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        if (e.ChangeType == WatcherChangeTypes.Changed)
        {
            LoadSetting(LuaRcPath);
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

        LoadSetting(path);
    }

    private void LoadSetting(string path)
    {
        try
        {
            if (!File.Exists(path))
            {
                return;
            }

            var fileText = File.ReadAllText(path);
            // ReSharper disable once IdentifierTypo
            var luarc = JsonConvert.DeserializeObject<Setting>(fileText, SerializerSettings);
            if (luarc is not null)
            {
                Setting = luarc;
            }
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
            var json = JsonConvert.SerializeObject(Setting, SerializerSettings);
            File.WriteAllText(path, json);
        }
        catch (Exception exception)
        {
            Logger.LogError(exception.ToString());
        }
    }
    
    public LuaFeatures GetFeatures()
    {
        var features = new LuaFeatures();
        var rc = Setting;
        if (rc.Workspace?.IgnoreDir is { } ignoreDir)
        {
            features.ExcludeFolders = ignoreDir.ToList();
        }

        if (rc.Workspace?.PreloadFileSize is { } preloadFileSize)
        {
            features.DontIndexMaxFileSize = preloadFileSize;
        }

        if (rc.Runtime?.Version is { } version)
        {
            switch (version)
            {
                case SettingRuntimeVersion.Lua_5_1:
                {
                    features.Language.LanguageLevel = LuaLanguageLevel.Lua51;
                    break;
                }
                case SettingRuntimeVersion.Lua_5_2:
                {
                    features.Language.LanguageLevel = LuaLanguageLevel.Lua52;
                    break;
                }
                case SettingRuntimeVersion.Lua_5_3:
                {
                    features.Language.LanguageLevel = LuaLanguageLevel.Lua53;
                    break;
                }
                case SettingRuntimeVersion.Lua_5_4:
                {
                    features.Language.LanguageLevel = LuaLanguageLevel.Lua54;
                    break;
                }
                case SettingRuntimeVersion.LuaJIT:
                {
                    features.Language.LanguageLevel = LuaLanguageLevel.LuaJIT;
                    break;
                }
            }
        }
        
        return features;
    }
}