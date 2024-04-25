using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Compilation.Semantic.Reference;
using EmmyLua.CodeAnalysis.Compilation.Semantic.Render;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Diagnostics;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;

namespace EmmyLua.CodeAnalysis.Compilation.Semantic;

public class SemanticModel
{
    public LuaCompilation Compilation { get; }

    public LuaDocument Document { get; }

    public SearchContext Context { get; }

    private LuaRenderBuilder RenderBuilder { get; }

    private References References { get; }

    public LuaDeclarationTree DeclarationTree { get; }

    public SemanticModel(LuaCompilation compilation, LuaDocument document, LuaDeclarationTree declarationTree)
    {
        Compilation = compilation;
        Document = document;
        Context = new(compilation, new SearchContextFeatures());
        RenderBuilder = new(Context);
        References = new(Context);
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

    public IEnumerable<LuaReference> FindReferences(LuaSyntaxElement element)
    {
        return References.FindReferences(element);
    }

    public IEnumerable<Diagnostic> GetDiagnostics()
    {
        return Compilation.GetDiagnostics(Document.Id);
    }

    public IEnumerable<Diagnostic> PopDiagnostics()
    {
        return Compilation.PopDiagnostics(Document.Id);
    }

    public IEnumerable<LuaDeclaration> GetGlobals()
    {
        return Compilation.DbManager.GetGlobals();
    }

    public IEnumerable<LuaDeclaration> GetDeclarationsBefore(LuaSyntaxElement beforeToken)
    {
        return DeclarationTree.GetDeclarations(beforeToken);
    }

    public LuaType? GetExportType(LuaDocumentId documentId)
    {
        return Compilation.DbManager.GetModuleExportType(documentId);
    }
}
