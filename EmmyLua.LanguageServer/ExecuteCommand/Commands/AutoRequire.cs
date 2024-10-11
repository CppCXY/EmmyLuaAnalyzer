using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Workspace;
using EmmyLua.CodeAnalysis.Workspace.Module.FilenameConverter;
using EmmyLua.LanguageServer.Framework.Protocol.Model;
using EmmyLua.LanguageServer.Framework.Protocol.Model.TextEdit;

namespace EmmyLua.LanguageServer.ExecuteCommand.Commands;

public class AutoRequire : ICommandBase
{
    private static readonly string CommandName = "emmy.autoRequire";

    public string Name { get; } = CommandName;

    public async Task ExecuteAsync(List<LSPAny>? parameters, CommandExecutor executor)
    {
        if (parameters is not { Count: 3 })
        {
            return;
        }

        var uri = string.Empty;
        var range = new DocumentRange();
        var requiredText = string.Empty;
        executor.Context.ReadyRead(() =>
        {
            var currentId = new LuaDocumentId(parameters[0].Value is int { } intId ? intId : 0);
            var needRequireId = new LuaDocumentId(parameters[1].Value is int { } intId2 ? intId2 : 0);
            var position = parameters[2].Value is int { } intPosition ? intPosition : 0;
            var currentDocument = executor.Context.LuaProject.GetDocument(currentId);
            if (currentDocument is null) return;
            var sourceBlock = currentDocument.SyntaxTree.SyntaxRoot.Block;
            if (sourceBlock is null) return;
            LuaStatSyntax? lastRequireStat = null;
            foreach (var stat in sourceBlock.ChildrenNode.OfType<LuaStatSyntax>())
            {
                if (stat.Position > position)
                {
                    break;
                }

                if (IsRequireStat(stat, executor.Context.LuaProject.Features))
                {
                    lastRequireStat = stat;
                }
            }

            if (lastRequireStat != null)
            {
                var line = currentDocument.GetLine(lastRequireStat.Range.EndOffset) + 1;
                range = new DocumentRange(new(line, 0), new(line, 0));
            }

            var module = executor.Context.LuaProject.ModuleManager.GetModuleInfo(needRequireId);
            if (module is null) return;
            var convention = executor.Context.SettingManager.Setting?.Completion.AutoRequireNamingConvention
                             ?? FilenameConvention.SnakeCase;
            var id = FilenameConverter.ConvertToIdentifier(module.Name, convention);
            var requireFunction = executor.Context.SettingManager
                                      .Setting?.Completion.AutoRequireFunction
                                  ?? "require";
            requiredText = $"local {id} = {requireFunction}(\"{module.ModulePath}\")\n";
            uri = currentDocument.Uri;
        });

        if (requiredText.Length != 0)
        {
            await executor.ApplyEditAsync(uri, new TextEdit()
            {
                NewText = requiredText,
                Range = range
            });
        }
    }

    public static Command MakeCommand(string title, LuaDocumentId currentId, LuaDocumentId needRequireId, int position)
    {
        return new Command()
        {
            Title = title,
            Name = CommandName,
            Arguments =
            [
                currentId.Id,
                needRequireId.Id,
                position
            ]
        };
    }

    private static bool IsRequireStat(LuaStatSyntax stat, LuaFeatures features)
    {
        if (stat is LuaLocalStatSyntax localStat)
        {
            foreach (var expr in localStat.ExprList)
            {
                if (expr is LuaCallExprSyntax { Name: { } name } && features.RequireLikeFunction.Contains(name))
                {
                    return true;
                }
            }
        }
        else if (stat is LuaAssignStatSyntax assignStat)
        {
            foreach (var expr in assignStat.ExprList)
            {
                if (expr is LuaCallExprSyntax { Name: { } name } && features.RequireLikeFunction.Contains(name))
                {
                    return true;
                }
            }
        }
        else if (stat is LuaCallStatSyntax callStat)
        {
            if (callStat.CallExpr is LuaCallExprSyntax { Name: { } name } && features.RequireLikeFunction.Contains(name))
            {
                return true;
            }
        }

        return false;
    }
}