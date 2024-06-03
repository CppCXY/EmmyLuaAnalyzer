namespace EmmyLua.CodeAnalysis.Document;

public readonly record struct SourceRange(int StartOffset = 0, int Length = 0)
{
    public static SourceRange Empty = new();

    public int EndOffset => StartOffset + Length;

    public override string ToString()
    {
        return $"[{StartOffset}, {EndOffset})";
    }

    public bool Contain(int offset)
    {
        return offset >= StartOffset && offset < EndOffset;
    }

    public bool Contain(SourceRange range)
    {
        return range.StartOffset >= StartOffset && range.EndOffset <= EndOffset;
    }

    public bool Intersect(SourceRange range)
    {
        return StartOffset < range.EndOffset && range.StartOffset < EndOffset;
    }

    public SourceRange Merge(SourceRange range)
    {
        var start = Math.Min(StartOffset, range.StartOffset);
        var end = Math.Max(EndOffset, range.EndOffset);
        return new SourceRange(start, end - start);
    }
}
