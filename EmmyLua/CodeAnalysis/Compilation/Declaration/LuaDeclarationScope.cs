using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Declaration;

public class DeclarationNodeContainer(int position)
    : DeclarationNode(position)
{
    public DeclarationNodeContainer? Parent { get; set; }

    public List<DeclarationNode> Children { get; } = [];

    public void Add(DeclarationNode node)
    {
        if (node is DeclarationNodeContainer container)
        {
            container.Parent = this;
        }

        // 如果Children为空，直接添加
        if (Children.Count == 0)
        {
            Children.Add(node);
            return;
        }

        // 如果Children的最后一个节点的位置小于等于node的位置，直接添加
        if (Children.Last().Position <= node.Position)
        {
            Children.Add(node);
        }
        else
        {
            var index = Children.FindIndex(n => n.Position > node.Position);
            // 否则，插入到找到的位置
            Children.Insert(index, node);
        }
    }
}

public enum ScopeFoundState
{
    Founded,
    NotFounded,
}

public class DeclarationScope(int pos)
    : DeclarationNodeContainer(pos)
{
    public DeclarationScope? ParentScope => Parent as DeclarationScope;

    public virtual ScopeFoundState WalkOver(Func<LuaDeclaration, ScopeFoundState> process)
    {
        return ScopeFoundState.NotFounded;
    }

    public virtual void WalkUp(int position, int level, Func<LuaDeclaration, ScopeFoundState> process)
    {
        var curIndex = Children.FindLastIndex(it => it.Position < position);
        for(var i = curIndex; i >= 0; i--)
        {
            if (Children[i] is LuaDeclaration declaration && process(declaration) == ScopeFoundState.Founded)
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

    public LuaDeclaration? FindNameDeclaration(LuaNameExprSyntax nameExpr)
    {
        if (nameExpr.Name is { } name)
        {
            var nameText = name.RepresentText;
            LuaDeclaration? result = null;
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

    public LuaDeclaration? FindDeclaration(LuaSyntaxElement element)
    {
        var position = element.Position;
        var symbolNode = Children.FirstOrDefault(it => it.Position == position);
        if (symbolNode is LuaDeclaration result)
        {
            return result;
        }

        return null;
    }

    public IEnumerable<LuaDeclaration> Descendants
    {
        get
        {
            var stack = new Stack<DeclarationNode>();
            stack.Push(this);
            while (stack.Count > 0)
            {
                var node = stack.Pop();
                // ReSharper disable once InvertIf
                if (node is LuaDeclaration declaration)
                {
                    yield return declaration;
                }
                else if (node is DeclarationNodeContainer n)
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

public class LocalStatDeclarationScope(int pos)
    : DeclarationScope(pos)
{
    public override ScopeFoundState WalkOver(Func<LuaDeclaration, ScopeFoundState> process)
    {
        return ProcessNode(process);
    }

    public override void WalkUp(int position, int level, Func<LuaDeclaration, ScopeFoundState> process)
    {
        ParentScope?.WalkUp(Position, level, process);
    }
}

public class RepeatStatDeclarationScope(int pos)
    : DeclarationScope(pos)
{
    public override void WalkUp(int position, int level, Func<LuaDeclaration, ScopeFoundState> process)
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

public class ForRangeStatDeclarationScope(int pos)
    : DeclarationScope(pos)
{
    public override void WalkUp(int position, int level, Func<LuaDeclaration, ScopeFoundState> process)
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
