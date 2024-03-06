using EmmyLua.CodeAnalysis.Compilation.Analyzer.FlowAnalyzer.ControlFlow;
using EmmyLua.CodeAnalysis.Syntax.Tree;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.FlowAnalyzer.DataFlow;

public class TypeTighteningAnalyzer(LuaCompilation compilation) : FlowAnalyzerBase(compilation)
{
    public override void Analyze(ControlFlowGraph cfg, LuaSyntaxTree tree)
    {

    }
}
