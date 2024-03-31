using EmmyLua.CodeAnalysis.Compilation.Semantic;
using EmmyLua.CodeAnalysis.Compilation.Semantic.Render;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Workspace.Module;
using LanguageServer.ExecuteCommand.Commands;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace LanguageServer.Completion.CompleteProvider;

public class ModuleProvider : ICompleteProviderBase
{
    private HashSet<string> BuiltinModules { get; } =
    [
        "io", "os", "string", "table", "math", "debug", "coroutine", "package", "utf8"
    ];

    public void AddCompletion(CompleteContext context)
    {
        if (context.TriggerToken?.Parent is not LuaNameExprSyntax nameExpr)
        {
            return;
        }

        var semanticModel = context.SemanticModel;
        var modules = semanticModel.Compilation.Workspace.ModuleGraph.GetAllModules();
        var localNames = semanticModel.GetDeclarations(context.TriggerToken).Select(it => it.Name).ToHashSet();
        foreach (var module in modules)
        {
            if (AllowModule(module, localNames, context.SemanticModel))
            {
                var documentId = module.DocumentId;
                var retTy = semanticModel.GetExportType(documentId);
                context.Add(new CompletionItem
                {
                    Label = module.Name,
                    Kind = CompletionItemKind.Module,
                    LabelDetails = new CompletionItemLabelDetails()
                    {
                        Detail = $" (in {module.ModulePath})",
                        Description = LuaTypeRender.RenderType(retTy, semanticModel.Context)
                    },
                    Data = module.DocumentId.Id.ToString(),
                    Command = AutoRequire.MakeCommand(
                        string.Empty, semanticModel.Document.Id, module.DocumentId,
                        nameExpr.Position)
                });
            }
        }
    }

    private bool AllowModule(
        ModuleGraph.RequiredModuleInfo moduleInfo, 
        HashSet<string> localNames,
        SemanticModel semanticModel)
    {
        var name = moduleInfo.Name;
        if (BuiltinModules.Contains(name))
        {
            return false;
        }

        if (localNames.Contains(name))
        {
            return false;
        }

        var documentId = moduleInfo.DocumentId;
        var retTy = semanticModel.GetExportType(documentId);
        return retTy is not null && !retTy.Equals(Builtin.Unknown) && !retTy.Equals(Builtin.Nil);
    }
}