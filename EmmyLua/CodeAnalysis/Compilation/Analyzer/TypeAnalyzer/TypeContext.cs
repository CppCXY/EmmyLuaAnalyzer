using EmmyLua.CodeAnalysis.Compilation.Type.Types;
using EmmyLua.CodeAnalysis.Document;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.TypeAnalyzer;

public class TypeContext(LuaCompilation compilation)
{
    private RangeCollection IgnoreRanges { get; } = new();

    public void AddIgnoreRange(SourceRange range)
    {
        IgnoreRanges.AddRange(range);
    }

    public bool IsIgnoreRange(SourceRange range)
    {
        return IgnoreRanges.Contains(range.StartOffset);
    }

    public LuaType? FindType(string name)
    {
        return null;
    }
}
