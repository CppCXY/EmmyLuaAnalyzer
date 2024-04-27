using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Workspace;

namespace EmmyLua.CodeAnalysis.Syntax.Node;

public readonly struct LuaElementPtr<TNode>(long uniqueId)
    where TNode : LuaSyntaxElement
{
    public static LuaElementPtr<TNode> Empty { get; } = new(0);

    public long UniqueId { get; } = uniqueId;

    public LuaDocumentId DocumentId => new((int)(UniqueId >> 32));

    public int ElementId => (int)UniqueId;

    public static LuaElementPtr<TNode> From(string idString)
    {
        return new LuaElementPtr<TNode>(long.Parse(idString));
    }

    public LuaElementPtr(TNode syntaxNode) : this(syntaxNode.UniqueId)
    {
    }

    public TNode? ToNode(LuaDocument document)
    {
        return document.SyntaxTree.GetElement(ElementId) as TNode;
    }

    public TNode? ToNode(LuaWorkspace workspace)
    {
        var document = workspace.GetDocument(DocumentId);
        if (document is null)
        {
            return null;
        }

        return ToNode(document);
    }

    public TNode? ToNode(SearchContext context)
    {
        return ToNode(context.Compilation.Workspace);
    }

    public LuaElementPtr<TBaseNode> Cast<TBaseNode>()
        where TBaseNode : LuaSyntaxNode
    {
        return new LuaElementPtr<TBaseNode>(UniqueId);
    }

    public LuaElementPtr<LuaSyntaxElement> UpCast()
    {
        return new LuaElementPtr<LuaSyntaxElement>(UniqueId);
    }

    // same as unique id
    public string Stringify => UniqueId.ToString();
}
