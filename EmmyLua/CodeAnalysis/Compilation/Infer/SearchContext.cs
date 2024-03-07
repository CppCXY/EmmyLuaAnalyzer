using EmmyLua.CodeAnalysis.Compilation.Infer.Searcher;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Infer;

public class SearchContext
{
    public LuaCompilation Compilation { get; }

    private Dictionary<LuaSyntaxElement, LuaType> Caches { get; } = new();

    private List<ILuaSearcher> Searchers { get; } = new();

    private HashSet<LuaSyntaxElement> InferGuard { get; } = new();

    private const int MaxDepth = 1000;

    // 推断深度
    private int _currentDepth = 0;

    public EnvSearcher EnvSearcher { get; } = new();

    public IndexSearcher IndexSearcher { get; } = new();

    public SearchContext(LuaCompilation compilation)
    {
        Compilation = compilation;
        Searchers.Add(EnvSearcher);
        Searchers.Add(IndexSearcher);
    }

    public LuaType Infer(LuaSyntaxElement? element)
    {
        if (element is null)
        {
            return Builtin.Unknown;
        }

        return InferCore(element);
        // Caches.TryGetValue(element, out var symbol) ? symbol : Caches[element] = InferCore(element);
    }

    public void ClearCache()
    {
        Caches.Clear();
    }

    private LuaType InferCore(LuaSyntaxElement element)
    {
        if (_currentDepth > MaxDepth)
        {
            return Builtin.Unknown;
        }

        if (!InferGuard.Add(element))
        {
            return Builtin.Unknown;
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
                _ => Builtin.Unknown
            };
        }
        finally
        {
            _currentDepth--;
            InferGuard.Remove(element);
        }
    }

    public IEnumerable<Declaration> GetMembers(string name)
    {
        return Searchers.SelectMany(searcher => searcher.SearchMembers(name, this));
    }

    public IEnumerable<Declaration> FindMember(LuaType luaType, string memberName)
    {
        if (luaType is LuaNamedType namedType)
        {
            return GetMembers(namedType.Name)
                .Where(it => string.Equals(it.Name, memberName, StringComparison.CurrentCulture));
        }

        return Enumerable.Empty<Declaration>();
    }
}
