using EmmyLua.Configuration;
using EmmyLua.LanguageServer.CodeLens;
using EmmyLua.LanguageServer.Completion;
using EmmyLua.LanguageServer.InlayHint;
using EmmyLua.LanguageServer.Server.Resource;
using EmmyLua.LanguageServer.SignatureHelper;

namespace EmmyLua.LanguageServer.Util;

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
        config.AutoRequireFilenameConvention = setting.Completion.AutoRequireFilenameConvention;

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

    public static CodeLensConfig GetCodeLensConfig(this SettingManager settingManager)
    {
        var config = new CodeLensConfig();
        var setting = settingManager.Setting;
        if (setting is null)
        {
            return config;
        }

        config.Enable = setting.CodeLens.Enable;

        return config;
    }

    public static SignatureHelperConfig GetSignatureConfig(this SettingManager settingManager)
    {
        var config = new SignatureHelperConfig();
        var setting = settingManager.Setting;
        if (setting is null)
        {
            return config;
        }

        config.DetailSignatureHelp = setting.Signature.DetailSignatureHelper;
        return config;
    }
}