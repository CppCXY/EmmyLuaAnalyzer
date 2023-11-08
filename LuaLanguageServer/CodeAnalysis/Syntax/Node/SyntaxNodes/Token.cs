using System.Globalization;
using LuaLanguageServer.CodeAnalysis.Kind;
using LuaLanguageServer.CodeAnalysis.Syntax.Green;
using LuaLanguageServer.CodeAnalysis.Syntax.Tree;

namespace LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

public class LuaStringToken : LuaSyntaxToken
{
    public LuaStringToken(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent) : base(greenNode, tree,
        parent)
    {
    }

    public string InnerString
    {
        get
        {
            switch (Kind)
            {
                // skip [====[
                case LuaTokenKind.TkString:
                {
                    var text = Text;
                    return text.Length > 2 ? text[1..^1].ToString() : text.ToString();
                }
                case LuaTokenKind.TkLongString:
                {
                    var text = Text;
                    var prefixCount = 0;
                    foreach (var t in text)
                    {
                        if ((t == '[' && prefixCount == 0) || t == '=')
                        {
                            prefixCount++;
                        }
                        else if (t == '[')
                        {
                            prefixCount++;
                            break;
                        }
                        else
                        {
                            break;
                        }
                    }

                    return text.Length > (prefixCount * 2)
                        ? text[prefixCount..^prefixCount].ToString()
                        : text.ToString();
                }
                default:
                    return string.Empty;
            }
        }
    }
}

public class LuaNumberToken : LuaSyntaxToken
{
    public LuaNumberToken(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent) : base(greenNode, tree,
        parent)
    {
    }

    public bool IsInteger => Kind == LuaTokenKind.TkInt;

    public bool IsComplex => Kind == LuaTokenKind.TkComplex;

    public bool IsFloat => Kind == LuaTokenKind.TkFloat;

    public override string ToString()
    {
        return Text.ToString();
    }
}

public class LuaIntegerToken : LuaNumberToken
{
    public ulong Value { get; }

    public string Suffix { get; }

    public LuaIntegerToken(
        ulong value,
        string suffix,
        GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent) : base(greenNode, tree, parent)
    {
        Value = value;
        Suffix = suffix;
    }

    public override string ToString()
    {
        return $"{Value}{Suffix}";
    }
}

public class LuaFloatToken : LuaNumberToken
{
    public double Value { get; }

    public LuaFloatToken(
        double value,
        GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent) : base(greenNode, tree, parent)
    {
        Value = value;
    }

    public override string ToString()
    {
        return Value.ToString(CultureInfo.InvariantCulture);
    }
}

public class LuaComplexToken : LuaNumberToken
{
    public string Value { get; }

    public LuaComplexToken(
        string value,
        GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent) : base(greenNode, tree, parent)
    {
        Value = value;
    }

    public override string ToString()
    {
        return $"{Value}i";
    }
}

public class LuaNilToken : LuaSyntaxToken
{
    public LuaNilToken(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent) : base(greenNode, tree,
        parent)
    {
    }
}

public class LuaBoolToken : LuaSyntaxToken
{
    public LuaBoolToken(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent) : base(greenNode, tree,
        parent)
    {
    }

    public bool Value => Text == "true";
}

public class LuaDotsToken : LuaSyntaxToken
{
    public LuaDotsToken(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent) : base(greenNode, tree,
        parent)
    {
    }
}

public class LuaNameToken : LuaSyntaxToken
{
    public LuaNameToken(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent) : base(greenNode, tree,
        parent)
    {
    }
}
