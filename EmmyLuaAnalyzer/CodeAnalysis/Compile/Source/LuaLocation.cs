namespace EmmyLuaAnalyzer.CodeAnalysis.Compile.Source;

/// <summary>
/// A position in a syntax tree.
/// </summary>
public class LuaLocation(LuaSource source, SourceRange range)
{
    public LuaSource Source { get; } = source;

    public SourceRange Range { get; } = range;

    public override string ToString()
    {
        return Range.ToString();
    }
}
