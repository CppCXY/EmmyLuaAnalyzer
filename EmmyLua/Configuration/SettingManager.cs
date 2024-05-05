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
        if (e.ChangeType is WatcherChangeTypes.Changed or WatcherChangeTypes.Created)
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
            Watcher.Created += OnChanged;
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
                ProcessSetting(Setting);
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

    private void ProcessSetting(Setting setting)
    {
        setting.Workspace.WorkspaceRoots = setting.Workspace.WorkspaceRoots.Select(PreProcessPath).ToList();
        setting.Workspace.Library = setting.Workspace.Library.Select(PreProcessPath).ToList();
        setting.Resource.Paths = setting.Resource.Paths.Select(PreProcessPath).ToList();
        if (setting.Resource.Paths.Count == 0)
        {
            setting.Resource.Paths.Add(Workspace);
        }
    }

    private string PreProcessPath(string path)
    {
        if (path.StartsWith('~'))
        {
            path = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), path[1..]);
        }
        else if (path.StartsWith("./"))
        {
            path = Path.Combine(Workspace, path[2..]);
        }
        else if (path.StartsWith("/"))
        {
            path = Path.Combine(Workspace, path.TrimStart('/'));
        }

        path = path.Replace("${workspaceFolder}", Workspace);
        return Path.GetFullPath(path);
    }

    public LuaFeatures GetLuaFeatures()
    {
        var features = new LuaFeatures();
        if (Setting is null)
        {
            return features;
        }

        var setting = Setting;
        features.ExcludeFolders.UnionWith(setting.Workspace.IgnoreDir);
        features.DontIndexMaxFileSize = setting.Workspace.PreloadFileSize;
        features.ThirdPartyRoots.AddRange(setting.Workspace.Library);
        features.WorkspaceRoots.AddRange(setting.Workspace.WorkspaceRoots);
        features.Language.LanguageLevel = setting.Runtime.Version;
        features.DiagnosticConfig.Globals.UnionWith(setting.Diagnostics.Globals);
        features.DiagnosticConfig.WorkspaceDisabledCodes.UnionWith(setting.Diagnostics.Disable);
        features.RequireLikeFunction.UnionWith(setting.Runtime.RequireLikeFunction);
        return features;
    }

    public void Save(Setting setting)
    {
        var json = JsonConvert.SerializeObject(setting, SerializerSettings);
        File.WriteAllText(SettingPath, json);
    }
}
