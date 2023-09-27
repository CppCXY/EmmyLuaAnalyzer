using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Declaration;

public class DeclarationNode
{
    public DeclarationNode? Prev => Parent?.Children.ElementAtOrDefault(Position - 1);

    public DeclarationNode? Next => Parent?.Children.ElementAtOrDefault(Position + 1);

    public DeclarationNodeContainer? Parent { get; set; }

    public int Position { get; }

    public DeclarationNode(int position, DeclarationNodeContainer? parent)
    {
        Position = position;
        Parent = parent;
    }
}

public abstract class DeclarationNodeContainer : DeclarationNode
{
    public List<DeclarationNode> Children { get; } = new();

    public DeclarationNodeContainer(int position, DeclarationNodeContainer? parent)
        : base(position, parent)
    {
    }

    public void Add(DeclarationNode node)
    {
        node.Parent = this;
        Children.Add(node);
    }

    public DeclarationNode? FirstChild => Children.FirstOrDefault();

    public DeclarationNode? LastChild => Children.LastOrDefault();

    public DeclarationNode? FindFirstChild(Func<DeclarationNode, bool> predicate) => Children.FirstOrDefault(predicate);

    public DeclarationNode? FindLastChild(Func<DeclarationNode, bool> predicate) => Children.LastOrDefault(predicate);
}

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

    public void WalkUp(int position, int level, Func<Declaration, bool> process)
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

    private Declaration? Find(LuaNameSyntax nameSyntax)
    {
        var name = nameSyntax.Name.RepresentText;
        Declaration? result = null;
        WalkUp(Tree.GetPosition(nameSyntax), 0, declaration =>
        {
            if (declaration.Name == name)
            {
                result = declaration;
                return false;
            }

            return true;
        });
        return result;
    }

    public Declaration? Find(LuaExprSyntax? expr)
    {
        switch (expr)
        {
            case LuaNameSyntax nameSyntax:
                return Find(nameSyntax);
            case LuaIndexExprSyntax indexExprSyntax:
            {
                var name = indexExprSyntax.Name?.RepresentText;
                if (name == null)
                {
                    return null;
                }

                var declaration = Find(indexExprSyntax.ParentExpr);
                return declaration?.FindField(name);
            }
            default:
                return null;
        }
    }
}
