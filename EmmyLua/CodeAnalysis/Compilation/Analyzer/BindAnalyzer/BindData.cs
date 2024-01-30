using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Document;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.BindAnalyzer;

public class BindData(DocumentId documentId, SymbolTree tree)
{
    public BindAnalyzeStep Step { get; set; } = BindAnalyzeStep.Start;

    public DocumentId DocumentId { get; } = documentId;

    public SymbolTree Tree { get; } = tree;
}
