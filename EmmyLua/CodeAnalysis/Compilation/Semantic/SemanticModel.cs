using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Compile.Diagnostic;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;

namespace EmmyLua.CodeAnalysis.Compilation.Semantic;

public class SemanticModel(LuaCompilation compilation, LuaDocument document)
{
    public LuaCompilation Compilation { get; } = compilation;

    public LuaDocument Document { get; } = document;

    public SearchContext Context { get; } = new(compilation);

    public LuaSymbol? GetSymbol(LuaSyntaxElement element)
    {
        var symbolTree = Compilation.GetSymbolTree(Document.Id);
        if (symbolTree?.FindSymbol(element) is { } symbol)
        {
            return symbol;
        }

        return null;
    }

    // 渲染符号的文档和类型
    public string RenderSymbol(LuaSymbol? symbol)
    {
        // if (symbol is null)
        // {
        //     return "";
        // }
        //
        // return symbol.Render();
        throw new NotImplementedException();
    }

    public IEnumerable<Diagnostic> GetDiagnostic()
    {
        var tree = Compilation.GetSyntaxTree(Document.Id);

        return tree?.Diagnostics.Select(it => it.WithLocation(
                   tree.Document.GetLocation(it.Range)
               ))
               ?? Enumerable.Empty<Diagnostic>();
    }
}
