using LuaLanguageServer.CodeAnalysis.Syntax.Node;
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

    public bool ProcessNode<T>(Func<T, bool> process)
    {
        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (var child in Children.OfType<T>())
        {
            if (!process(child))
            {
                return false;
            }
        }

        return true;
    }

    public DeclarationNode? FirstChild => Children.FirstOrDefault();

    public DeclarationNode? LastChild => Children.LastOrDefault();

    public DeclarationNode? FindFirstChild(Func<DeclarationNode, bool> predicate) => Children.FirstOrDefault(predicate);

    public DeclarationNode? FindLastChild(Func<DeclarationNode, bool> predicate) => Children.LastOrDefault(predicate);
}

[Flags]
public enum DeclarationFlag : ushort
{
    Local = 0x0001,
    Function = 0x0002,
    ClassMember = 0x0004,
    Global = 0x0008,
}

public class Declaration : DeclarationNode
{
    public string Name { get; }

    public LuaSyntaxElement SyntaxElement { get; }

    public DeclarationFlag Flags { get; set; }

    private Dictionary<string, Declaration> _fields = new();

    public Declaration? PrevDeclaration { get; set; }

    public bool IsLocal => (Flags & DeclarationFlag.Local) != 0;

    public bool IsFunction => (Flags & DeclarationFlag.Function) != 0;

    public bool IsClassMember => (Flags & DeclarationFlag.ClassMember) != 0;

    public bool IsGlobal => (Flags & DeclarationFlag.Global) != 0;

    public Declaration(
        string name, int position, LuaSyntaxElement syntaxElement, DeclarationFlag flag,
        DeclarationNodeContainer? parent, Declaration? prev)
        : base(position, parent)
    {
        Name = name;
        SyntaxElement = syntaxElement;
        Flags = flag;
        PrevDeclaration = prev;
    }

    public Declaration? FindField(string name)
    {
        return _fields.TryGetValue(name, out var child) ? child : null;
    }

    public void AddField(Declaration child)
    {
        _fields.Add(child.Name, child);
    }

    public Declaration FirstDeclaration => PrevDeclaration?.FirstDeclaration ?? this;
}
