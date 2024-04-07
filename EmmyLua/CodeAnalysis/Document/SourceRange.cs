namespace EmmyLua.CodeAnalysis.Document;

public readonly struct SourceRange(int startOffset = 0, int length = 0)
{
    public static SourceRange Empty = new();

    public int StartOffset { get; init; } = startOffset;
    public int Length { get; init; } = length;

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
