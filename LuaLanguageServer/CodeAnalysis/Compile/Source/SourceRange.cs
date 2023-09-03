namespace LuaLanguageServer.CodeAnalysis.Compile.Source;

public struct SourceRange
{
    public int StartOffset { get; set; }
    public int Length { get; set; }

    public int EndOffset => StartOffset + Length;

    public SourceRange(int startOffset = 0, int length = 0)
    {
        StartOffset = startOffset;
        Length = length;
    }
}
