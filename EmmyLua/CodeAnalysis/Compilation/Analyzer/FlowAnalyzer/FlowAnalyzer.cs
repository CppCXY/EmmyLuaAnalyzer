using EmmyLua.CodeAnalysis.Compilation.Analyzer.FlowAnalyzer.ControlFlow;
using EmmyLua.CodeAnalysis.Compilation.Analyzer.FlowAnalyzer.DataFlow;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.FlowAnalyzer;

public class FlowAnalyzer(LuaCompilation compilation) : LuaAnalyzer(compilation)
{
    // private List<FlowAnalyzerBase> Analyzers { get; } = new();
    //
    // private void AddAnalyzer(FlowAnalyzerBase analyzer)
    // {
    //     Analyzers.Add(analyzer);
    // }

    // AddAnalyzer(new ReachableAnalyzer(compilation));

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

            // foreach (var analyzer in Analyzers)
            // {
            //     foreach (var cfg in Compilation.ControlFlowGraphs[documentId].Values)
            //     {
            //         analyzer.Analyze(cfg, syntaxTree);
            //     }
            // }
        }
    }
}
