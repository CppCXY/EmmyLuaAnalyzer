using EmmyLua.CodeAnalysis.Compilation.Analyzer.ResolveAnalyzer;
using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Index;
using EmmyLua.CodeAnalysis.Compilation.Reference;
using EmmyLua.CodeAnalysis.Compilation.Scope;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.DeclarationAnalyzer;

public class DeclarationContext(
    LuaDocument document,
    DeclarationAnalyzer analyzer,
    AnalyzeContext analyzeContext)
{
    private DeclarationScope? _topScope;

    private DeclarationScope? _curScope;

    private Stack<DeclarationScope> _scopeStack = new();

    private Dictionary<SyntaxElementId, DeclarationScope> _scopeOwners = new();

    private Dictionary<SyntaxElementId, List<LuaElementPtr<LuaDocTagSyntax>>> _attachedDocs = new();

    private HashSet<SyntaxElementId> _uniqueReferences = new();

    private Dictionary<SyntaxElementId, LuaDeclaration> _declarations = new();

    private Dictionary<SyntaxElementId, LuaElementPtr<LuaClosureExprSyntax>> _elementRelatedClosure = new();

    private DeclarationAnalyzer Analyzer { get; } = analyzer;

    private LuaCompilation Compilation => Analyzer.Compilation;

    public WorkspaceIndex Db => Compilation.Db;

    private AnalyzeContext AnalyzeContext { get; } = analyzeContext;

    public LuaDocument Document { get; } = document;

    public LuaDocumentId DocumentId => Document.Id;

    public LuaDeclarationTree? GetDeclarationTree()
    {
        if (_topScope is not null)
        {
            return new LuaDeclarationTree(_scopeOwners, _topScope);
        }

        return null;
    }

    public LuaDeclaration? FindLocalDeclaration(LuaNameExprSyntax nameExpr)
    {
        return FindScope(nameExpr)?.FindNameDeclaration(nameExpr);
    }

    public DeclarationScope? FindScope(LuaSyntaxNode element)
    {
        LuaSyntaxElement? cur = element;
        while (cur != null)
        {
            if (_scopeOwners.TryGetValue(cur.UniqueId, out var scope))
            {
                return scope;
            }

            cur = cur.Parent;
        }

        return null;
    }

    public void AddLocalDeclaration(LuaSyntaxElement element, LuaDeclaration luaDeclaration)
    {
        _curScope?.Add(new DeclarationNode(element.Position, luaDeclaration));
        AddAttachedDeclaration(element, luaDeclaration);
    }

    public void AddAttachedDeclaration(LuaSyntaxElement element, LuaDeclaration luaDeclaration)
    {
        _declarations.Add(element.UniqueId, luaDeclaration);
    }

    public void AddReference(ReferenceKind kind, LuaDeclaration declaration, LuaSyntaxElement nameElement)
    {
        if (!_uniqueReferences.Add(nameElement.UniqueId))
        {
            return;
        }

        var reference = new LuaReference(new(nameElement), kind);
        Db.AddReference(DocumentId, declaration, reference);
    }

    public void AddUnResolved(UnResolved declaration)
    {
        AnalyzeContext.UnResolves.Add(declaration);
    }

    public void PushScope(LuaSyntaxElement element)
    {
        if (_scopeOwners.TryGetValue(element.UniqueId, out var scope))
        {
            _scopeStack.Push(scope);
            _curScope = scope;
            return;
        }

        var position = element.Position;
        switch (element)
        {
            case LuaLocalStatSyntax:
            {
                SetScope(new LocalStatDeclarationScope(position, [], _curScope), element);
                break;
            }
            case LuaRepeatStatSyntax:
            {
                SetScope(new RepeatStatDeclarationScope(position, [], _curScope), element);
                break;
            }
            case LuaForRangeStatSyntax:
            {
                SetScope(new ForRangeStatDeclarationScope(position, [], _curScope), element);
                break;
            }
            default:
            {
                SetScope(new DeclarationScope(position, [], _curScope), element);
                break;
            }
        }
    }

    private void SetScope(DeclarationScope scope, LuaSyntaxElement element)
    {
        _scopeStack.Push(scope);
        _topScope ??= scope;
        _scopeOwners.Add(element.UniqueId, scope);
        _curScope?.Add(scope);
        _curScope = scope;
    }

    public void PopScope()
    {
        if (_scopeStack.Count != 0)
        {
            _scopeStack.Pop();
        }

        _curScope = _scopeStack.Count != 0 ? _scopeStack.Peek() : _topScope;
    }

    public void AttachDoc(LuaDocTagSyntax docTagSyntax)
    {
        var comment = docTagSyntax.Parent;
        if (comment is LuaCommentSyntax)
        {
            if (_attachedDocs.TryGetValue(comment.UniqueId, out var list))
            {
                list.Add(new LuaElementPtr<LuaDocTagSyntax>(docTagSyntax));
            }
            else
            {
                _attachedDocs.Add(comment.UniqueId, [new(docTagSyntax)]);
            }
        }
    }

    public IEnumerable<(LuaSyntaxElement, List<LuaDocTagSyntax>)> GetAttachedDocs()
    {
        foreach (var (commentId, docTags) in _attachedDocs)
        {
            var ptr = new LuaElementPtr<LuaCommentSyntax>(commentId);
            var comment = ptr.ToNode(Document);
            if (comment is { Owner: { } owner })
            {
                var docList = new List<LuaDocTagSyntax>();
                foreach (var elementPtr in docTags)
                {
                    var docTag = elementPtr.ToNode(Document);
                    if (docTag is not null)
                    {
                        docList.Add(docTag);
                    }
                }

                yield return (owner, docList);
            }
        }
    }

    public LuaDeclaration? GetAttachedDeclaration(LuaSyntaxElement element)
    {
        return _declarations.GetValueOrDefault(element.UniqueId);
    }

    public void SetElementRelatedClosure(LuaSyntaxElement element, LuaClosureExprSyntax closureExprSyntax)
    {
        _elementRelatedClosure.TryAdd(element.UniqueId, new(closureExprSyntax));
    }

    public LuaClosureExprSyntax? GetElementRelatedClosure(LuaSyntaxElement element)
    {
        return _elementRelatedClosure.GetValueOrDefault(element.UniqueId).ToNode(Document);
    }
}
