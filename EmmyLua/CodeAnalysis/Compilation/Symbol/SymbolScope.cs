using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Symbol;

public enum ScopeFoundState
{
    Founded,
    NotFounded,
}

public class SymbolScope(SymbolTree tree, int pos)
    : SymbolNodeContainer(pos)
{
    public SymbolTree Tree { get; } = tree;

    public SymbolScope? ParentScope => Parent as SymbolScope;

    public virtual ScopeFoundState WalkOver(Func<Declaration, ScopeFoundState> process)
    {
        return ScopeFoundState.NotFounded;
    }

    public virtual void WalkUp(int position, int level, Func<Declaration, ScopeFoundState> process)
    {
        var cur = FindLastChild(it => it.Position < position);
        while (cur != null)
        {
            switch (cur)
            {
                case Declaration declaration when process(declaration) == ScopeFoundState.Founded:
                    return;
                case SymbolScope scope when scope.WalkOver(process) == ScopeFoundState.Founded:
                    return;
                default:
                    cur = cur.Prev;
                    break;
            }
        }

        ParentScope?.WalkUp(position, level + 1, process);
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

    public Declaration? FindNameDeclaration(LuaNameExprSyntax nameExpr)
    {
        if (nameExpr.Name is { } name)
        {
            var nameText = name.RepresentText;
            Declaration? result = null;
            WalkUp(nameExpr.Position, 0, declaration =>
            {
                if (declaration is { Feature: SymbolFeature.Global or SymbolFeature.Local}
                    && string.Equals(declaration.Name, nameText, StringComparison.CurrentCulture))
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

    public Symbol? FindSymbol(LuaSyntaxElement element)
    {
        var position = element.Position;
        var symbolNode = FindFirstChild(it => it.Position == position);
        if (symbolNode is Symbol result)
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
            var cur = ParentScope;
            while (cur != null)
            {
                yield return cur;
                cur = cur.ParentScope;
            }
        }
    }
}

public class LocalStatSymbolScope(SymbolTree tree, int pos)
    : SymbolScope(tree, pos)
{
    public override ScopeFoundState WalkOver(Func<Declaration, ScopeFoundState> process)
    {
        return ProcessNode(process);
    }

    public override void WalkUp(int position, int level, Func<Declaration, ScopeFoundState> process)
    {
        ParentScope?.WalkUp(Position, level, process);
    }
}

public class RepeatStatSymbolScope(SymbolTree tree, int pos)
    : SymbolScope(tree, pos)
{
    public override void WalkUp(int position, int level, Func<Declaration, ScopeFoundState> process)
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

public class ForRangeStatSymbolScope(SymbolTree tree, int pos)
    : SymbolScope(tree, pos)
{
    public override void WalkUp(int position, int level, Func<Declaration, ScopeFoundState> process)
    {
        if (level == 0)
        {
            ParentScope?.WalkUp(position, level, process);
        }
        else
        {
            base.WalkUp(position, level, process);
        }
    }
}

public class MethodStatSymbolScope(SymbolTree tree, int pos, ParameterDeclaration? self)
    : SymbolScope(tree, pos)
{
    public override ScopeFoundState WalkOver(Func<Declaration, ScopeFoundState> process)
    {
        return ProcessNode(process);
    }

    public override void WalkUp(int position, int level, Func<Declaration, ScopeFoundState> process)
    {
        if (level == 0)
        {
            ParentScope?.WalkUp(position, level, process);
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
