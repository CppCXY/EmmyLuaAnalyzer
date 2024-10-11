using EmmyLua.CodeAnalysis.Compilation.Analyzer.FlowAnalyzer.ControlFlow;
using EmmyLua.CodeAnalysis.Compile.Kind;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.FlowAnalyzer;

public class FlowAnalyzer(LuaCompilation compilation) : LuaAnalyzer(compilation, "Flow")
{
    public override void Analyze(AnalyzeContext analyzeContext)
    {
        foreach (var document in analyzeContext.LuaDocuments)
        {
            var syntaxTree = document.SyntaxTree;

            var builder = new CfgBuilder();
            var blocks = syntaxTree.SyntaxRoot.Iter.DescendantsOfKind(LuaSyntaxKind.Block);
            foreach (var it in blocks)
            {
                if (it.Parent.Kind is LuaSyntaxKind.Source or LuaSyntaxKind.ClosureExpr)
                {
                    analyzeContext.ControlFlowGraphs[it.UniqueId] = builder.Build(it.ToNode<LuaBlockSyntax>()!);
                }
            }
        }
    }
}
