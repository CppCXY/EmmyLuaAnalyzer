using EmmyLua.CodeAnalysis.Compilation.Type.Types;
using EmmyLua.CodeAnalysis.Document;

namespace EmmyLua.CodeAnalysis.Compilation.Type.Compile;

public class TypeContext(LuaCompilation compilation, LuaDocument document)
{
    public LuaCompilation Compilation { get; } = compilation;

    public LuaTypeManager TypeManager => Compilation.TypeManager;

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
