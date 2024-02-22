using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.FlowAnalyzer.ControlFlow;

public class CfgEdge(CfgNode source, CfgNode target, LuaExprSyntax? condition = null)
{
    public CfgNode Target { get; } = target;

    public CfgNode Source { get; } = source;

    public LuaExprSyntax? Condition { get; } = condition;
}
