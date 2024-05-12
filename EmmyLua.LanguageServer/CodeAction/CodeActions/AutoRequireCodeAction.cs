using EmmyLua.CodeAnalysis.Document;
using EmmyLua.LanguageServer.ExecuteCommand.Commands;
using EmmyLua.LanguageServer.Server;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using DiagnosticCode = EmmyLua.CodeAnalysis.Diagnostics.DiagnosticCode;

namespace EmmyLua.LanguageServer.CodeAction.CodeActions;

public class AutoRequireCodeAction(DiagnosticCode code) : CodeActionBase(code)
{
    public override IEnumerable<OmniSharp.Extensions.LanguageServer.Protocol.Models.CodeAction> GetCodeActions(
        string data, LuaDocumentId currentDocumentId, ServerContext context)
    {
        if (data.Length == 0)
        {
            yield break;
        }
        
        var documentIds = data.Split(',').Select(it => new LuaDocumentId(int.Parse(it)));
        foreach (var documentId in documentIds)
        {
            var moduleInfo = context.LuaWorkspace.ModuleGraph.GetModuleInfo(documentId);
            if (moduleInfo is not null)
            {
                yield return new OmniSharp.Extensions.LanguageServer.Protocol.Models.CodeAction
                {
                    Title = $"import '{moduleInfo.ModulePath}'",
                    Command = AutoRequire.MakeCommand(string.Empty, currentDocumentId, moduleInfo.DocumentId, 0),
                    Kind = CodeActionKind.QuickFix,
                    IsPreferred = true
                };
            }
        }
    }
}