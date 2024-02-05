using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Syntax.Node;

namespace EmmyLua.CodeAnalysis.Compilation.Symbol;

public class SymbolNode(int position)
{
    public SymbolNode? Prev { get; set; }

    public SymbolNode? Next { get; set; }

    public SymbolNodeContainer? Parent { get; set; } = null;

    public int Position { get; } = position;
}

public abstract class SymbolNodeContainer(int position)
    : SymbolNode(position)
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

    public SymbolNode? FirstChild => Children.FirstOrDefault();

    public SymbolNode? LastChild => Children.LastOrDefault();

    public SymbolNode? FindFirstChild(Func<SymbolNode, bool> predicate) => Children.FirstOrDefault(predicate);

    public SymbolNode? FindLastChild(Func<SymbolNode, bool> predicate) => Children.LastOrDefault(predicate);
}

public enum SymbolFeature
{
    None,
    Local,
    Global,
}

public class Symbol(
    string name,
    int position,
    LuaSyntaxElement? syntaxElement,
    SymbolKind kind,
    Symbol? prev,
    ILuaType? declarationType,
    SymbolFeature feature = SymbolFeature.None
)
    : SymbolNode(position)
{
    public string Name { get; } = name;

    public LuaSyntaxElement? SyntaxElement { get; } = syntaxElement;

    public SymbolKind Kind { get; } = kind;

    private ILuaType? _declarationType = declarationType;

    public ILuaType? DeclarationType
    {
        get => _declarationType ?? PrevSymbol?.FirstSymbol._declarationType;
        set => _declarationType = value;
    }

    public SymbolFeature Feature { get; internal set; } = feature;

    public Symbol? PrevSymbol { get; set; } = prev;

    public Symbol FirstSymbol => PrevSymbol?.FirstSymbol ?? this;

    public override string ToString()
    {
        return $"{Kind} {Name}";
    }
}
