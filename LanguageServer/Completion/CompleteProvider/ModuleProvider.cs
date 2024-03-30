using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using LanguageServer.ExecuteCommand.Commands;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace LanguageServer.Completion.CompleteProvider;

public class ModuleProvider : ICompleteProviderBase
{
    private HashSet<string> BuiltinModules { get; } =
    [
        "io", "os", "string", "table", "math", "debug", "coroutine", "package", "bit32", "utf8"
    ];

    public void AddCompletion(CompleteContext context)
    {
        if (context.TriggerToken?.Parent is not LuaNameExprSyntax nameExpr)
        {
            return;
        }

        var semanticModel = context.SemanticModel;
        var modules = semanticModel.Compilation.Workspace.ModuleGraph.GetAllModules();
        var localNames = semanticModel.GetDeclarations(context.TriggerToken).Select(it=>it.Name).ToHashSet();
        foreach (var module in modules)
        {
            if (AllowModule(module.Name, localNames))
            {
                context.Add(new CompletionItem
                {
                    Label = module.Name,
                    Kind = CompletionItemKind.Module,
                    LabelDetails = new CompletionItemLabelDetails()
                    {
                        Detail = $" (in {module.ModulePath})",
                    },
                    Data = module.DocumentId.ToString(),
                    Command = AutoRequire.MakeCommand(
                        string.Empty, semanticModel.Document.Id, module.DocumentId,
                        nameExpr.Position)
                });
            }
        }
    }

    private bool AllowModule(string name, HashSet<string> localNames)
    {
        if (BuiltinModules.Contains(name))
        {
            return false;
        }

        if (localNames.Contains(name))
        {
            return false;
        }

        return true;
    }
}