using EmmyLua.CodeAnalysis.Compilation.Analyzer.ResolveAnalyzer;
using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Index;
using EmmyLua.CodeAnalysis.Compilation.Reference;
using EmmyLua.CodeAnalysis.Compilation.Scope;
using EmmyLua.CodeAnalysis.Compilation.Signature;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Diagnostics;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.DeclarationAnalyzer;

public class DeclarationBuilder(
    LuaDocument document,
    LuaCompilation compilation,
    AnalyzeContext analyzeContext)
{
    private DeclarationScope? _topScope;

    private DeclarationScope? _curScope;

    private Stack<DeclarationScope> _scopeStack = new();

    private Dictionary<SyntaxElementId, DeclarationScope> _scopeOwners = new();

    private Dictionary<SyntaxElementId, List<LuaElementPtr<LuaDocTagSyntax>>> _attachedDocs = new();

    private HashSet<SyntaxElementId> _uniqueReferences = new();

    private Dictionary<SyntaxElementId, LuaSymbol> _declarations = new();

    private Dictionary<SyntaxElementId, LuaElementPtr<LuaClosureExprSyntax>> _relatedClosure = new();

    private LuaCompilation Compilation { get; } = compilation;

    public ProjectIndex ProjectIndex => Compilation.ProjectIndex;

    public GlobalIndex GlobalIndex => Compilation.GlobalIndex;

    public LuaTypeManager TypeManager => Compilation.TypeManager;

    public LuaSignatureManager SignatureManager => Compilation.SignatureManager;

    private AnalyzeContext AnalyzeContext { get; } = analyzeContext;

    public LuaDocument Document { get; } = document;

    public LuaDocumentId DocumentId => Document.Id;

    public LuaDeclarationTree? Build()
    {
        if (_topScope is not null)
        {
            return new LuaDeclarationTree(_scopeOwners, _topScope, _declarations, _relatedClosure, _attachedDocs);
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

    public void AttachToNext(LuaDocTagSyntax docTagSyntax)
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

    public void AddRelatedClosure(LuaSyntaxElement element, LuaClosureExprSyntax closureExprSyntax)
    {
        _relatedClosure.TryAdd(element.UniqueId, new(closureExprSyntax));
    }

    public void AddDiagnostic(Diagnostic diagnostic)
    {
        if (Compilation.Diagnostics.CanAddDiagnostic(DocumentId, diagnostic.Code, diagnostic.Range))
        {
            Compilation.Diagnostics.AddDiagnostic(DocumentId, diagnostic);
        }
    }
}
