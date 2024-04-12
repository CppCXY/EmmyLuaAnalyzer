using EmmyLua.CodeAnalysis.Diagnostics;
using LanguageServer.CodeAction.CodeActions;
using LanguageServer.Server;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Diagnostic = OmniSharp.Extensions.LanguageServer.Protocol.Models.Diagnostic;
using DiagnosticCode = EmmyLua.CodeAnalysis.Diagnostics.DiagnosticCode;

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
            }
        }

        return result;
    }
}