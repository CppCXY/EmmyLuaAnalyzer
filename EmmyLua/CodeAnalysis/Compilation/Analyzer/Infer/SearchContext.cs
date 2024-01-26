using EmmyLua.CodeAnalysis.Compilation.Analyzer.Infer.Searcher;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.Infer;

public class SearchContext
{
    public LuaCompilation Compilation { get; }

    private Dictionary<LuaSyntaxElement, ILuaType> _caches = new();

    private List<ILuaSearcher> _searchers = new();

    private HashSet<ILuaType> _substituteGuard = new();

    private HashSet<LuaSyntaxElement> _inferGuard = new();

    private const int MaxDepth = 1000;

    // 推断深度
    private int _currentDepth = 0;

    public EnvSearcher EnvSearcher { get; } = new();

    public IndexSearcher IndexSearcher { get; } = new();

    public SearchContext(LuaCompilation compilation)
    {
        Compilation = compilation;
        _searchers.Add(EnvSearcher);
        _searchers.Add(IndexSearcher);
    }

    public ILuaType Infer(LuaSyntaxElement? element)
    {
        if (element is null)
        {
            return Compilation.Builtin.Unknown;
        }

        return _caches.TryGetValue(element, out var symbol) ? symbol : _caches[element] = InferCore(element);
    }

    public void ClearCache()
    {
        _caches.Clear();
    }

    private ILuaType InferCore(LuaSyntaxElement element)
    {
        if (_currentDepth > MaxDepth)
        {
            return Compilation.Builtin.Unknown;
        }

        if (!_inferGuard.Add(element))
        {
            return Compilation.Builtin.Unknown;
        }

        try
        {
            _currentDepth++;
            return element switch
            {
                LuaExprSyntax expr => ExpressionInfer.InferExpr(expr, this),
                LuaLocalNameSyntax localName => DeclarationInfer.InferLocalName(localName, this),
                LuaParamDefSyntax paramDef => DeclarationInfer.InferParam(paramDef, this),
                LuaSourceSyntax source => DeclarationInfer.InferSource(source, this),
                LuaDocTypeSyntax ty => TypeInfer.InferType(ty, this),
                _ => Compilation.Builtin.Unknown
            };
        }
        finally
        {
            _currentDepth--;
            _inferGuard.Remove(element);
        }
    }

    public ILuaType FindLuaType(string name)
    {
        foreach (var searcher in _searchers)
        {
            if (searcher.SearchType(name, this).FirstOrDefault() is { } ty)
            {
                return ty;
            }
        }

        return Compilation.Builtin.Unknown;
    }

    public IEnumerable<Symbol.Symbol> FindMembers(string name)
    {
        return _searchers.SelectMany(searcher => searcher.SearchMembers(name, this));
    }

    public IEnumerable<Symbol.Symbol> FindGenericParams(string name)
    {
        return _searchers.SelectMany(searcher => searcher.SearchGenericParams(name, this));
    }

    public IEnumerable<ILuaType> FindSupers(string name)
    {
        return _searchers.SelectMany(searcher => searcher.SearchSupers(name, this));
    }

    public string GetUniqueId(LuaSyntaxElement element)
    {
        var source = element.Tree.Document;
        if (source is LuaDocument document)
        {
            var documentId = document.Id;
            return $"{documentId.Guid}:{Compilation.SymbolTrees[documentId].GetPosition(element)}";
        }

        return string.Empty;
    }

    public bool TryAddSubstitute(ILuaType type)
    {
        return _substituteGuard.Add(type);
    }

    public void RemoveSubstitute(ILuaType type)
    {
        _substituteGuard.Remove(type);
    }
}
