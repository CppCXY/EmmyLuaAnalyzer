using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Declaration;

public class DeclarationScope(DeclarationTree tree, int pos, DeclarationScope? parent)
    : DeclarationNodeContainer(pos, parent)
{
    public DeclarationTree Tree { get; } = tree;

    public new DeclarationScope? Parent { get; } = parent;

    public virtual bool WalkOver(Func<Declaration, bool> process)
    {
        return true;
    }

    public virtual void WalkUp(int position, int level, Func<Declaration, bool> process)
    {
        var cur = FindLastChild(it => it.Position < position);
        while (cur != null)
        {
            switch (cur)
            {
                case Declaration declaration when !process(declaration):
                case DeclarationScope scope when !scope.WalkOver(process):
                    return;
                default:
                    cur = cur.Prev;
                    break;
            }
        }

        Parent?.WalkUp(position, level + 1, process);
    }

    public Declaration? FindNameExpr(LuaNameExprSyntax nameExpr)
    {
        if (nameExpr.Name is { } name)
        {
            var nameText = name.RepresentText;
            Declaration? result = null;
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

    public IEnumerable<Declaration> DescendantDeclarations
    {
        get
        {
            var stack = new Stack<DeclarationNode>();
            stack.Push(this);
            while (stack.Count > 0)
            {
                var node = stack.Pop();
                // ReSharper disable once InvertIf
                if (node is Declaration declaration)
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
}

public class LocalStatDeclarationScope(DeclarationTree tree, int pos, DeclarationScope? parent)
    : DeclarationScope(tree, pos, parent)
{
    public override bool WalkOver(Func<Declaration, bool> process)
    {
        return ProcessNode(process);
    }

    public override void WalkUp(int position, int level, Func<Declaration, bool> process)
    {
        Parent?.WalkUp(Position, level, process);
    }
}

public class RepeatStatDeclarationScope(DeclarationTree tree, int pos, DeclarationScope? parent)
    : DeclarationScope(tree, pos, parent)
{
    public override void WalkUp(int position, int level, Func<Declaration, bool> process)
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

public class ForRangeStatDeclarationScope(DeclarationTree tree, int pos, DeclarationScope? parent)
    : DeclarationScope(tree, pos, parent)
{
    public override void WalkUp(int position, int level, Func<Declaration, bool> process)
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

public class MethodStatDeclarationScope(DeclarationTree tree, int pos, DeclarationScope? parent)
    : DeclarationScope(tree, pos, parent)
{
    public override void WalkUp(int position, int level, Func<Declaration, bool> process)
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
