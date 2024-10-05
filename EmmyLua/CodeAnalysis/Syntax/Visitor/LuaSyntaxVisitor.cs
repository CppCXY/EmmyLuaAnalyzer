using EmmyLua.CodeAnalysis.Syntax.Node;

namespace EmmyLua.CodeAnalysis.Syntax.Visitor;

public abstract class LuaSyntaxVisitor
{
    private bool _continueChild = true;

    public void Visit(LuaSyntaxElement node)
    {
        var stack = new Stack<LuaSyntaxElement>();
        stack.Push(node);

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            VisitElement(current);

            if (_continueChild && current is LuaSyntaxNode syntaxNode)
            {
                foreach (var child in syntaxNode.ChildrenWithTokens.Reverse())
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

    protected abstract void VisitElement(LuaSyntaxElement element);
}
