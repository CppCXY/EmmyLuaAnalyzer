using EmmyLuaAnalyzer.CodeAnalysis.Compilation.Type;
using EmmyLuaAnalyzer.CodeAnalysis.Syntax.Node;

namespace EmmyLuaAnalyzer.CodeAnalysis.Compilation.Analyzer.Declaration;

public class DeclarationNode(int position, DeclarationNodeContainer? parent)
{
    public DeclarationNode? Prev { get; set; }

    public DeclarationNode? Next { get; set; }

    public DeclarationNodeContainer? Parent { get; set; } = parent;

    public int Position { get; } = position;
}

public abstract class DeclarationNodeContainer(int position, DeclarationNodeContainer? parent)
    : DeclarationNode(position, parent)
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
    Method = 0x0002,
    ClassMember = 0x0004,
    Global = 0x0008,
    TypeDeclaration = 0x0010,
    Parameter = 0x0020,
    DocField = 0x0040,
    EnumMember = 0x0080,
    Virtual = 0x0100,
    GenericParameter = 0x0200,
}

public class Declaration(
    string name,
    int position,
    LuaSyntaxElement syntaxElement,
    DeclarationFlag flag,
    DeclarationNodeContainer? parent,
    Declaration? prev,
    ILuaType? type)
    : DeclarationNode(position, parent)
{
    public string Name { get; } = name;

    public LuaSyntaxElement SyntaxElement { get; } = syntaxElement;

    public DeclarationFlag Flags { get; set; } = flag;

    public ILuaType? Type { get; set; } = type;

    public Declaration? PrevDeclaration { get; set; } = prev;

    public bool IsLocal => (Flags & DeclarationFlag.Local) != 0;

    public bool IsMethod => (Flags & DeclarationFlag.Method) != 0;

    public bool IsClassMember => (Flags & DeclarationFlag.ClassMember) != 0;

    public bool IsGlobal => (Flags & DeclarationFlag.Global) != 0;

    public bool IsParam => (Flags & DeclarationFlag.Parameter) != 0;

    public Declaration FirstDeclaration => PrevDeclaration?.FirstDeclaration ?? this;

    public Declaration WithType(ILuaType? type)
    {
        return new Declaration(Name, Position, SyntaxElement, Flags, Parent, PrevDeclaration, type);
    }

    public override string ToString()
    {
        return $"{Flags} {Name}";
    }
}

public class VirtualDeclaration(ILuaType? type)
    : Declaration("", 0, null, DeclarationFlag.Virtual, null, null, type)
{
}
