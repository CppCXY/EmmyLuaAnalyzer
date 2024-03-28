using EmmyLua.CodeAnalysis.Workspace;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace LanguageServer.Util;

public static class ToSelector
{
    public static TextDocumentSelector ToTextDocumentSelector(LuaWorkspace workspace)
    {
        var filters = workspace.Features.Extensions
            .Select(ext => new TextDocumentFilter() { Pattern = $"**/{ext}" })
            .ToList();

        return new TextDocumentSelector
        (
            filters
        );
    }
}