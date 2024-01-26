using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Symbol;

public enum ScopeFoundState
{
    Founded,
    NotFounded,
}

public class SymbolScope(SymbolTree tree, int pos, SymbolScope? parent)
    : SymbolNodeContainer(pos, parent)
{
    public SymbolTree Tree { get; } = tree;

    public new SymbolScope? Parent { get; } = parent;

    public virtual ScopeFoundState WalkOver(Func<Symbol, ScopeFoundState> process)
    {
        return ScopeFoundState.NotFounded;
    }

    public virtual void WalkUp(int position, int level, Func<Symbol, ScopeFoundState> process)
    {
        var cur = FindLastChild(it => it.Position < position);
        while (cur != null)
        {
            switch (cur)
            {
                case Symbol symbol when process(symbol) == ScopeFoundState.Founded:
                    return;
                case SymbolScope scope when scope.WalkOver(process) == ScopeFoundState.Founded:
                    return;
                default:
                    cur = cur.Prev;
                    break;
            }
        }

        Parent?.WalkUp(position, level + 1, process);
    }

    public ScopeFoundState ProcessNode<T>(Func<T, ScopeFoundState> process)
    {
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var child in Children.OfType<T>())
        {
            if (process(child) == ScopeFoundState.Founded)
            {
                return ScopeFoundState.Founded;
            }
        }

        return ScopeFoundState.NotFounded;
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
                    return ScopeFoundState.Founded;
                }

                return ScopeFoundState.NotFounded;
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

    public IEnumerable<Symbol> Descendants
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

    public IEnumerable<SymbolScope> Ancestors
    {
        get
        {
            var cur = Parent;
            while (cur != null)
            {
                yield return cur;
                cur = cur.Parent;
            }
        }
    }
}

public class LocalStatSymbolScope(SymbolTree tree, int pos, SymbolScope? parent)
    : SymbolScope(tree, pos, parent)
{
    public override ScopeFoundState WalkOver(Func<Symbol, ScopeFoundState> process)
    {
        return ProcessNode(process);
    }

    public override void WalkUp(int position, int level, Func<Symbol, ScopeFoundState> process)
    {
        Parent?.WalkUp(Position, level, process);
    }
}

public class RepeatStatSymbolScope(SymbolTree tree, int pos, SymbolScope? parent)
    : SymbolScope(tree, pos, parent)
{
    public override void WalkUp(int position, int level, Func<Symbol, ScopeFoundState> process)
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
    public override void WalkUp(int position, int level, Func<Symbol, ScopeFoundState> process)
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

public class MethodStatSymbolScope(SymbolTree tree, int pos, SymbolScope? parent, Symbol? self)
    : SymbolScope(tree, pos, parent)
{
    public override ScopeFoundState WalkOver(Func<Symbol, ScopeFoundState> process)
    {
        return ProcessNode(process);
    }

    public override void WalkUp(int position, int level, Func<Symbol, ScopeFoundState> process)
    {
        if (level == 0)
        {
            Parent?.WalkUp(position, level, process);
        }
        else
        {
            if (self is not null && process(self) == ScopeFoundState.Founded)
            {
                return;
            }
            base.WalkUp(position, level, process);
        }
    }
}
