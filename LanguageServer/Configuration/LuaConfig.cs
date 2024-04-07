using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Workspace;
using LanguageServer.Configuration.Json;
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

    private LuaRc _dotLuaRc;
    
    private readonly ReaderWriterLockSlim _lock = new ReaderWriterLockSlim();
    
    public LuaRc DotLuaRc
    {
        get
        {
            _lock.EnterReadLock();
            try
            {
                return _dotLuaRc;
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }
        set
        {
            _lock.EnterWriteLock();
            try
            {
                _dotLuaRc = value;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
    }

    public void UpdateConfig(Action<LuaRc> updateAction)
    {
        _lock.EnterWriteLock();
        try
        {
            updateAction(DotLuaRc!);
            SaveLuaRc(LuaRcPath);
        }
        finally
        {
            _lock.ExitWriteLock();
        }
    }

    private FileSystemWatcher Watcher { get; } = new();

    public LuaConfig(ILogger<LuaConfig> logger)
    {
        Watcher.Changed += OnChanged;
        Logger = logger;
        _dotLuaRc = new();
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
            // ReSharper disable once IdentifierTypo
            var luarc = JsonConvert.DeserializeObject<LuaRc>(fileText, SerializerSettings);
            if (luarc is not null)
            {
                DotLuaRc = luarc;
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
            var json = JsonConvert.SerializeObject(DotLuaRc, SerializerSettings);
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
        var rc = DotLuaRc;
        if (rc.Workspace?.IgnoreDirs is { } ignoreDirs)
        {
            features.ExcludeFolders = ignoreDirs;
        }

        if (rc.Workspace?.PreloadFileSize is { } preloadFileSize)
        {
            features.DontIndexMaxFileSize = preloadFileSize;
        }

        if (rc.Runtime?.Version is { } version)
        {
            switch (version)
            {
                case "Lua5.1":
                {
                    features.Language.LanguageLevel = LuaLanguageLevel.Lua51;
                    break;
                }
                case "Lua5.2":
                {
                    features.Language.LanguageLevel = LuaLanguageLevel.Lua52;
                    break;
                }
                case "Lua5.3":
                {
                    features.Language.LanguageLevel = LuaLanguageLevel.Lua53;
                    break;
                }
                case "Lua5.4":
                {
                    features.Language.LanguageLevel = LuaLanguageLevel.Lua54;
                    break;
                }
                case "LuaJIT":
                {
                    features.Language.LanguageLevel = LuaLanguageLevel.LuaJIT;
                    break;
                }
            }
        }
        
        return features;
    }
}