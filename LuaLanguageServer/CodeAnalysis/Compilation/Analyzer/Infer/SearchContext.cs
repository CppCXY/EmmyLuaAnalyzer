using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Infer.Searcher;
using LuaLanguageServer.CodeAnalysis.Compilation.Symbol;
using LuaLanguageServer.CodeAnalysis.Compilation.Type;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Infer;

public class SearchContext
{
    public LuaCompilation Compilation { get; }

    private Dictionary<LuaSyntaxElement, ILuaType> _caches = new();

    private List<ILuaSearcher> _searchers = new();

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

    public IEnumerable<ILuaSymbol> FindMembers(ILuaType type)
    {
        return _searchers.SelectMany(searcher => searcher.SearchMembers(type, this));
    }
}
