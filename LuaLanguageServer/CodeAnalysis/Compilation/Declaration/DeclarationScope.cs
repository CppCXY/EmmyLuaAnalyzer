using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Declaration;

public class DeclarationScope : DeclarationNodeContainer
{
    public DeclarationTree Tree { get; }

    public new DeclarationScope? Parent { get; }

    public DeclarationScope(DeclarationTree tree, int position, DeclarationScope? parent)
        : base(position, parent)
    {
        Tree = tree;
        Parent = parent;
    }

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

    private Declaration? Find(LuaNameExprSyntax nameExpr)
    {
        var name = nameExpr.Name.RepresentText;
        Declaration? result = null;
        WalkUp(Tree.GetPosition(nameExpr), 0, declaration =>
        {
            if (declaration.Name != name) return true;
            result = declaration;
            return false;
        });
        return result;
    }

    public Declaration? Find(LuaExprSyntax? expr)
    {
        switch (expr)
        {
            case LuaNameExprSyntax nameSyntax:
                return Find(nameSyntax);
            case LuaIndexExprSyntax indexExprSyntax:
            {
                // var name = indexExprSyntax.Name?.RepresentText;
                // if (name == null)
                // {
                //     return null;
                // }
                //
                // var declaration = Find(indexExprSyntax.PrefixExpr);
                // return declaration?.FindField(name);
                return null;
            }
            default:
                return null;
        }
    }
}

public class LocalStatDeclarationScope : DeclarationScope
{
    public LocalStatDeclarationScope(DeclarationTree tree, int position, DeclarationScope? parent)
        : base(tree, position, parent)
    {
    }

    public override bool WalkOver(Func<Declaration, bool> process)
    {
        return ProcessNode(process);
    }

    public override void WalkUp(int position, int level, Func<Declaration, bool> process)
    {
        Parent?.WalkUp(Position, level, process);
    }
}

public class RepeatStatDeclarationScope : DeclarationScope
{
    public RepeatStatDeclarationScope(DeclarationTree tree, int position, DeclarationScope? parent)
        : base(tree, position, parent)
    {
    }

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

public class ForRangeStatDeclarationScope : DeclarationScope
{
    public ForRangeStatDeclarationScope(DeclarationTree tree, int position, DeclarationScope? parent)
        : base(tree, position, parent)
    {
    }

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
