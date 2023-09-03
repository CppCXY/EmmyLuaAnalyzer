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

    public string FilePath => SourceFile?.FilePath ?? string.Empty;

    public LuaSourceLocation(LuaSyntaxTree tree, SourceRange range) : base(LuaLocationKind.SourceFile, tree, range)
    {
    }
}
