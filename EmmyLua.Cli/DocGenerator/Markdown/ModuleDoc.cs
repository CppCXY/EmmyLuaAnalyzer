using EmmyLua.CodeAnalysis.Compilation;
using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Compilation.Semantic.Render;
using EmmyLua.CodeAnalysis.Compilation.Semantic.Render.Renderer;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Workspace.Module;

namespace EmmyLua.Cli.DocGenerator.Markdown;

public class ModuleDoc
{
    private SearchContext SearchContext { get; }

    private LuaRenderContext RenderContext { get; }

    private LuaRenderBuilder RenderBuilder { get; }

    private ModuleIndex ModuleIndex { get; }

    private LuaRenderFeature Feature { get; } = new LuaRenderFeature()
    {
        ShowTypeLink = false,
        ExpandAlias = false,
    };

    public ModuleDoc(LuaCompilation compilation, ModuleIndex moduleIndex)
    {
        SearchContext = new SearchContext(compilation, new SearchContextFeatures());
        RenderBuilder = new LuaRenderBuilder(SearchContext);
        ModuleIndex = moduleIndex;
        RenderContext = new LuaRenderContext(SearchContext, Feature);
    }

    public string Build()
    {
        RenderContext.AddH1Title(ModuleIndex.ModulePath);
        var document = SearchContext.Compilation.Workspace.GetDocument(ModuleIndex.DocumentId);
        if (document is null)
        {
            return RenderContext.GetText();
        }

        RenderModuleDescription(document);
        RenderContext.AppendLine();

        RenderContext.AddH2Title("Public members:");
        RenderContext.AddSeparator();
        RenderModuleMembers(document);

        return RenderContext.GetText();
    }

    private void RenderModuleDescription(LuaDocument document)
    {
        RenderContext.Append(RenderBuilder.RenderModule(document, Feature));
    }

    private IEnumerable<LuaFuncStatSyntax> GetModuleStats(LuaDocument document)
    {
        if (document.SyntaxTree.SyntaxRoot?.Block is { StatList: { } statList })
        {
            foreach (var funcStat in statList.OfType<LuaFuncStatSyntax>())
            {
                yield return funcStat;
            }
        }
    }

    private void RenderModuleMembers(LuaDocument document)
    {
        foreach (var funcStat in GetModuleStats(document))
        {
            if (funcStat is { NameElement.Parent: { } node })
            {
                var declaration = SearchContext.FindDeclaration(node);
                if (declaration is not null)
                {
                    RenderFuncDeclaration(declaration, funcStat);
                    RenderContext.AddSeparator();
                }
            }
        }
    }

    private void RenderFuncDeclaration(LuaDeclaration declaration, LuaFuncStatSyntax funcStat)
    {
        if (declaration.IsLocal || declaration.IsPrivate)
        {
            return;
        }

        var asyncText = declaration.IsAsync ? "async " : string.Empty;

        if (declaration.Info is MethodInfo methodInfo)
        {
            if (methodInfo.IndexPtr.ToNode(SearchContext) is { } indexExpr)
            {
                RenderContext.WrapperLua(() =>
                {
                    RenderContext.Append($"{asyncText}function {indexExpr.Text}");
                    LuaTypeRenderer.RenderFunc(methodInfo.Method, RenderContext);
                });
            }
            else if (methodInfo.NamePtr.ToNode(SearchContext) is { } nameExpr)
            {
                RenderContext.WrapperLua(() =>
                {
                    RenderContext.Append($"{asyncText}function {nameExpr.Text}");
                    LuaTypeRenderer.RenderFunc(methodInfo.Method, RenderContext);
                });
            }

            var comments = funcStat.Comments;
            foreach (var comment in comments)
            {
                if (comment.CommentText is { Length: > 0 } commentText)
                {
                    // RenderContext.Append("    ");
                    RenderContext.Append(commentText); //.Replace("\n", "\n    "));
                }

                RenderContext.AppendLine();
            }
        }
    }
}