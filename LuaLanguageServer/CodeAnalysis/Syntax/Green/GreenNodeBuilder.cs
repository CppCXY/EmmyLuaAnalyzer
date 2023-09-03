using LuaLanguageServer.LuaCore.Compile.Source;
using LuaLanguageServer.LuaCore.Kind;

namespace LuaLanguageServer.LuaCore.Syntax.Green;

public class GreenNodeBuilder
{
    struct ParentInfo
    {
        public int FirstChild { get; }
        public LuaSyntaxKind Kind { get; }

        public ParentInfo(int position, LuaSyntaxKind kind)
        {
            FirstChild = position;
            Kind = kind;
        }
    }


    private Stack<ParentInfo> Parents { get; } = new Stack<ParentInfo>();

    private List<GreenNode> Children { get; } = new List<GreenNode>();

    public void StartNode(LuaSyntaxKind kind)
    {
        var position = Children.Count;
        Parents.Push(new ParentInfo(position, kind));
    }

    public void FinishNode()
    {
        var parentInfo = Parents.Pop();
        var nodeChildren = new List<GreenNode>();
        var nodeRange = new SourceRange();
        for (var i = parentInfo.FirstChild; i < Children.Count; i++)
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

            Children[i].Slot = i - parentInfo.FirstChild;
            nodeChildren.Add(Children[i]);
        }

        Children.RemoveRange(parentInfo.FirstChild, Children.Count - parentInfo.FirstChild);

        Children.Add(new GreenNode(parentInfo.Kind, nodeRange, nodeChildren));
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
