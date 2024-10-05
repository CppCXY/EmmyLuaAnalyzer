using EmmyLua.CodeAnalysis.Compilation.Type.Types;

namespace EmmyLua.CodeAnalysis.Compilation.Type.Visitor;

public abstract class LuaTypeVisitor
{
    private bool _continueChild = true;

    public void Visit(LuaType type)
    {
        var stack = new Stack<LuaType>();
        stack.Push(type);

        while (stack.Count > 0)
        {
            var current = stack.Pop();
            VisitType(current);

            if (_continueChild && current is LuaComplexType complexType)
            {
                foreach (var child in complexType.ChildrenTypes)
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

    protected abstract void VisitType(LuaType type);
}
