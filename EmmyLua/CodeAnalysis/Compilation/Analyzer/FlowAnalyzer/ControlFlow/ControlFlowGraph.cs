using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Syntax.Tree;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.FlowAnalyzer.ControlFlow;

public class ControlFlowGraph
{
    public CfgNode EntryNode { get; } = new(CfgNodeKind.Entry);
    public CfgNode ExitNode { get; } = new(CfgNodeKind.Exit);

    public List<CfgNode> Nodes { get; } = new();

    private List<CfgEdge> Edges { get; } = new();

    public void AddEdge(CfgNode source, CfgNode target, LuaExprSyntax? condition = null)
    {
        var edge = new CfgEdge(source, target, condition);
        Edges.Add(edge);
        source.AddOutgoing(edge);
        target.AddIncoming(edge);
    }

    public CfgNode CreateNode(CfgNodeKind kind = CfgNodeKind.BasicBlock)
    {
        var block = new CfgNode(kind);
        Nodes.Add(block);
        return block;
    }


}
