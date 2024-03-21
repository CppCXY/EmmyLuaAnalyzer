using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Compilation.Semantic.Render;
using EmmyLua.CodeAnalysis.Compile.Diagnostic;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;

namespace EmmyLua.CodeAnalysis.Compilation.Semantic;

public class SemanticModel
{
    public LuaCompilation Compilation { get; }

    public LuaDocument Document { get; }

    public SearchContext Context { get; }

    public LuaRenderBuilder RenderBuilder { get; }

    public LuaDeclarationTree DeclarationTree { get; }

    public SemanticModel(LuaCompilation compilation, LuaDocument document, LuaDeclarationTree declarationTree)
    {
        Compilation = compilation;
        Document = document;
        Context = new(compilation);
        RenderBuilder = new(Context);
        DeclarationTree = declarationTree;
    }

    // 渲染符号的文档和类型
    public string RenderSymbol(LuaSyntaxElement? symbol)
    {
        if (symbol is null)
        {
            return string.Empty;
        }

        return RenderBuilder.Render(symbol);
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
