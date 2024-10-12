using EmmyLua.CodeAnalysis.Compilation;
using EmmyLua.CodeAnalysis.Compilation.Search;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Workspace;

namespace EmmyLua.CodeAnalysis.Syntax.Node;

public readonly record struct LuaPtr<TNode>(SyntaxElementId UniqueId)
    where TNode : LuaSyntaxElement
{
    public static readonly LuaPtr<TNode> Empty = new(SyntaxElementId.Empty);

    public SyntaxElementId UniqueId { get; } = UniqueId;

    public LuaDocumentId DocumentId => UniqueId.DocumentId;

    public int ElementId => UniqueId.ElementId;

    public static LuaPtr<TNode> From(string idString)
    {
        return new LuaPtr<TNode>(SyntaxElementId.From(idString));
    }

    public LuaPtr(TNode syntaxNode) : this(syntaxNode.UniqueId)
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

    public TNode? ToNode(LuaCompilation compilation)
    {
        return ToNode(compilation.Project);
    }

    public SyntaxIterator ToIter(LuaDocument document)
    {
        return new(ElementId, document.SyntaxTree);
    }

    public LuaPtr<TBaseNode> Cast<TBaseNode>()
        where TBaseNode : LuaSyntaxNode
    {
        return new LuaPtr<TBaseNode>(UniqueId);
    }

    public LuaPtr<LuaSyntaxElement> UpCast()
    {
        return new LuaPtr<LuaSyntaxElement>(UniqueId);
    }

    // same as unique id
    public string Stringify => UniqueId.ToString();
}
