using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Kind;

namespace EmmyLua.CodeAnalysis.Syntax.Tree.Green;

public class GreenNodeBuilder
{
    readonly struct ParentInfo(int position, LuaSyntaxKind kind)
    {
        public int FirstChild { get; } = position;
        public LuaSyntaxKind Kind { get; } = kind;
    }

    private int ElementCount { get; set; } = 0;

    private Stack<ParentInfo> Parents { get; } = new();

    private List<GreenNode> Children { get; } = [];

    public void StartNode(LuaSyntaxKind kind)
    {
        var position = Children.Count;
        Parents.Push(new ParentInfo(position, kind));
    }

    private static bool IsTrivia(GreenNode greenNode)
    {
        if (greenNode.IsNode)
        {
            return greenNode.SyntaxKind is LuaSyntaxKind.Comment;
        }

        return greenNode.TokenKind is LuaTokenKind.TkWhitespace or LuaTokenKind.TkEndOfLine;
    }

    public void FinishNode()
    {
        if (Parents.Count == 0)
        {
            return;
        }

        var parentInfo = Parents.Pop();
        var nodeChildren = new List<GreenNode>();
        var length = 0;
        var childStart = parentInfo.FirstChild;
        var childEnd = Children.Count - 1;
        var childCount = Children.Count;
        // skip trivia
        if (parentInfo.Kind is not (LuaSyntaxKind.Block or LuaSyntaxKind.Source))
        {
            for (; childStart < Children.Count; childStart++)
            {
                if (!IsTrivia(Children[childStart]))
                {
                    break;
                }
            }

            for (; childEnd >= childStart; childEnd--)
            {
                if (!IsTrivia(Children[childEnd]))
                {
                    break;
                }
            }
        }

        for (var i = childStart; i <= childEnd; i++)
        {
            if (i == childStart)
            {
                length = Children[i].Length;
            }
            else
            {
                length += Children[i].Length;
            }

            nodeChildren.Add(Children[i]);
        }

        Children.RemoveRange(childStart, childEnd - childStart + 1);
        var green = CreateGreenNode(parentInfo.Kind, length, nodeChildren);
        if (childEnd + 1 < childCount)
        {
            Children.Insert(childStart, green);
        }
        else
        {
            Children.Add(green);
        }
    }

    private GreenNode CreateGreenNode(LuaSyntaxKind kind, int length, List<GreenNode> children)
    {
        ElementCount++;
        return new GreenNode(kind, length, children);
    }

    private GreenNode CreateGreenToken(LuaTokenKind kind, SourceRange range)
    {
        ElementCount++;
        return new GreenNode(kind, range.Length);
    }

    public void EatToken(LuaTokenKind kind, SourceRange range)
    {
        Children.Add(CreateGreenToken(kind, range));
    }

    public (GreenNode, int) Finish()
    {
        return (Children[0], ElementCount);
    }
}
