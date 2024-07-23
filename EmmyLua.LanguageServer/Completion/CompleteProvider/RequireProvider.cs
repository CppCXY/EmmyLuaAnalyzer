using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Completion;
using EmmyLua.LanguageServer.Framework.Protocol.Model;

namespace EmmyLua.LanguageServer.Completion.CompleteProvider;

public class RequireProvider : ICompleteProviderBase
{
    // support stupid code
    private char[] Separators { get; } = ['.', '/', '\\'];

    public void AddCompletion(CompleteContext context)
    {
        var trigger = context.TriggerToken;
        if (trigger is LuaStringToken modulePathToken
            && trigger.Parent?.Parent?.Parent is LuaCallExprSyntax { Name: { } funcName }
            && context.SemanticModel.Compilation.Project.Features.RequireLikeFunction.Contains(funcName))
        {
            var moduleInfos =
                context.SemanticModel.Compilation.Project.ModuleManager.GetCurrentModuleNames(modulePathToken.Value);

            var modulePath = modulePathToken.Value;
            var index = modulePath.LastIndexOfAny(Separators);
            var moduleBase = string.Empty;
            if (index != -1)
            {
                moduleBase = modulePath[..(index + 1)];
            }

            foreach (var moduleInfo in moduleInfos)
            {
                var filterText = moduleInfo.Name;
                if (moduleBase.Length != 0)
                {
                    filterText = $"{moduleBase}{filterText}";
                }

                context.Add(new CompletionItem
                {
                    Label = moduleInfo.Name,
                    Kind = moduleInfo.IsFile ? CompletionItemKind.File : CompletionItemKind.Folder,
                    Detail = moduleInfo.Uri,
                    FilterText = filterText,
                    InsertText = filterText,
                    Data = new LSPAny(moduleInfo.DocumentId?.Id.ToString())
                });
            }

            context.StopHere();
        }
    }
}