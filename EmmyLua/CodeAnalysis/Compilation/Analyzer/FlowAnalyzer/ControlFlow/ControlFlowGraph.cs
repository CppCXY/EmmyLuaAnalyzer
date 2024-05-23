using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.FlowAnalyzer.ControlFlow;

public class ControlFlowGraph
{
    public CfgNode EntryNode { get; }
    public CfgNode ExitNode { get; }

    public List<CfgNode> Nodes { get; } = [];

    private List<CfgEdge> Edges { get; } = [];

    private Dictionary<int, List<int>> Predecessors { get; } = new();

    private Dictionary<int, List<int>> Successors { get; } = new();

    public ControlFlowGraph()
    {
        EntryNode = CreateNode(CfgNodeKind.Entry);
        ExitNode = CreateNode(CfgNodeKind.Exit);
    }

    public void AddEdge(CfgNode source, CfgNode target, LuaExprSyntax? condition = null)
    {
        var edge = new CfgEdge(source.Index, target.Index);
        Edges.Add(edge);
        if (!Successors.ContainsKey(source.Index))
        {
            Successors[source.Index] = [];
        }

        Successors[source.Index].Add(target.Index);
        if (!Predecessors.ContainsKey(target.Index))
        {
            Predecessors[target.Index] = [];
        }

        Predecessors[target.Index].Add(source.Index);
    }

    public CfgNode CreateNode(CfgNodeKind kind = CfgNodeKind.BasicBlock)
    {
        var index = Nodes.Count;
        var block = new CfgNode(index, kind);
        Nodes.Add(block);
        return block;
    }

    public IEnumerable<CfgNode> GetPredecessors(CfgNode node)
    {
        if (Predecessors.TryGetValue(node.Index, out var predecessors))
        {
            return predecessors.Select(i => Nodes[i]);
        }

        return [];
    }

    public IEnumerable<CfgNode> GetSuccessors(CfgNode node)
    {
        if (Successors.TryGetValue(node.Index, out var successors))
        {
            return successors.Select(i => Nodes[i]);
        }

        return [];
    }
}
