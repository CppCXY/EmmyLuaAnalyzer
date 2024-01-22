using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Symbol;

public class SymbolScope(SymbolTree tree, int pos, SymbolScope? parent)
    : SymbolNodeContainer(pos, parent)
{
    public SymbolTree Tree { get; } = tree;

    public new SymbolScope? Parent { get; } = parent;

    public virtual bool WalkOver(Func<Symbol, bool> process)
    {
        return true;
    }

    public virtual void WalkUp(int position, int level, Func<Symbol, bool> process)
    {
        var cur = FindLastChild(it => it.Position < position);
        while (cur != null)
        {
            switch (cur)
            {
                case Symbol declaration when !process(declaration):
                case SymbolScope scope when !scope.WalkOver(process):
                    return;
                default:
                    cur = cur.Prev;
                    break;
            }
        }

        Parent?.WalkUp(position, level + 1, process);
    }

    public Symbol? FindNameExpr(LuaNameExprSyntax nameExpr)
    {
        if (nameExpr.Name is { } name)
        {
            var nameText = name.RepresentText;
            Symbol? result = null;
            WalkUp(Tree.GetPosition(nameExpr), 0, declaration =>
            {
                if ((declaration.IsGlobal || declaration.IsLocal) &&
                    string.Equals(declaration.Name, nameText, StringComparison.CurrentCulture))
                {
                    result = declaration;
                    return false;
                }

                return true;
            });
            return result;
        }

        return null;
    }

    public Symbol? FindParamDef(LuaParamDefSyntax paramDef)
    {
        var position = Tree.GetPosition(paramDef);
        var declarationNode = FindFirstChild(it => it.Position == position);
        if (declarationNode is Symbol result)
        {
            return result;
        }

        return null;
    }

    public Symbol? FindLocalName(LuaLocalNameSyntax localName)
    {
        var position = Tree.GetPosition(localName);
        var declarationNode = FindFirstChild(it => it.Position == position);
        if (declarationNode is Symbol result)
        {
            return result;
        }

        return null;
    }

    public IEnumerable<Symbol> DescendantDeclarations
    {
        get
        {
            var stack = new Stack<SymbolNode>();
            stack.Push(this);
            while (stack.Count > 0)
            {
                var node = stack.Pop();
                // ReSharper disable once InvertIf
                if (node is Symbol declaration)
                {
                    yield return declaration;
                }
                else if (node is SymbolNodeContainer n)
                {
                    foreach (var child in n.Children.AsEnumerable().Reverse())
                    {
                        stack.Push(child);
                    }
                }
            }
        }
    }
}

public class LocalStatSymbolScope(SymbolTree tree, int pos, SymbolScope? parent)
    : SymbolScope(tree, pos, parent)
{
    public override bool WalkOver(Func<Symbol, bool> process)
    {
        return ProcessNode(process);
    }

    public override void WalkUp(int position, int level, Func<Symbol, bool> process)
    {
        Parent?.WalkUp(Position, level, process);
    }
}

public class RepeatStatSymbolScope(SymbolTree tree, int pos, SymbolScope? parent)
    : SymbolScope(tree, pos, parent)
{
    public override void WalkUp(int position, int level, Func<Symbol, bool> process)
    {
        if (Children.FirstOrDefault() is SymbolScope scope && level == 0)
        {
            scope.WalkUp(position, level, process);
        }
        else
        {
            base.WalkUp(position, level, process);
        }
    }
}

public class ForRangeStatSymbolScope(SymbolTree tree, int pos, SymbolScope? parent)
    : SymbolScope(tree, pos, parent)
{
    public override void WalkUp(int position, int level, Func<Symbol, bool> process)
    {
        if (level == 0)
        {
            Parent?.WalkUp(position, level, process);
        }
        else
        {
            base.WalkUp(position, level, process);
        }
    }
}

public class MethodStatSymbolScope(SymbolTree tree, int pos, SymbolScope? parent)
    : SymbolScope(tree, pos, parent)
{
    public override void WalkUp(int position, int level, Func<Symbol, bool> process)
    {
        if (level == 0)
        {
            Parent?.WalkUp(position, level, process);
        }
        else
        {
            base.WalkUp(position, level, process);
        }
    }
}
