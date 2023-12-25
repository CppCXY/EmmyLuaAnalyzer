using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Declaration;
using LuaLanguageServer.CodeAnalysis.Workspace;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Bind;

public class BindData(DocumentId documentId, DeclarationTree tree)
{
    public BindAnalyzeStep Step { get; set; } = BindAnalyzeStep.Start;

    public DocumentId DocumentId { get; } = documentId;

    public DeclarationTree Tree { get; } = tree;
}
