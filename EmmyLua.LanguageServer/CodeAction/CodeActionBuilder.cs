using EmmyLua.CodeAnalysis.Diagnostics;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.LanguageServer.CodeAction.CodeActions;
using EmmyLua.LanguageServer.ExecuteCommand.Commands;
using EmmyLua.LanguageServer.Framework.Protocol.Message.CodeAction;
using EmmyLua.LanguageServer.Framework.Protocol.Model;
using EmmyLua.LanguageServer.Server;
using Diagnostic = EmmyLua.LanguageServer.Framework.Protocol.Model.Diagnostic.Diagnostic;
using DiagnosticCode = EmmyLua.CodeAnalysis.Diagnostics.DiagnosticCode;

namespace EmmyLua.LanguageServer.CodeAction;

public class CodeActionBuilder
{
    public List<CodeActionBase> CodeActions { get; } = new()
    {
        new AutoRequireCodeAction(DiagnosticCode.NeedImport)
    };

    public Dictionary<DiagnosticCode, CodeActionBase> CodeActionMap { get; } = new();

    public CodeActionBuilder()
    {
        foreach (var codeAction in CodeActions)
        {
            CodeActionMap[codeAction.Code] = codeAction;
        }
    }

    public List<CommandOrCodeAction> Build(IEnumerable<Diagnostic> diagnostics, string currentUri,
        ServerContext context)
    {
        var result = new List<CommandOrCodeAction>();
        var currentDocumentId = context.LuaProject.GetDocumentIdByUri(currentUri);
        if (!currentDocumentId.HasValue)
        {
            return result;
        }

        foreach (var diagnostic in diagnostics)
        {
            if (diagnostic is { Source: "EmmyLua", Code.StringValue: { } codeString })
            {
                var code = DiagnosticCodeHelper.GetCode(codeString);
                if (CodeActionMap.TryGetValue(code, out var codeAction)
                    && diagnostic.Data?.Value is string data)
                {
                    result.AddRange(codeAction.GetCodeActions(data, currentDocumentId.Value, context)
                        .Select(it => new CommandOrCodeAction(it)));
                }

                if (code != DiagnosticCode.None)
                {
                    AddDisableActions(result, codeString, currentDocumentId.Value, diagnostic.Range);
                }
            }
        }

        return result;
    }

    private void AddDisableActions(List<CommandOrCodeAction> result, string codeString, LuaDocumentId documentId,
        DocumentRange range)
    {
        if (codeString == "syntax-error")
        {
            return;
        }

        result.Add(new CommandOrCodeAction(
            DiagnosticAction.MakeCommand(
                $"Disable current line diagnostic ({codeString})",
                codeString,
                "disable-next-line",
                documentId,
                range
            )
        ));

        result.Add(new CommandOrCodeAction(
            DiagnosticAction.MakeCommand(
                $"Disable current file diagnostic ({codeString})",
                codeString,
                "disable",
                documentId,
                range
            )
        ));

        result.Add(new CommandOrCodeAction(
            SetConfig.MakeCommand(
                $"Disable workspace diagnostic ({codeString})",
                SetConfigAction.Add,
                "Diagnostics.Disable",
                codeString
            )
        ));
    }
}