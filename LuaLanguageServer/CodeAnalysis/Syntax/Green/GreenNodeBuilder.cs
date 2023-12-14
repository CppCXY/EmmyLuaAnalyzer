using LuaLanguageServer.CodeAnalysis.Compile.Source;
using LuaLanguageServer.CodeAnalysis.Kind;

namespace LuaLanguageServer.CodeAnalysis.Syntax.Green;

public class GreenNodeBuilder
{
    struct ParentInfo(int position, LuaSyntaxKind kind)
    {
        public int FirstChild { get; } = position;
        public LuaSyntaxKind Kind { get; } = kind;
    }

    private Stack<ParentInfo> Parents { get; } = new();

    private List<GreenNode> Children { get; } = new();

    public void StartNode(LuaSyntaxKind kind)
    {
        var position = Children.Count;
        Parents.Push(new ParentInfo(position, kind));
    }

    private static bool IsTrivia(GreenNode greenNode)
    {
        if (greenNode.IsSyntaxNode)
        {
            return greenNode.SyntaxKind is LuaSyntaxKind.Comment;
        }
        else
        {
            return greenNode.TokenKind is LuaTokenKind.TkWhitespace or LuaTokenKind.TkEndOfLine;
        }
    }

    public void FinishNode()
    {
        var parentInfo = Parents.Pop();
        var nodeChildren = new List<GreenNode>();
        var nodeRange = new SourceRange();
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
            if (i == parentInfo.FirstChild)
            {
                nodeRange.StartOffset = Children[i].Range.StartOffset;
                nodeRange.Length = Children[i].Range.Length;
            }
            else
            {
                nodeRange.Length += Children[i].Range.Length;
            }

            Children[i].ChildPosition = i - parentInfo.FirstChild;
            nodeChildren.Add(Children[i]);
        }


        Children.RemoveRange(childStart, childEnd - childStart + 1);
        var green = new GreenNode(parentInfo.Kind, nodeRange, nodeChildren);
        if (childEnd + 1 < childCount)
        {
            Children.Insert(childStart, green);
        }
        else
        {
            Children.Add(green);
        }
    }

    public void EatToken(LuaTokenKind kind, SourceRange range)
    {
        Children.Add(new GreenNode(kind, range));
    }

    public GreenNode Finish()
    {
        return Children[0];
    }
}
