using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Scope;

public record DeclarationNodeBase(int Position);

public record DeclarationNode(int Position, LuaSymbol Symbol)
    : DeclarationNodeBase(Position);

public record DeclarationNodeBaseContainer(
    int Position,
    List<DeclarationNodeBase> Children,
    DeclarationNodeBaseContainer? Parent = null)
    : DeclarationNodeBase(Position)
{
    public void Add(DeclarationNodeBase nodeBase)
    {
        // 如果Children为空，直接添加
        if (Children.Count == 0)
        {
            Children.Add(nodeBase);
            return;
        }

        // 如果Children的最后一个节点的位置小于等于node的位置，直接添加
        if (Children.Last().Position <= nodeBase.Position)
        {
            Children.Add(nodeBase);
        }
        else
        {
            var index = Children.FindIndex(n => n.Position > nodeBase.Position);
            // 否则，插入到找到的位置
            Children.Insert(index, nodeBase);
        }
    }
}

public enum ScopeFoundState
{
    Founded,
    NotFounded,
}

public record DeclarationScope(
    int Position,
    List<DeclarationNodeBase> Children,
    DeclarationNodeBaseContainer? Parent = null)
    : DeclarationNodeBaseContainer(Position, Children, Parent)
{
    public DeclarationScope? ParentScope => Parent as DeclarationScope;

    public virtual ScopeFoundState WalkOver(Func<LuaSymbol, ScopeFoundState> process)
    {
        return ScopeFoundState.NotFounded;
    }

    public virtual void WalkUp(int position, int level, Func<LuaSymbol, ScopeFoundState> process)
    {
        var curIndex = Children.FindLastIndex(it => it.Position < position);
        for (var i = curIndex; i >= 0; i--)
        {
            if (Children[i] is DeclarationNode { Symbol: { } declaration } &&
                process(declaration) == ScopeFoundState.Founded)
            {
                return;
            }
            else if (Children[i] is DeclarationScope scope && scope.WalkOver(process) == ScopeFoundState.Founded)
            {
                return;
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

    public LuaSymbol? FindNameDeclaration(LuaNameExprSyntax nameExpr)
    {
        if (nameExpr.Name is { } name)
        {
            var nameText = name.RepresentText;
            LuaSymbol? result = null;
            var position = nameExpr.Position;
            if (nameExpr.Ancestors.OfType<LuaStatSyntax>().FirstOrDefault() is LuaLocalStatSyntax
                {
                    Position: { } newPosition
                })
            {
                position = newPosition;
            }

            WalkUp(position, 0, declaration =>
            {
                if ((declaration.IsLocal || declaration.IsGlobal)
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

    public LuaSymbol? FindDeclaration(LuaSyntaxElement element)
    {
        var position = element.Position;
        var symbolNode = Children.FirstOrDefault(it => it.Position == position);
        if (symbolNode is DeclarationNode { Symbol: { } result })
        {
            return result;
        }

        return null;
    }

    public IEnumerable<LuaSymbol> Descendants
    {
        get
        {
            var stack = new Stack<DeclarationNodeBase>();
            stack.Push(this);
            while (stack.Count > 0)
            {
                var node = stack.Pop();
                // ReSharper disable once InvertIf
                if (node is DeclarationNode { Symbol: { } declaration })
                {
                    yield return declaration;
                }
                else if (node is DeclarationNodeBaseContainer n)
                {
                    foreach (var child in n.Children.AsEnumerable().Reverse())
                    {
                        stack.Push(child);
                    }
                }
            }
        }
    }

    public IEnumerable<DeclarationScope> Ancestors
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

public record LocalStatDeclarationScope(
    int Position,
    List<DeclarationNodeBase> Children,
    DeclarationNodeBaseContainer? Parent = null)
    : DeclarationScope(Position, Children, Parent)
{
    public override ScopeFoundState WalkOver(Func<LuaSymbol, ScopeFoundState> process)
    {
        return ProcessNode(process);
    }

    public override void WalkUp(int position, int level, Func<LuaSymbol, ScopeFoundState> process)
    {
        ParentScope?.WalkUp(Position, level, process);
    }
}

public record RepeatStatDeclarationScope(
    int Position,
    List<DeclarationNodeBase> Children,
    DeclarationNodeBaseContainer? Parent = null)
    : DeclarationScope(Position, Children, Parent)
{
    public override void WalkUp(int position, int level, Func<LuaSymbol, ScopeFoundState> process)
    {
        if (Children.FirstOrDefault() is DeclarationScope scope && level == 0)
        {
            scope.WalkUp(position, level, process);
        }
        else
        {
            base.WalkUp(position, level, process);
        }
    }
}

public record ForRangeStatDeclarationScope(
    int Position,
    List<DeclarationNodeBase> Children,
    DeclarationNodeBaseContainer? Parent = null)
    : DeclarationScope(Position, Children, Parent)
{
    public override void WalkUp(int position, int level, Func<LuaSymbol, ScopeFoundState> process)
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
