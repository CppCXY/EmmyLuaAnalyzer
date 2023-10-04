using LuaLanguageServer.CodeAnalysis.Compilation.Symbol;
using LuaLanguageServer.CodeAnalysis.Kind;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Infer;

public class SearchContext
{
    public LuaCompilation Compilation { get; }

    private Dictionary<LuaSyntaxElement, ILuaSymbol> _caches = new();

    public SearchContext(LuaCompilation compilation)
    {
        Compilation = compilation;
    }

    public ILuaSymbol Infer(LuaSyntaxElement? element)
    {
        if (element is null)
        {
            return Compilation.Builtin.Unknown;
        }

        return _caches.TryGetValue(element, out var symbol) ? symbol : _caches[element] = InferCore(element);
    }

    private ILuaSymbol InferCore(LuaSyntaxElement element)
    {
        return element switch
        {
            LuaExprSyntax expr => ExpressionInfer.InferExpr(expr, this),
            LuaLocalNameSyntax localName => DeclarationInfer.InferLocalName(localName, this),
            LuaFuncStatSyntax funcStat => throw new NotImplementedException(),
            _ => Compilation.Builtin.Unknown
        };
    }
}
