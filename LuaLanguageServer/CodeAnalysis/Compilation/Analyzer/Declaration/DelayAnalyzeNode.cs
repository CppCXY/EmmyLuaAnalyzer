using LuaLanguageServer.CodeAnalysis.Syntax.Node;
using LuaLanguageServer.CodeAnalysis.Workspace;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Declaration;

public class DelayAnalyzeNode(LuaSyntaxNode node, DocumentId documentId)
{
    public LuaSyntaxNode Node => node;

    public DocumentId DocumentId => documentId;
}
