using EmmyLua.CodeAnalysis.Diagnostics;
using EmmyLua.Configuration;
using EmmyLua.LanguageServer.Framework.Protocol.Model;
using DiagnosticCode = EmmyLua.CodeAnalysis.Diagnostics.DiagnosticCode;

namespace EmmyLua.LanguageServer.ExecuteCommand.Commands;

public enum SetConfigAction
{
    None,
    Add,
    Set,

    // TODO
    Remove
}

static class SetConfigActionHelper
{
    public static string ToConfigString(this SetConfigAction action)
    {
        return action switch
        {
            SetConfigAction.Add => "add",
            SetConfigAction.Set => "set",
            SetConfigAction.Remove => "remove",
            _ => "none"
        };
    }

    public static SetConfigAction FromConfigString(string action)
    {
        return action switch
        {
            "add" => SetConfigAction.Add,
            "set" => SetConfigAction.Set,
            "remove" => SetConfigAction.Remove,
            _ => SetConfigAction.None
        };
    }
}

public class SetConfig : ICommandBase
{
    private static readonly string CommandName = "emmy.setConfig";

    public string Name => CommandName;

    public async Task ExecuteAsync(List<LSPAny>? parameters, CommandExecutor executor)
    {
        if (parameters is not { Count: 3 })
        {
            return;
        }

        var action =
            SetConfigActionHelper.FromConfigString(parameters[0].Value is string actionName
                ? actionName
                : string.Empty);
        var path = parameters[1].Value is string pathValue ? pathValue : string.Empty;
        var value = parameters[2].Value is string strValue ? strValue : string.Empty;
        executor.Context.ReadyWrite(() =>
        {
            var config = executor.Context.SettingManager.Setting ?? new Setting();

            switch (action)
            {
                case SetConfigAction.Add:
                {
                    var property = GetPropertyByName(config, path);
                    if (property is List<string> list)
                    {
                        list.Add(value);
                    }
                    else if (property is List<DiagnosticCode> list2)
                    {
                        list2.Add(DiagnosticCodeHelper.GetCode(value));
                    }

                    executor.Context.SettingManager.Save(config);
                    break;
                }
                case SetConfigAction.Set:
                {
                    break;
                }
            }
        });
    }

    private object? GetPropertyByName(Setting setting, string propertyName)
    {
        var parts = propertyName.Split('.');
        object? obj = setting;
        foreach (var part in parts)
        {
            if (obj == null) return null;

            var type = obj.GetType();
            var property = type.GetProperty(part);
            if (property == null) return null;

            obj = property.GetValue(obj);
        }

        return obj;
    }

    public static Command MakeCommand(string title, SetConfigAction action, string path, string value)
    {
        return new Command()
        {
            Title = title,
            Name = CommandName,
            Arguments = [action.ToConfigString(), path, value]
        };
    }
}