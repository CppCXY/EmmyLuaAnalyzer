using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.FlowAnalyzer.ControlFlow;

public class CfgNode(CfgNodeKind kind)
{
    public CfgNodeKind Kind { get; } = kind;

    public List<LuaStatSyntax> Statements { get; } = new();

    public List<CfgEdge>? Incomings { get; private set; }

    public List<CfgEdge>? Outgoings { get; private set; }

    public IEnumerable<CfgNode> Successors => Outgoings?.Select(e => e.Target) ?? Enumerable.Empty<CfgNode>();

    public void AddIncoming(CfgEdge edge)
    {
        Incomings ??= new();
        Incomings.Add(edge);
    }

    public void AddOutgoing(CfgEdge edge)
    {
        Outgoings ??= new();
        Outgoings.Add(edge);
    }
}
