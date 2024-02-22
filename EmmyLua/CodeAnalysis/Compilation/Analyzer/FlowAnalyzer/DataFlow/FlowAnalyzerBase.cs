using EmmyLua.CodeAnalysis.Compilation.Analyzer.FlowAnalyzer.ControlFlow;
using EmmyLua.CodeAnalysis.Syntax.Tree;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.FlowAnalyzer.DataFlow;

public abstract class FlowAnalyzerBase(LuaCompilation compilation)
{
    public LuaCompilation Compilation { get; } = compilation;

    public abstract void Analyze(ControlFlowGraph cfg, LuaSyntaxTree tree);
}
