﻿using EmmyLua.CodeAnalysis.Compilation.Semantic;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Workspace.Module;
using EmmyLua.CodeAnalysis.Workspace.Module.FilenameConverter;
using EmmyLua.LanguageServer.ExecuteCommand.Commands;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Completion;

namespace EmmyLua.LanguageServer.Completion.CompleteProvider;

public class ModuleProvider : ICompleteProviderBase
{
    private HashSet<string> BuiltinModules { get; } =
    [
        "io", "os", "string", "table", "math", "debug", "coroutine", "package", "utf8"
    ];

    public void AddCompletion(CompleteContext context)
    {
        if (!context.CompletionConfig.AutoRequire)
        {
            return;
        }

        if (context.TriggerToken?.Parent is not LuaNameExprSyntax nameExpr)
        {
            return;
        }

        var semanticModel = context.SemanticModel;
        var modules = semanticModel.Compilation.Project.ModuleManager.GetAllModules();
        var localNames = semanticModel.GetDeclarationsBefore(context.TriggerToken).Select(it => it.Name).ToHashSet();
        foreach (var module in modules)
        {
            if (AllowModule(module, localNames, context.SemanticModel))
            {
                var documentId = module.DocumentId;
                var retTy = semanticModel.GetExportType(documentId) ?? Builtin.Unknown;
                var insetText = FilenameConverter.ConvertToIdentifier(module.Name,
                    context.CompletionConfig.AutoRequireFilenameConvention);
                context.Add(new CompletionItem
                {
                    Label = insetText,
                    Kind = CompletionItemKind.Module,
                    LabelDetails = new CompletionItemLabelDetails()
                    {
                        Detail = $" (in {module.ModulePath})",
                        Description = context.RenderBuilder.RenderType(retTy, context.RenderFeature)
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
        ModuleIndex moduleInfo,
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
        return retTy is not null && !retTy.IsSameType(Builtin.Unknown, semanticModel.Context) &&
               !retTy.IsSameType(Builtin.Nil, semanticModel.Context);
    }
}