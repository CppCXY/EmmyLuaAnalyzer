// using EmmyLua.CodeAnalysis.Compilation.Semantic.Render;
// using EmmyLua.CodeAnalysis.Compilation.Semantic.Render.Renderer;

namespace EmmyLua.Cli.DocGenerator.Markdown;

// public class ModuleDoc
// {
//     private SearchContext SearchContext { get; }
//
//     private LuaRenderContext RenderContext { get; }
//
//     private LuaRenderBuilder RenderBuilder { get; }
//
//     private ModuleIndex ModuleIndex { get; }
//
//     private LuaRenderFeature Feature { get; } = new LuaRenderFeature()
//     {
//         ShowTypeLink = false,
//         ExpandAlias = false,
//     };
//
//     public ModuleDoc(LuaCompilation compilation, ModuleIndex moduleIndex)
//     {
//         SearchContext = new SearchContext(compilation, new SearchContextFeatures());
//         RenderBuilder = new LuaRenderBuilder(SearchContext);
//         ModuleIndex = moduleIndex;
//         RenderContext = new LuaRenderContext(SearchContext, Feature);
//     }
//
//     public string Build()
//     {
//         RenderContext.AddH1Title($"module {ModuleIndex.ModulePath}");
//         var document = SearchContext.Compilation.Workspace.GetDocument(ModuleIndex.DocumentId);
//         if (document is null)
//         {
//             return RenderContext.GetText();
//         }
//
//         RenderModuleDescription(document);
//         RenderContext.AppendLine();
//
//         RenderContext.AddH2Title("Public members:");
//         RenderContext.AddSeparator();
//         RenderModuleMembers(document);
//
//         return RenderContext.GetText();
//     }
//
//     private void RenderModuleDescription(LuaDocument document)
//     {
//         RenderContext.Append(RenderBuilder.RenderModule(document, Feature));
//     }
//     
//     private IEnumerable<LuaFuncStatSyntax> GetModuleStats(LuaDocument document)
//     {
//         if (document.SyntaxTree.SyntaxRoot.Block is { StatList: { } statList })
//         {
//             foreach (var funcStat in statList.OfType<LuaFuncStatSyntax>())
//             {
//                 yield return funcStat;
//             }
//         }
//     }
//
//     private void RenderModuleMembers(LuaDocument document)
//     {
//         foreach (var funcStat in GetModuleStats(document))
//         {
//             if (funcStat is { NameElement.Parent: { } node })
//             {
//                 var symbol = SearchContext.FindDeclaration(node);
//                 if (symbol is LuaSymbol luaSymbol)
//                 {
//                     RenderFuncDeclaration(luaSymbol, funcStat);
//                     RenderContext.AddSeparator();
//                 }
//             }
//         }
//     }
//
//     private void RenderFuncDeclaration(LuaSymbol symbol, LuaFuncStatSyntax funcStat)
//     {
//         if (symbol.IsLocal || symbol.IsPrivate)
//         {
//             return;
//         }
//
//         var asyncText = symbol.IsAsync ? "async " : string.Empty;
//
//         if (symbol.Info is MethodInfo methodInfo)
//         {
//             if (methodInfo.IndexPtr.ToNode(SearchContext) is { } indexExpr)
//             {
//                 RenderContext.WrapperLua(() =>
//                 {
//                     RenderContext.Append($"{asyncText}function {indexExpr.Text}");
//                     LuaTypeRenderer.RenderFunc(methodInfo.Method, RenderContext);
//                 });
//             }
//             else if (methodInfo.NamePtr.ToNode(SearchContext) is { } nameExpr)
//             {
//                 RenderContext.WrapperLua(() =>
//                 {
//                     RenderContext.Append($"{asyncText}function {nameExpr.Text}");
//                     LuaTypeRenderer.RenderFunc(methodInfo.Method, RenderContext);
//                 });
//             }
//
//             var comments = funcStat.Comments;
//             foreach (var comment in comments)
//             {
//                 if (comment.CommentText is { Length: > 0 } commentText)
//                 {
//                     RenderContext.Append(commentText);
//                 }
//
//                 RenderContext.AppendLine();
//             }
//         }
//     }
// }