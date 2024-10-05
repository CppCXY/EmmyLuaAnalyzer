using EmmyLua.CodeAnalysis.Syntax.Node;

namespace EmmyLua.CodeAnalysis.Syntax.Visitor;

public abstract class LuaSyntaxNodeVisitor
{
    private bool _continueChild = true;

    public void Visit(LuaSyntaxNode node)
    {
        var stack = new Stack<LuaSyntaxNode>();
        stack.Push(node);

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            VisitNode(current);

            if (_continueChild)
            {
                foreach (var child in current.ChildrenNode.Reverse())
                {
                    stack.Push(child);
                }
            }
            _continueChild = true;
        }
    }

    public void SkipChildren()
    {
        _continueChild = false;
    }

    protected abstract void VisitNode(LuaSyntaxNode node);
}
