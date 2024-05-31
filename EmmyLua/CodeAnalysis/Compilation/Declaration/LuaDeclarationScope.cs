using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Declaration;

public class DeclarationNodeContainer(int position)
    : DeclarationNode(position)
{
    public List<DeclarationNode> Children { get; } = [];

    public void Add(DeclarationNode node)
    {
        node.Parent = this;

        // 如果Children为空，直接添加
        if (Children.Count == 0)
        {
            Children.Add(node);
            return;
        }

        // 如果Children的最后一个节点的位置小于等于node的位置，直接添加
        if (Children.Last().Position <= node.Position)
        {
            var last = Children.Last();
            node.Prev = last;
            last.Next = node;
            Children.Add(node);
        }
        else
        {
            var index = Children.FindIndex(n => n.Position > node.Position);
            // 否则，插入到找到的位置
            var nextNode = Children[index];
            var prevNode = nextNode.Prev;

            node.Next = nextNode;
            node.Prev = prevNode;

            if (prevNode != null)
            {
                prevNode.Next = node;
            }

            nextNode.Prev = node;

            Children.Insert(index, node);
        }
    }

    public DeclarationNode? FindFirstChild(Func<DeclarationNode, bool> predicate) => Children.FirstOrDefault(predicate);

    public DeclarationNode? FindLastChild(Func<DeclarationNode, bool> predicate) => Children.LastOrDefault(predicate);
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
        var cur = FindLastChild(it => it.Position < position);
        while (cur != null)
        {
            switch (cur)
            {
                case LuaDeclaration declaration when process(declaration) == ScopeFoundState.Founded:
                    return;
                case DeclarationScope scope when scope.WalkOver(process) == ScopeFoundState.Founded:
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
        var symbolNode = FindFirstChild(it => it.Position == position);
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
