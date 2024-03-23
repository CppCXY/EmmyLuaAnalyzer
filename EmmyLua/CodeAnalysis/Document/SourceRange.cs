namespace EmmyLua.CodeAnalysis.Document;

public struct SourceRange(int startOffset = 0, int length = 0): IEquatable<SourceRange>
{
    public static SourceRange Empty = new();

    public int StartOffset { get; set; } = startOffset;
    public int Length { get; set; } = length;

    public int EndOffset => StartOffset + Length;

    public override string ToString()
    {
        return $"[{StartOffset}, {EndOffset})";
    }

    public bool Contain(int offset)
    {
        return offset >= StartOffset && offset < EndOffset;
    }

    public SourceRange Merge(SourceRange range)
    {
        var start = Math.Min(StartOffset, range.StartOffset);
        var end = Math.Max(EndOffset, range.EndOffset);
        return new SourceRange(start, end - start);
    }

    public bool Equals(SourceRange other)
    {
        return StartOffset == other.StartOffset && Length == other.Length;
    }

    public override bool Equals(object? obj)
    {
        return obj is SourceRange other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(StartOffset, Length);
    }
}
