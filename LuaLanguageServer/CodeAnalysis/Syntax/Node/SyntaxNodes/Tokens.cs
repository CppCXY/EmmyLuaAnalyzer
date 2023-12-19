using System.Globalization;
using LuaLanguageServer.CodeAnalysis.Kind;
using LuaLanguageServer.CodeAnalysis.Syntax.Green;
using LuaLanguageServer.CodeAnalysis.Syntax.Tree;

namespace LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

public class LuaStringToken(string value, GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    : LuaSyntaxToken(greenNode, tree, parent)
{
    public string Value { get; } = value;

    public override string ToString()
    {
        return Value;
    }
}

public class LuaNumberToken(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    : LuaSyntaxToken(greenNode, tree, parent)
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
    LuaSyntaxElement? parent)
    : LuaNumberToken(greenNode, tree, parent)
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
    LuaSyntaxElement? parent)
    : LuaNumberToken(greenNode, tree, parent)
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
    LuaSyntaxElement? parent)
    : LuaNumberToken(greenNode, tree, parent)
{
    public string Value { get; } = value;

    public override string ToString()
    {
        return $"{Value}i";
    }
}

public class LuaNilToken(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    : LuaSyntaxToken(greenNode, tree, parent);

public class LuaBoolToken(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    : LuaSyntaxToken(greenNode, tree, parent)
{
    public bool Value => Text == "true";
}

public class LuaDotsToken(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    : LuaSyntaxToken(greenNode, tree, parent);

public class LuaNameToken(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
    : LuaSyntaxToken(greenNode, tree, parent);
