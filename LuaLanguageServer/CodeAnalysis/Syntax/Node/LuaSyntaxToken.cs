using LuaLanguageServer.CodeAnalysis.Kind;
using LuaLanguageServer.CodeAnalysis.Syntax.Green;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;
using LuaLanguageServer.CodeAnalysis.Syntax.Tree;

namespace LuaLanguageServer.CodeAnalysis.Syntax.Node;

public class LuaSyntaxToken : LuaSyntaxElement
{
    public LuaSyntaxToken(GreenNode greenNode, LuaSyntaxTree tree, LuaSyntaxElement? parent)
        : base(greenNode, tree, parent)
    {
    }

    public LuaTokenKind Kind => Green.TokenKind;

    public ReadOnlySpan<char> Text => Tree.Source.Text.AsSpan(Green.Range.StartOffset, Green.Range.Length);

    public string RepresentText
    {
        get
        {
            switch (Kind)
            {
                // remove \' or \"
                case LuaTokenKind.TkString:
                {
                    var text = Text;
                    return text.Length > 2 ? text[1..^1].ToString() : text.ToString();
                }
                // skip [====[
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
                {
                    return Text.ToString();
                }
            }
        }
    }

}
