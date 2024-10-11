using EmmyLua.CodeAnalysis.Compilation.Analyzer.ResolveAnalyzer;
using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Index;
using EmmyLua.CodeAnalysis.Compilation.Reference;
using EmmyLua.CodeAnalysis.Compilation.Scope;
using EmmyLua.CodeAnalysis.Compilation.Signature;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Compilation.Type.Types;
using EmmyLua.CodeAnalysis.Compile.Kind;
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

    private HashSet<SyntaxElementId> _uniqueReferences = new();

    private Dictionary<SyntaxElementId, LuaSymbol> _declarations = new();

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
        var cur = element.Iter;
        while (cur.IsValid)
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

    public void PushScope(SyntaxIterator it)
    {
        if (_scopeOwners.TryGetValue(it.UniqueId, out var scope))
        {
            _scopeStack.Push(scope);
            _curScope = scope;
            return;
        }

        var position = it.Position;
        switch (it.Kind)
        {
            case LuaSyntaxKind.LocalStat:
            {
                SetScope(new LocalStatDeclarationScope(position, [], _curScope), it);
                break;
            }
            case LuaSyntaxKind.RepeatStat:
            {
                SetScope(new RepeatStatDeclarationScope(position, [], _curScope), it);
                break;
            }
            case LuaSyntaxKind.ForRangeStat:
            {
                SetScope(new ForRangeStatDeclarationScope(position, [], _curScope), it);
                break;
            }
            default:
            {
                SetScope(new DeclarationScope(position, [], _curScope), it);
                break;
            }
        }
    }

    private void SetScope(DeclarationScope scope, SyntaxIterator it)
    {
        _scopeStack.Push(scope);
        _topScope ??= scope;
        _scopeOwners.Add(it.UniqueId, scope);
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

    public void AddDiagnostic(Diagnostic diagnostic)
    {
        if (Compilation.Diagnostics.CanAddDiagnostic(DocumentId, diagnostic.Code, diagnostic.Range))
        {
            Compilation.Diagnostics.AddDiagnostic(DocumentId, diagnostic);
        }
    }

    public void AddIndexExprMember(LuaIndexExprSyntax indexExpr, LuaSymbol member)
    {
        if (indexExpr.PrefixExpr is LuaNameExprSyntax nameExpr)
        {
            var prevSymbol = FindLocalDeclaration(nameExpr);
            if (prevSymbol?.Type is LuaNamedType namedType)
            {
                var typeInfo = TypeManager.FindTypeInfo(namedType);
                typeInfo?.AddImplement(member);
                return;
            }
        }

        var unResolvedIndex = new UnResolvedSymbol(
            member,
            null,
            ResolveState.UnResolvedIndex
        );

        AddUnResolved(unResolvedIndex);
    }

    public LuaSymbol? FindLocalSymbol(LuaSyntaxElement element)
    {
        return _declarations.GetValueOrDefault(element.UniqueId);
    }

    public LuaTypeRef CreateRef(LuaDocTypeSyntax typeSyntax)
    {
        var refType = new LuaTypeRef(LuaTypeId.Create(typeSyntax));
        AnalyzeContext.TypeRefs.Add(refType);
        return refType;
    }
}
