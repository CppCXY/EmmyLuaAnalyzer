using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Infer.Searcher;
using LuaLanguageServer.CodeAnalysis.Compilation.Type;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;
using LuaLanguageServer.CodeAnalysis.Workspace;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Infer;

public class SearchContext
{
    public LuaCompilation Compilation { get; }

    private Dictionary<LuaSyntaxElement, ILuaType> _caches = new();

    private List<ILuaSearcher> _searchers = new();

    private HashSet<ILuaType> _substituteGuard = new();

    private const int MaxDepth = 1000;

    // 推断深度
    private int _currentDepth = 0;

    public CallExprInfer CallExprInfer { get; } = new();

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

    private ILuaType InferCore(LuaSyntaxElement element)
    {
        if (_currentDepth > MaxDepth)
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
                LuaFuncStatSyntax funcStat => throw new NotImplementedException(),
                LuaSourceSyntax source => DeclarationInfer.InferSource(source, this),
                _ => Compilation.Builtin.Unknown
            };
        }
        finally
        {
            _currentDepth--;
        }
    }

    public ILuaNamedType FindLuaType(string name)
    {
        foreach (var searcher in _searchers)
        {
            if (searcher.TrySearchLuaType(name, this, out var ty) && ty is not null)
            {
                return ty;
            }
        }

        return Compilation.Builtin.Unknown;
    }

    public IEnumerable<Declaration.Declaration> FindMembers(ILuaType type)
    {
        return _searchers.SelectMany(searcher => searcher.SearchMembers(type, this));
    }

    public IEnumerable<Declaration.Declaration> FindGenericParams(string name)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<ILuaType> FindSupers(string name)
    {
        throw new NotImplementedException();
    }

    public string GetUniqueId(LuaSyntaxElement element, DocumentId documentId)
    {
        return $"{documentId.Guid}:{Compilation.DeclarationTrees[documentId].GetPosition(element)}";
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
