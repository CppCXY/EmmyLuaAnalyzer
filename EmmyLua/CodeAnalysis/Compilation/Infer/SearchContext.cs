using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Infer;

public class SearchContext(LuaCompilation compilation, bool allowCache = true)
{
    public LuaCompilation Compilation { get; } = compilation;

    private Dictionary<LuaSyntaxElement, LuaType> Caches { get; } = new();

    private HashSet<LuaSyntaxElement> InferGuard { get; } = new();

    private const int MaxDepth = 1000;

    // 推断深度
    private int _currentDepth = 0;

    public LuaType Infer(LuaSyntaxElement? element)
    {
        if (element is null)
        {
            return Builtin.Unknown;
        }

        if (allowCache)
        {
            return Caches.TryGetValue(element, out var luaType) ? luaType : Caches[element] = InferCore(element);
        }
        else
        {
            return InferCore(element);
        }
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

    public IEnumerable<LuaDeclaration> GetMembers(string name)
    {
        if (name is "_G" or "_ENV")
        {
            return Compilation.ProjectIndex.GetGlobals();
        }

        return Compilation.ProjectIndex.GetMembers(name);
    }

    public IEnumerable<LuaDeclaration> FindMember(LuaType luaType, string memberName)
    {
        if (luaType is LuaNamedType namedType)
        {
            return GetMembers(namedType.Name)
                .Where(it => string.Equals(it.Name, memberName, StringComparison.CurrentCulture));
        }
        else if (luaType is LuaUnionType unionType)
        {
            return unionType.UnionTypes.SelectMany(it => FindMember(it, memberName));
        }

        return Enumerable.Empty<LuaDeclaration>();
    }
}
