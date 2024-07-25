using EmmyLua.CodeAnalysis.Compilation.Search;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Workspace;

namespace EmmyLua.CodeAnalysis.Syntax.Node;

public readonly record struct LuaElementPtr<TNode>(SyntaxElementId UniqueId)
    where TNode : LuaSyntaxElement
{
    public static readonly LuaElementPtr<TNode> Empty = new(SyntaxElementId.Empty);

    public SyntaxElementId UniqueId { get; } = UniqueId;

    public LuaDocumentId DocumentId => UniqueId.DocumentId;

    public int ElementId => UniqueId.ElementId;

    public static LuaElementPtr<TNode> From(string idString)
    {
        return new LuaElementPtr<TNode>(SyntaxElementId.From(idString));
    }

    public LuaElementPtr(TNode syntaxNode) : this(syntaxNode.UniqueId)
    {
    }

    public TNode? ToNode(LuaDocument document)
    {
        return document.SyntaxTree.GetElement(ElementId) as TNode;
    }

    public TNode? ToNode(LuaProject project)
    {
        var document = project.GetDocument(DocumentId);
        if (document is null)
        {
            return null;
        }

        return ToNode(document);
    }

    public TNode? ToNode(SearchContext context)
    {
        return ToNode(context.Compilation.Project);
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
