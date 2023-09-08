using LuaLanguageServer.CodeAnalysis.Compile.Source;
using LuaLanguageServer.CodeAnalysis.Syntax.Tree;

namespace LuaLanguageServer.CodeAnalysis.Syntax.Location;

/// <summary>
/// A position in a source file.
/// </summary>
public abstract class LuaLocation
{
    public LuaLocationKind Kind { get; }

    public LuaSyntaxTree SourceTree { get; }

    public SourceRange Range { get; }

    internal LuaLocation(LuaLocationKind kind, LuaSyntaxTree tree, SourceRange range)
    {
        Kind = kind;
        SourceTree = tree;
        Range = range;
    }

    public override string ToString()
    {
        return $"{Kind} {Range}";
    }
}

public sealed class LuaSourceLocation : LuaLocation
{
    public LuaSourceFile? SourceFile
    {
        get
        {
            if (SourceTree.Source is LuaSourceFile sourceFile)
            {
                return sourceFile;
            }
            else
            {
                return null;
            }
        }
    }

    public int BaseLine { get;} = 0;

    public string FilePath => SourceFile?.FilePath ?? string.Empty;

    public LuaSourceLocation(LuaSyntaxTree tree, SourceRange range, int baseLine = 0) : base(LuaLocationKind.SourceFile, tree, range)
    {
        BaseLine = baseLine;
    }

    public override string ToString()
    {
        var sourceFile = SourceTree.Source;
        var startLine = sourceFile.GetLine(Range.StartOffset) + BaseLine;
        var startCol = sourceFile.GetCol(Range.StartOffset);

        var endLine = sourceFile.GetLine(Range.EndOffset) + BaseLine;
        var endCol = sourceFile.GetCol(Range.EndOffset);
        return $"{Kind} {FilePath} [{startLine}:{startCol} - {endLine}:{endCol}]";
    }
}

// 扩展方法Range to Location
public static class SourceRangeExtensions
{
    public static LuaLocation ToLocation(this SourceRange range, LuaSyntaxTree tree, int baseLine = 0)
    {
        return new LuaSourceLocation(tree, range, baseLine);
    }
}
