using EmmyLua.CodeAnalysis.Compilation.Analyzer.FlowAnalyzer.ControlFlow;
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
            var blocks = syntaxTree.SyntaxRoot.Descendants.OfType<LuaBlockSyntax>();
            foreach (var block in blocks)
            {
                if (block.Parent is LuaSourceSyntax or LuaClosureExprSyntax)
                {
                    analyzeContext.ControlFlowGraphs[block.UniqueId] = builder.Build(block);
                }
            }
        }
    }
}
