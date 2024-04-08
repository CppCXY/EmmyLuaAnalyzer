using EmmyLua.CodeAnalysis.Compilation.Analyzer.FlowAnalyzer.ControlFlow;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.FlowAnalyzer;

public class FlowAnalyzer(LuaCompilation compilation) : LuaAnalyzer(compilation)
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
                    if (!Compilation.ControlFlowGraphs.TryGetValue(documentId, out var cfgDict))
                    {
                        cfgDict = new Dictionary<LuaBlockSyntax, ControlFlowGraph>();
                        Compilation.ControlFlowGraphs[documentId] = cfgDict;
                    }

                    cfgDict.Add(block, builder.Build(block));
                }
            }
        }
    }
}
