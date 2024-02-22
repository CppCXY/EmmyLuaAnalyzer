using EmmyLua.CodeAnalysis.Compilation.Analyzer.FlowAnalyzer.ControlFlow;
using EmmyLua.CodeAnalysis.Compilation.Analyzer.FlowAnalyzer.DataFlow;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.FlowAnalyzer;

public class FlowAnalyzer : LuaAnalyzer
{
    private List<FlowAnalyzerBase> _analyzers = new();

    private void AddAnalyzer(FlowAnalyzerBase analyzer)
    {
        _analyzers.Add(analyzer);
    }

    public FlowAnalyzer(LuaCompilation compilation) : base(compilation)
    {
        AddAnalyzer(new ReachableAnalyzer(compilation));
    }

    public override void Analyze(DocumentId documentId)
    {
        if (Compilation.GetSyntaxTree(documentId) is { } syntaxTree)
        {
            var builder = new CfgBuilder();
            var blocks = syntaxTree.SyntaxRoot.Descendants.OfType<LuaBlockSyntax>();
            foreach (var block in blocks)
            {
                if (block.Parent is LuaSourceSyntax or LuaFuncBodySyntax)
                {
                    if (!Compilation.ControlFlowGraphs.TryGetValue(documentId, out var cfgDict))
                    {
                        cfgDict = new Dictionary<LuaBlockSyntax, ControlFlowGraph>();
                        Compilation.ControlFlowGraphs[documentId] = cfgDict;
                    }

                    cfgDict.Add(block, builder.Build(block));
                }
            }

            foreach (var analyzer in _analyzers)
            {
                foreach (var cfg in Compilation.ControlFlowGraphs[documentId].Values)
                {
                    analyzer.Analyze(cfg, syntaxTree);
                }
            }
        }
    }
}
