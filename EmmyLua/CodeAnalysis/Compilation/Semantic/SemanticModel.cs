using EmmyLua.CodeAnalysis.Common;
using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Compilation.Semantic.Reference;
using EmmyLua.CodeAnalysis.Compilation.Semantic.Render;
using EmmyLua.CodeAnalysis.Diagnostics;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Type;

namespace EmmyLua.CodeAnalysis.Compilation.Semantic;

public class SemanticModel
{
    public LuaCompilation Compilation { get; }

    public LuaDocument Document { get; }

    public SearchContext Context { get; }

    public LuaRenderBuilder RenderBuilder { get; }

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
    public string RenderSymbol(LuaSyntaxElement? symbol, LuaRenderFeature feature)
    {
        if (symbol is null)
        {
            return string.Empty;
        }

        return RenderBuilder.Render(symbol, feature);
    }

    public IEnumerable<LuaReference> FindReferences(LuaSyntaxElement element)
    {
        return References.FindReferences(element);
    }

    public IEnumerable<Diagnostic> GetDiagnostics()
    {
        return Compilation.GetDiagnostics(Document.Id, Context);
    }

    public IEnumerable<IDeclaration> GetGlobals()
    {
        return Compilation.Db.QueryAllGlobal();
    }

    public IEnumerable<LuaDeclaration> GetDeclarationsBefore(LuaSyntaxElement beforeToken)
    {
        return DeclarationTree.GetDeclarationsBefore(beforeToken);
    }

    public LuaType? GetExportType(LuaDocumentId documentId)
    {
        return Compilation.Db.QueryModuleType(documentId);
    }
}
