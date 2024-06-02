using EmmyLua.CodeAnalysis.Common;

namespace EmmyLua.CodeAnalysis.Document;

/// <summary>
/// A position in a syntax tree.
/// </summary>
public class LuaLocation(LuaDocument luaDocument, SourceRange range, int baseLine = 0) : ILocation
{
    public LuaDocument LuaDocument { get; } = luaDocument;

    public SourceRange Range { get; } = range;

    private int BaseLine { get; } = baseLine;

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
