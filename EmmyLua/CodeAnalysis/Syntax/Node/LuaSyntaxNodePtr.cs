using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Kind;
using EmmyLua.CodeAnalysis.Workspace;

namespace EmmyLua.CodeAnalysis.Syntax.Node;

public readonly struct LuaSyntaxNodePtr<TNode>(LuaDocumentId documentId, SourceRange range, LuaSyntaxKind kind)
    where TNode : LuaSyntaxNode
{
    public static LuaSyntaxNodePtr<TNode> Empty { get; } =
        new(LuaDocumentId.VirtualDocumentId, new SourceRange(), LuaSyntaxKind.None);

    public LuaDocumentId DocumentId { get; } = documentId;

    public SourceRange Range { get; } = range;

    public LuaSyntaxKind Kind { get; } = kind;

    public LuaSyntaxNodePtr(TNode syntaxNode) : this(syntaxNode.Tree.Document.Id, syntaxNode.Range, syntaxNode.Kind)
    {
    }

    public TNode? ToNode(LuaSyntaxNode root)
    {
        return root.FindNode(Range, Kind) as TNode;
    }

    public TNode? ToNode(LuaWorkspace workspace)
    {
        var document = workspace.GetDocument(DocumentId);
        if (document is null)
        {
            return null;
        }

        return ToNode(document.SyntaxTree.SyntaxRoot);
    }

    public TNode? ToNode(SearchContext context)
    {
        return ToNode(context.Compilation.Workspace);
    }

    public LuaSyntaxNodePtr<TBaseNode> Cast<TBaseNode>()
        where TBaseNode : LuaSyntaxNode
    {
        return new LuaSyntaxNodePtr<TBaseNode>(DocumentId, Range, Kind);
    }

    public LuaSyntaxNodePtr<LuaSyntaxNode> UpCast()
    {
        return new LuaSyntaxNodePtr<LuaSyntaxNode>(DocumentId, Range, Kind);
    }

    // same as unique id
    public string Stringify => $"{DocumentId.Id}_{Range.StartOffset}_{Range.Length}_{(int)Kind}";
}
