using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace EmmyLua.LanguageServer.Completion.CompleteProvider;

public class RequireProvider : ICompleteProviderBase
{
    public void AddCompletion(CompleteContext context)
    {
        var trigger = context.TriggerToken;
        if (trigger is LuaStringToken modulePathToken
            && trigger.Parent?.Parent?.Parent is LuaCallExprSyntax { Name: { } funcName }
            && context.SemanticModel.Compilation.Workspace.Features.RequireLikeFunction.Contains(funcName))
        {
            var moduleInfos =
                context.SemanticModel.Compilation.Workspace.ModuleGraph.GetCurrentModuleNames(modulePathToken.Value);
            
            var modulePath = modulePathToken.Value;
            var parts = modulePath.Split('.');
            var moduleBase = string.Empty;
            if (parts.Length > 1)
            {
                moduleBase = string.Join('.', parts[..^1]);
            }

            foreach (var moduleInfo in moduleInfos)
            {
                var filterText = moduleInfo.Name;
                if (moduleBase.Length != 0)
                {
                    filterText = $"{moduleBase}.{filterText}";
                }
                
                context.Add(new CompletionItem
                {
                    Label = moduleInfo.Name,
                    Kind = moduleInfo.IsFile ? CompletionItemKind.File : CompletionItemKind.Folder,
                    Detail = moduleInfo.Uri,
                    FilterText = filterText,
                    InsertText = filterText,
                    Data = moduleInfo.DocumentId?.Id.ToString()
                });
            }
            context.StopHere();
        }
    }
}