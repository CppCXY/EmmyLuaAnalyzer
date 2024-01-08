using EmmyLuaAnalyzer.CodeAnalysis.Compile.Source;

namespace EmmyLuaAnalyzer.CodeAnalysis.Workspace;

public class LuaDocumentLocation: LuaLocation
{
    public int BaseLine { get;} = 0;

    public new LuaDocument Source => (LuaDocument)base.Source;

    public string FilePath => Source.Id.Path;

    public LuaDocumentLocation(LuaDocument document, SourceRange range, int baseLine = 0) : base(document, range)
    {
        BaseLine = baseLine;
    }

    public override string ToString()
    {
        var document = Source;
        var startLine = document.GetLine(Range.StartOffset) + BaseLine;
        var startCol = document.GetCol(Range.StartOffset);

        var endLine = document.GetLine(Range.EndOffset) + BaseLine;
        var endCol = document.GetCol(Range.EndOffset);
        return $"{FilePath} [{startLine}:{startCol} - {endLine}:{endCol}]";
    }
}
