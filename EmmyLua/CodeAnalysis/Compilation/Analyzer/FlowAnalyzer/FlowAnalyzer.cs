using EmmyLua.CodeAnalysis.Compilation.Analyzer.FlowAnalyzer.ControlFlow;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.FlowAnalyzer;

public class FlowAnalyzer(LuaCompilation compilation) : LuaAnalyzer(compilation, "Flow")
{
    public override void Analyze(AnalyzeContext analyzeContext)
    {
        foreach (var document in analyzeContext.LuaDocuments)
        {
            var documentId = document.Id;
            var syntaxTree = document.SyntaxTree;

            var builder = new CfgBuilder();
            var blocks = syntaxTree.SyntaxRoot.Descendants.OfType<LuaBlockSyntax>();
            foreach (var block in blocks)
            {
                if (block.Parent is LuaSourceSyntax or LuaClosureExprSyntax)
                {
                    if (!analyzeContext.ControlFlowGraphs.TryGetValue(documentId, out var cfgDict))
                    {
                        cfgDict = new Dictionary<LuaBlockSyntax, ControlFlowGraph>();
                        analyzeContext.ControlFlowGraphs[documentId] = cfgDict;
                    }

                    cfgDict.TryAdd(block, builder.Build(block));
                }
            }
        }
    }
}
