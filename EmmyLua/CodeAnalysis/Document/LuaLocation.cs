namespace EmmyLua.CodeAnalysis.Document;

/// <summary>
/// A position in a syntax tree.
/// </summary>
public record LuaLocation(
    int StartLine,
    int StartCol,
    int EndLine,
    int EndCol,
    string Uri)
{
    public static LuaLocation Empty { get; } = new LuaLocation(0, 0, 0, 0, string.Empty);

    public string FilePath => new Uri(Uri).AbsolutePath;

    public string Uri { get; } = Uri;

    public int StartLine { get; } = StartLine;

    public int StartCol { get; } = StartCol;

    public int EndLine { get; } = EndLine;

    public int EndCol { get; } = EndCol;

    public override string ToString()
    {
        return $"{FilePath}({StartLine}, {StartCol}) - ({EndLine}, {EndCol})";
    }

    public string LspLocation => $"{Uri}#L{StartLine + 1}:{StartCol}";
}
