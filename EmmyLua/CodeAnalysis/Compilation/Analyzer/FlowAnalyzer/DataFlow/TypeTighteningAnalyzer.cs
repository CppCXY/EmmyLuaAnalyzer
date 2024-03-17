using EmmyLua.CodeAnalysis.Compilation.Analyzer.FlowAnalyzer.ControlFlow;
using EmmyLua.CodeAnalysis.Syntax.Tree;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.FlowAnalyzer.DataFlow;

// TODO 等我有实力再来
public class TypeTighteningAnalyzer(LuaCompilation compilation) : FlowAnalyzerBase(compilation)
{
    public override void Analyze(ControlFlowGraph cfg, LuaSyntaxTree tree)
    {

    }
}
