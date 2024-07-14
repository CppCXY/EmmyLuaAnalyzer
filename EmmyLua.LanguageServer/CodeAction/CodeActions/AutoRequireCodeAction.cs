using EmmyLua.CodeAnalysis.Document;
using EmmyLua.LanguageServer.ExecuteCommand.Commands;
using EmmyLua.LanguageServer.Framework.Protocol.Message.CodeAction;
using EmmyLua.LanguageServer.Server;
using DiagnosticCode = EmmyLua.CodeAnalysis.Diagnostics.DiagnosticCode;

namespace EmmyLua.LanguageServer.CodeAction.CodeActions;

public class AutoRequireCodeAction(DiagnosticCode code) : CodeActionBase(code)
{
    public override IEnumerable<Framework.Protocol.Message.CodeAction.CodeAction> GetCodeActions(
        string data, LuaDocumentId currentDocumentId, ServerContext context)
    {
        if (data.Length == 0)
        {
            yield break;
        }
        
        var documentIds = data.Split(',').Select(it => new LuaDocumentId(int.Parse(it)));
        foreach (var documentId in documentIds)
        {
            var moduleInfo = context.LuaWorkspace.ModuleManager.GetModuleInfo(documentId);
            if (moduleInfo is not null)
            {
                yield return new Framework.Protocol.Message.CodeAction.CodeAction
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