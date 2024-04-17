using EmmyLua.Configuration;
using LanguageServer.Completion;

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
}