using EmmyLua.Configuration;
using LanguageServer.Completion;
using LanguageServer.InlayHint;
using LanguageServer.Server.Resource;

namespace LanguageServer.Configuration;

public static class SettingManagerExtension
{
    public static CompletionConfig GetCompletionConfig(this SettingManager settingManager)
    {
        var config = new CompletionConfig();
        var setting = settingManager.Setting;
        if (setting is null)
        {
            return config;
        }

        config.AutoRequire = setting.Completion.AutoRequire;
        config.CallSnippet = setting.Completion.CallSnippet;

        return config;
    }
    
    public static InlayHintConfig GetInlayHintConfig(this SettingManager settingManager)
    {
        var config = new InlayHintConfig();
        var setting = settingManager.Setting;
        if (setting is null)
        {
            return config;
        }

        config.ParamHint = setting.Hint.ParamHint;
        config.IndexHint = setting.Hint.IndexHint;
        config.LocalHint = setting.Hint.LocalHint;
        config.OverrideHint = setting.Hint.OverrideHint;

        return config;
    }
    
    public static ResourceConfig GetResourceConfig(this SettingManager settingManager)
    {
        var config = new ResourceConfig();
        var setting = settingManager.Setting;
        if (setting is null)
        {
            return config;
        }

        config.Paths = setting.Resource.Paths;

        return config;
    }
}