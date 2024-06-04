using EmmyLua.CodeAnalysis.Common;

namespace EmmyLua.CodeAnalysis.Document;

/// <summary>
/// A position in a syntax tree.
/// </summary>
public record LuaLocation(LuaDocument LuaDocument, SourceRange Range, int BaseLine = 0) : ILocation
{
    public static LuaLocation Empty { get; } = new LuaLocation(LuaDocument.Empty, SourceRange.Empty);

    private string FilePath => LuaDocument.Path;

    public IDocument Document => LuaDocument;

    public int StartLine => LuaDocument.GetLine(Range.StartOffset) + BaseLine;

    public int StartCol => LuaDocument.GetCol(Range.StartOffset);

    public int EndLine => LuaDocument.GetLine(Range.EndOffset) + BaseLine;

    public int EndCol => LuaDocument.GetCol(Range.EndOffset);

    public override string ToString()
    {
        return $"{FilePath}({StartLine}, {StartCol}) - ({EndLine}, {EndCol})";
    }
}
