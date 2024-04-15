using EmmyLua.CodeAnalysis.Diagnostics;
using EmmyLua.CodeAnalysis.Document;
using LanguageServer.CodeAction.CodeActions;
using LanguageServer.ExecuteCommand.Commands;
using LanguageServer.Server;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Diagnostic = OmniSharp.Extensions.LanguageServer.Protocol.Models.Diagnostic;
using DiagnosticCode = EmmyLua.CodeAnalysis.Diagnostics.DiagnosticCode;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;


namespace LanguageServer.CodeAction;

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
        var currentDocumentId = context.LuaWorkspace.GetDocumentIdByUri(currentUri);
        if (!currentDocumentId.HasValue)
        {
            return result;
        }

        foreach (var diagnostic in diagnostics)
        {
            if (diagnostic is { Source: "EmmyLua", Code.String: { } codeString })
            {
                var code = DiagnosticCodeHelper.GetCode(codeString);
                if (CodeActionMap.TryGetValue(code, out var codeAction)
                    && diagnostic.Data?.ToObject<string>() is { } data)
                {
                    result.AddRange(codeAction.GetCodeActions(data, currentDocumentId.Value, context)
                        .Select(CommandOrCodeAction.From));
                }

                AddDisableActions(result, codeString, currentDocumentId.Value, diagnostic.Range);
            }
        }

        return result;
    }

    private void AddDisableActions(List<CommandOrCodeAction> result, string codeString, LuaDocumentId documentId,
        Range range)
    {
        if (codeString == "syntax-error")
        {
            return;
        }

        result.Add(CommandOrCodeAction.From(
            DiagnosticAction.MakeCommand(
                $"Disable current line diagnostic ({codeString})",
                codeString,
                "disable-next-line",
                documentId,
                range
            )
        ));
        
        result.Add(CommandOrCodeAction.From(
            DiagnosticAction.MakeCommand(
                $"Disable current file diagnostic ({codeString})",
                codeString,
                "disable",
                documentId,
                range
            )
        ));
        
        // result.Add(CommandOrCodeAction.From(
        //     DiagnosticAction.MakeCommand(
        //         $"Disable workspace diagnostic ({codeString})",
        //         codeString,
        //         "disable-workspace",
        //         documentId,
        //         range
        //     )
        // ));
    }
}