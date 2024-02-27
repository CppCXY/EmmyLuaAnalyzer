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

    private Dictionary<LuaSyntaxElement, LuaType> _caches = new();

    private List<ILuaSearcher> _searchers = new();

    private HashSet<LuaType> _substituteGuard = new();

    private HashSet<LuaType> _subTypesGuard = new();

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

    public LuaType Infer(LuaSyntaxElement? element)
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

    private LuaType InferCore(LuaSyntaxElement element)
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

    public LuaType FindLuaType(string name)
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

    public IEnumerable<Declaration> GetMembers(string name)
    {
        return _searchers.SelectMany(searcher => searcher.SearchMembers(name, this));
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

    public IEnumerable<GenericParameterDeclaration> FindGenericParams(string name)
    {
        return _searchers.SelectMany(searcher => searcher.SearchGenericParams(name, this));
    }

    public IEnumerable<LuaType> FindSupers(string name)
    {
        return _searchers.SelectMany(searcher => searcher.SearchSupers(name, this));
    }

    public string GetUniqueId(LuaSyntaxElement element)
    {
        var document = element.Tree.Document;
        var documentId = document.Id;
        return $"{documentId.Id}|{Compilation.SymbolTrees[documentId].GetPosition(element)}";
    }

    public bool TryAddSubstitute(LuaType type)
    {
        return _substituteGuard.Add(type);
    }

    public void RemoveSubstitute(LuaType type)
    {
        _substituteGuard.Remove(type);
    }

    public bool TryAddSubType(LuaType type)
    {
        return _subTypesGuard.Add(type);
    }

    public void RemoveSubType(LuaType type)
    {
        _subTypesGuard.Remove(type);
    }
}
