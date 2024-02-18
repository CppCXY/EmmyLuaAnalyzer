using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.FlowAnalyzer.ControlFlow;

public class BasicBlock
{
    public List<LuaStatSyntax> Statements { get; } = new();

    public List<BasicBlock> Successors { get; } = new();
}
