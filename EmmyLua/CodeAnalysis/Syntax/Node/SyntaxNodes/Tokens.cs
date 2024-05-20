using System.Globalization;
using EmmyLua.CodeAnalysis.Document.Version;
using EmmyLua.CodeAnalysis.Kind;
using EmmyLua.CodeAnalysis.Syntax.Green;
using EmmyLua.CodeAnalysis.Syntax.Tree;

namespace EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

public class LuaStringToken(
    string value,
    GreenNode greenNode,
    LuaSyntaxTree tree,
    LuaSyntaxElement? parent,
    int startOffset)
    : LuaSyntaxToken(greenNode, tree, parent, startOffset)
{
    public string Value { get; } = value;

    public override string ToString()
    {
        return Value;
    }
}

public class LuaNumberToken(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaSyntaxToken(greenNode, tree, parent, startOffset)
{
    public bool IsInteger => Kind == LuaTokenKind.TkInt;

    public bool IsComplex => Kind == LuaTokenKind.TkComplex;

    public bool IsFloat => Kind == LuaTokenKind.TkFloat;

    public override string ToString()
    {
        return Text.ToString();
    }
}

public class LuaIntegerToken(
    long value,
    string suffix,
    GreenNode greenNode,
    LuaSyntaxTree tree,
    LuaSyntaxElement? parent,
    int startOffset)
    : LuaNumberToken(greenNode, tree, parent, startOffset)
{
    public long Value { get; } = value;

    public string Suffix { get; } = suffix;

    public override string ToString()
    {
        return $"{Value}{Suffix}";
    }
}

public class LuaFloatToken(
    double value,
    GreenNode greenNode,
    LuaSyntaxTree tree,
    LuaSyntaxElement? parent,
    int startOffset)
    : LuaNumberToken(greenNode, tree, parent, startOffset)
{
    public double Value { get; } = value;

    public override string ToString()
    {
        return Value.ToString(CultureInfo.InvariantCulture);
    }
}

public class LuaComplexToken(
    string value,
    GreenNode greenNode,
    LuaSyntaxTree tree,
    LuaSyntaxElement? parent,
    int startOffset)
    : LuaNumberToken(greenNode, tree, parent, startOffset)
{
    public string Value { get; } = value;

    public override string ToString()
    {
        return $"{Value}i";
    }
}

public class LuaNilToken(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaSyntaxToken(greenNode, tree, parent, startOffset);

public class LuaBoolToken(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaSyntaxToken(greenNode, tree, parent, startOffset)
{
    public bool Value => Text == "true";
}

public class LuaDotsToken(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaSyntaxToken(greenNode, tree, parent, startOffset);

public class LuaNameToken(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaSyntaxToken(greenNode, tree, parent, startOffset);

public class LuaWhitespaceToken(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent, int startOffset)
    : LuaSyntaxToken(greenNode, tree, parent, startOffset);

public class LuaVersionNumberToken(
    VersionNumber version,
    GreenNode greenNode,
    LuaSyntaxTree tree,
    LuaSyntaxElement? parent,
    int startOffset)
    : LuaSyntaxToken(greenNode, tree, parent, startOffset)
{
    public VersionNumber Version { get; } = version;
}

public class LuaTemplateTypeToken(
    string name,
    GreenNode greenNode,
    LuaSyntaxTree tree,
    LuaSyntaxElement? parent,
    int startOffset)
    : LuaSyntaxToken(greenNode, tree, parent, startOffset)
{
    public string Name { get; } = name;
}
