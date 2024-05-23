using EmmyLua.CodeAnalysis.Compilation.Analyzer.FlowAnalyzer.ControlFlow;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.FlowAnalyzer;

public class FlowAnalyzer(LuaCompilation compilation) : LuaAnalyzer(compilation, "Flow")
{
    public override void Analyze(AnalyzeContext analyzeContext)
    {
        var cfgTuples = analyzeContext.LuaDocuments
            .SelectMany(it => it.SyntaxTree.SyntaxRoot.Descendants.OfType<LuaBlockSyntax>())
            // .AsParallel()
            .Select(it => (it.UniqueId, new CfgBuilder().Build(it)));

        foreach (var cfgTuple in cfgTuples)
        {
            analyzeContext.ControlFlowGraphs[cfgTuple.Item1] = cfgTuple.Item2;
        }
    }
}
