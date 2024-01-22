using EmmyLua.CodeAnalysis.Compilation.Analyzer.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Workspace;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.Bind;

public class BindData(DocumentId documentId, SymbolTree tree)
{
    public BindAnalyzeStep Step { get; set; } = BindAnalyzeStep.Start;

    public DocumentId DocumentId { get; } = documentId;

    public SymbolTree Tree { get; } = tree;
}
