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
    LuaDocumentId documentId,
    DeclarationAnalyzer analyzer,
    AnalyzeContext analyzeContext)
{
    private DeclarationScope? _topScope;

    private DeclarationScope? _curScope;

    private Stack<DeclarationScope> _scopeStack = new();

    private Dictionary<SyntaxElementId, DeclarationScope> _scopeOwners = new();

    private HashSet<SyntaxElementId> _uniqueReferences = new();

    private DeclarationAnalyzer Analyzer { get; } = analyzer;

    private LuaCompilation Compilation => Analyzer.Compilation;

    public WorkspaceIndex Db => Compilation.Db;

    private AnalyzeContext AnalyzeContext { get; } = analyzeContext;

    public LuaDocumentId DocumentId { get; } = documentId;

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

    public void AddDeclaration(int position, LuaDeclaration luaDeclaration)
    {
        _curScope?.Add(new DeclarationNode(position, luaDeclaration));
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

    public void SetScope(DeclarationScope scope, LuaSyntaxElement element)
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

}
