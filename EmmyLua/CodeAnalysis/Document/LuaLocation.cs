namespace EmmyLua.CodeAnalysis.Document;

/// <summary>
/// A position in a syntax tree.
/// </summary>
public class LuaLocation(LuaDocument document, SourceRange range, int baseLine = 0)
{
    public LuaDocument Document { get; } = document;

    public SourceRange Range { get; } = range;

    public int BaseLine { get;} = baseLine;

    public string FilePath => Document.Id.Path;

    public override string ToString()
    {
        var document = Document;
        var startLine = document.GetLine(Range.StartOffset) + BaseLine;
        var startCol = document.GetCol(Range.StartOffset);

        var endLine = document.GetLine(Range.EndOffset) + BaseLine;
        var endCol = document.GetCol(Range.EndOffset);
        return $"{FilePath} [{startLine}:{startCol} - {endLine}:{endCol}]";
    }
}
