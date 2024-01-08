using EmmyLua.CodeAnalysis.Compilation.Analyzer.Declaration;
using EmmyLua.CodeAnalysis.Workspace;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.Bind;

public class BindData(DocumentId documentId, DeclarationTree tree)
{
    public BindAnalyzeStep Step { get; set; } = BindAnalyzeStep.Start;

    public DocumentId DocumentId { get; } = documentId;

    public DeclarationTree Tree { get; } = tree;
}
