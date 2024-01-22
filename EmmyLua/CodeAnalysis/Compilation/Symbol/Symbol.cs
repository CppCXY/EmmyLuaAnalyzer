using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Syntax.Node;

namespace EmmyLua.CodeAnalysis.Compilation.Symbol;

public class SymbolNode(int position, SymbolNodeContainer? parent)
{
    public SymbolNode? Prev { get; set; }

    public SymbolNode? Next { get; set; }

    public SymbolNodeContainer? Parent { get; set; } = parent;

    public int Position { get; } = position;
}

public abstract class SymbolNodeContainer(int position, SymbolNodeContainer? parent)
    : SymbolNode(position, parent)
{
    public List<SymbolNode> Children { get; } = [];

    public void Add(SymbolNode node)
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

    public SymbolNode? FirstChild => Children.FirstOrDefault();

    public SymbolNode? LastChild => Children.LastOrDefault();

    public SymbolNode? FindFirstChild(Func<SymbolNode, bool> predicate) => Children.FirstOrDefault(predicate);

    public SymbolNode? FindLastChild(Func<SymbolNode, bool> predicate) => Children.LastOrDefault(predicate);
}

public class Symbol(
    string name,
    int position,
    LuaSyntaxElement? syntaxElement,
    SymbolFlag flag,
    SymbolNodeContainer? parent,
    Symbol? prev,
    ILuaType? type
)
    : SymbolNode(position, parent)
{
    public string Name { get; } = name;

    public LuaSyntaxElement? SyntaxElement { get; } = syntaxElement;

    public SymbolFlag Flags { get; } = flag;

    public ILuaType? Type
    {
        get => type ?? PrevSymbol?.FirstSymbol.Type;
        set => type = value;
    }

    public Symbol? PrevSymbol { get; set; } = prev;

    public bool IsLocal => (Flags & SymbolFlag.Local) != 0;

    public bool IsMethod => (Flags & SymbolFlag.Method) != 0;

    public bool IsClassMember => (Flags & SymbolFlag.ClassMember) != 0;

    public bool IsGlobal => (Flags & SymbolFlag.Global) != 0;

    public bool IsParam => (Flags & SymbolFlag.Parameter) != 0;

    public Symbol FirstSymbol => PrevSymbol?.FirstSymbol ?? this;

    public Symbol WithType(ILuaType? luaType)
    {
        return new Symbol(Name, Position, SyntaxElement, Flags, Parent, PrevSymbol, luaType);
    }

    public override string ToString()
    {
        return $"{Flags} {Name}";
    }
}

public class VirtualSymbol(string name, ILuaType? type)
    : Symbol(name, 0, null, SymbolFlag.Virtual, null, null, type)
{
    public VirtualSymbol(ILuaType? type)
        : this("", type)
    {
    }
}
