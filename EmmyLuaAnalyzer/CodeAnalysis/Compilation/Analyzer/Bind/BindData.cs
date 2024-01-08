using EmmyLuaAnalyzer.CodeAnalysis.Compilation.Analyzer.Declaration;
using EmmyLuaAnalyzer.CodeAnalysis.Workspace;

namespace EmmyLuaAnalyzer.CodeAnalysis.Compilation.Analyzer.Bind;

public class BindData(DocumentId documentId, DeclarationTree tree)
{
    public BindAnalyzeStep Step { get; set; } = BindAnalyzeStep.Start;

    public DocumentId DocumentId { get; } = documentId;

    public DeclarationTree Tree { get; } = tree;
}
