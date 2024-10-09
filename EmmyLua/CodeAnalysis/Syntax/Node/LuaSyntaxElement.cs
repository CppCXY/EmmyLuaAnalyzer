using EmmyLua.CodeAnalysis.Diagnostics;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Tree;


namespace EmmyLua.CodeAnalysis.Syntax.Node;

public class LuaSyntaxElement(int index, LuaSyntaxTree tree)
{
    public int ElementId { get; } = index;

    public LuaSyntaxTree Tree { get; } = tree;

    public LuaDocumentId DocumentId => Tree.Document.Id;

    public SourceRange Range => Tree.GetSourceRange(ElementId);

    public SyntaxElementId UniqueId => new(DocumentId, ElementId);

    public bool IsValid => ElementId != -1;

    public string UniqueString => UniqueId.ToString();

    public int Position => Range.StartOffset;

    public LuaLocation Location => Tree.Document.GetLocation(Range);

    public void PushDiagnostic(DiagnosticSeverity severity, string message)
    {
        var diagnostic = new Diagnostic(severity, DiagnosticCode.SyntaxError, message, Range);
        Tree.PushDiagnostic(diagnostic);
    }

    public SyntaxIterator Iter => new(ElementId, Tree);
}
