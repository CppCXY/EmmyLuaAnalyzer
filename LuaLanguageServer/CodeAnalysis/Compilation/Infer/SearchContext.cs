using LuaLanguageServer.CodeAnalysis.Compilation.Infer.Searcher;
using LuaLanguageServer.CodeAnalysis.Compilation.StubIndex;
using LuaLanguageServer.CodeAnalysis.Compilation.Type;
using LuaLanguageServer.CodeAnalysis.Kind;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Infer;

public class SearchContext
{
    public LuaCompilation Compilation { get; }

    private Dictionary<LuaSyntaxElement, ILuaType> _caches = new();

    private Dictionary<LuaSyntaxElement, LuaTypeMember> _memberCaches = new();

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

    public TMember? InferMember<TMember>(LuaSyntaxElement element, Func<TMember?> factory)
        where TMember : LuaTypeMember
    {
        if (_memberCaches.TryGetValue(element, out var member))
        {
            return member as TMember;
        }

        var result = factory();
        if (result is not null)
        {
            _memberCaches[element] = result;
        }

        return result;
    }

    public ILuaType FindLuaType(string name)
    {
        foreach (var searcher in _searchers)
        {
            if (searcher.TrySearchType(name, this, out var ty))
            {
                return ty;
            }
        }
        return Compilation.Builtin.Unknown;
    }

    public IEnumerable<TMember> FindMembers<TMember>(ILuaType type)
    {
        return _searchers.SelectMany(searcher => searcher.SearchMembers(type, this).OfType<TMember>());
    }
}
