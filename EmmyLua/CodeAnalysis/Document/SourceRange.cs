namespace EmmyLua.CodeAnalysis.Document;

public struct SourceRange(int startOffset = 0, int length = 0)
{
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
}
