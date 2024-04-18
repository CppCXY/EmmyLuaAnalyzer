using EmmyLua.CodeAnalysis.Workspace;
using Newtonsoft.Json;


namespace EmmyLua.Configuration;

public class SettingManager
{
    public static readonly string ConfigName = ".emmyrc.json";

    private string Workspace { get; set; } = string.Empty;

    private string SettingPath => Path.Combine(Workspace, ConfigName);

    public Setting? Setting { get; private set; }

    public delegate void SettingChanged(SettingManager settingManager);

    public event SettingChanged? OnSettingChanged;

    private bool _firstLoad = true;

    private JsonSerializerSettings SerializerSettings { get; } = new()
    {
        Formatting = Formatting.Indented
    };

    private FileSystemWatcher? Watcher { get; set; }

    private System.Timers.Timer? _timer;

    private void OnChanged(object sender, FileSystemEventArgs e)
    {
        if (e.ChangeType == WatcherChangeTypes.Changed)
        {
            _timer?.Stop();
            _timer = new System.Timers.Timer(500); // 设置延迟时间为500毫秒
            _timer.Elapsed += (s, ee) => LoadSetting(SettingPath);
            _timer.AutoReset = false;
            _timer.Start();
        }
    }

    public void Watch(string workspace)
    {
        Workspace = workspace;
        if (Watcher is null)
        {
            Watcher = new FileSystemWatcher();
            Watcher.Changed += OnChanged;
            Watcher.Path = Workspace;
            Watcher.Filter = ConfigName;
            Watcher.EnableRaisingEvents = true;
        }

        LoadSetting(SettingPath);
    }

    public void LoadSetting(string settingPath)
    {
        try
        {
            if (!File.Exists(settingPath))
            {
                return;
            }

            var fileText = File.ReadAllText(settingPath);
            // ReSharper disable once IdentifierTypo
            var setting = JsonConvert.DeserializeObject<Setting>(fileText, SerializerSettings);
            if (setting is not null)
            {
                Setting = setting;
            }

            if (!_firstLoad)
            {
                OnSettingChanged?.Invoke(this);
            }

            _firstLoad = false;
        }
        catch (Exception e)
        {
            // ignore
        }
    }

    public LuaFeatures GetLuaFeatures()
    {
        var features = new LuaFeatures();
        if (Setting is null)
        {
            return features;
        }

        var setting = Setting;
        var excludeHash = features.ExcludeFolders.ToHashSet();
        features.ExcludeFolders.AddRange(setting.Workspace.IgnoreDir.Where(it => !excludeHash.Contains(it)));
        features.DontIndexMaxFileSize = setting.Workspace.PreloadFileSize;
        features.ThirdPartyRoots.AddRange(setting.Workspace.Library);
        features.Language.LanguageLevel = setting.Runtime.Version;
        features.DiagnosticConfig.Globals.UnionWith(setting.Diagnostics.Globals);
        features.DiagnosticConfig.WorkspaceDisabledCodes.UnionWith(setting.Diagnostics.Disable);
        return features;
    }

    public void Save(Setting setting)
    {
        var json = JsonConvert.SerializeObject(setting, SerializerSettings);
        File.WriteAllText(SettingPath, json);
    }
}
