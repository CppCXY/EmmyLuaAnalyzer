using EmmyLua.CodeAnalysis.Diagnostics;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.LanguageServer.Server;

namespace EmmyLua.LanguageServer.CodeAction.CodeActions;

public abstract class CodeActionBase(DiagnosticCode code)
{
    public DiagnosticCode Code { get; } = code;

    public abstract IEnumerable<OmniSharp.Extensions.LanguageServer.Protocol.Models.CodeAction> GetCodeActions(
        string data, LuaDocumentId currentDocumentId, ServerContext context);
}