using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Workspace;
using MediatR;
using Newtonsoft.Json.Linq;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace LanguageServer.ExecuteCommand.Commands;

public class AutoRequire : ICommandBase
{
    private static readonly string CommandName = "emmy.autoRequire";

    public string Name { get; } = CommandName;

    public async Task<Unit> ExecuteAsync(JArray? parameters, CommandExecutor executor)
    {
        if (parameters is not { Count: 3 })
        {
            return await Unit.Task;
        }

        var uri = string.Empty;
        var range = new Range(0, 0, 0, 0);
        var requiredText = string.Empty;
        executor.Context.ReadyRead(() =>
        {
            var currentId = new LuaDocumentId(parameters[0].Value<int>());
            var needRequireId = new LuaDocumentId(parameters[1].Value<int>());
            var position = parameters[2].Value<int>();
            var currentDocument = executor.Context.LuaWorkspace.GetDocument(currentId);
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
            
                if (IsRequireStat(stat, executor.Context.LuaWorkspace.Features))
                {
                    lastRequireStat = stat;
                }
            }
            
            if (lastRequireStat != null)
            {
                var line = currentDocument.GetLine(lastRequireStat.Range.EndOffset) + 1;
                range = new Range(line, 0, line, 0);
            }

            var module = executor.Context.LuaWorkspace.ModuleGraph.GetModuleInfo(needRequireId);
            if (module is null) return;
            var requireFunction = executor.Context.SettingManager
                .Setting?.Completion.AutoRequireFunction
                ?? "require";
            requiredText = $"local {module.Name} = {requireFunction}(\"{module.ModulePath}\")\n";
            uri = currentDocument.Uri;
        });

        if (requiredText.Length != 0)
        {
            return await executor.ApplyEditAsync(uri, new TextEdit()
            {
                NewText = requiredText,
                Range = range
            });
        }

        return await Unit.Task;
    }

    public static Command MakeCommand(string title, LuaDocumentId currentId, LuaDocumentId needRequireId, int position)
    {
        return new Command()
        {
            Title = title,
            Name = CommandName,
            Arguments = new JArray()
            {
                currentId.Id.ToString(),
                needRequireId.Id.ToString(),
                position.ToString()
            }
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
            if (callStat.Expr is LuaCallExprSyntax { Name: { } name } && features.RequireLikeFunction.Contains(name))
            {
                return true;
            }
        }

        return false;
    }
}