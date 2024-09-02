using EmmyLua.CodeAnalysis.Compilation.Analyzer.ResolveAnalyzer;
using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Index;
using EmmyLua.CodeAnalysis.Compilation.Reference;
using EmmyLua.CodeAnalysis.Compilation.Scope;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Diagnostics;
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

    private Dictionary<SyntaxElementId, LuaSymbol> _declarations = new();

    private Dictionary<SyntaxElementId, LuaElementPtr<LuaClosureExprSyntax>> _elementRelatedClosure = new();

    private DeclarationAnalyzer Analyzer { get; } = analyzer;

    private LuaCompilation Compilation => Analyzer.Compilation;

    public ProjectIndex ProjectIndex => Compilation.ProjectIndex;

    public GlobalIndex GlobalIndex => Compilation.GlobalIndex;

    public LuaTypeManager TypeManager => Compilation.TypeManager;

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

    public LuaSymbol? FindLocalDeclaration(LuaNameExprSyntax nameExpr)
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

    public void AddLocalDeclaration(LuaSyntaxElement element, LuaSymbol luaSymbol)
    {
        _curScope?.Add(new DeclarationNode(element.Position, luaSymbol));
        AddAttachedDeclaration(element, luaSymbol);
    }

    public void AddAttachedDeclaration(LuaSyntaxElement element, LuaSymbol luaSymbol)
    {
        _declarations.Add(element.UniqueId, luaSymbol);
    }

    public void AddReference(ReferenceKind kind, LuaSymbol symbol, LuaSyntaxElement nameElement)
    {
        if (!_uniqueReferences.Add(nameElement.UniqueId))
        {
            return;
        }

        var reference = new LuaReference(new(nameElement), kind);
        ProjectIndex.AddReference(DocumentId, symbol, reference);
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

    public LuaSymbol? GetAttachedDeclaration(LuaSyntaxElement element)
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

    public void AddDiagnostic(Diagnostic diagnostic)
    {
        if (Compilation.Diagnostics.CanAddDiagnostic(DocumentId, diagnostic.Code, diagnostic.Range))
        {
            Compilation.Diagnostics.AddDiagnostic(DocumentId, diagnostic);
        }
    }
}
