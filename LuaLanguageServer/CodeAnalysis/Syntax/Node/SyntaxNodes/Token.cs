using LuaLanguageServer.CodeAnalysis.Kind;
using LuaLanguageServer.CodeAnalysis.Syntax.Green;
using LuaLanguageServer.CodeAnalysis.Syntax.Tree;

namespace LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

public class LuaStringToken : LuaSyntaxToken
{
    public LuaStringToken(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent) : base(greenNode, tree, parent)
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
    public LuaNumberToken(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent) : base(greenNode, tree, parent)
    {
    }

    public bool IsInteger => FirstChildToken(LuaTokenKind.TkInt) != null;

    public bool IsComplex => FirstChildToken(LuaTokenKind.TkComplex) != null;

    public bool IsFloat => FirstChildToken(LuaTokenKind.TkNumber) != null;

    public long IntegerValue => long.Parse(Text);

    public double FloatValue => double.Parse(Text);
}

public class LuaNilToken : LuaSyntaxToken
{
    public LuaNilToken(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent) : base(greenNode, tree, parent)
    {
    }

}

public class LuaBoolToken : LuaSyntaxToken
{
    public LuaBoolToken(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent) : base(greenNode, tree, parent)
    {
    }

    public bool Value => Text == "true";
}

public class LuaDotsToken : LuaSyntaxToken
{
    public LuaDotsToken(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent) : base(greenNode, tree, parent)
    {
    }
}

public class LuaNameToken : LuaSyntaxToken
{
    public LuaNameToken(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent) : base(greenNode, tree, parent)
    {
    }
}
