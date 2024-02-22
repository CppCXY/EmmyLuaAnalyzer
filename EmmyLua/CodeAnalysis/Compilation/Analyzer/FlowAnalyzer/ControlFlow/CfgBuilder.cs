using EmmyLua.CodeAnalysis.Compile.Diagnostic;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Syntax.Tree;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.FlowAnalyzer.ControlFlow;

public class CfgBuilder
{
    private ControlFlowGraph _graph = null!;

    private Dictionary<CfgNode, CfgNode> _loopExits = new();

    private Dictionary<CfgNode, Dictionary<string, CfgNode>> _scopeLabels = new();

    private class GotoNode(CfgNode source, LuaGotoStatSyntax gotoStat)
    {
        public LuaGotoStatSyntax GotoStat { get; } = gotoStat;
        public CfgNode Source { get; } = source;
    }

    private List<GotoNode> _gotoNodes = new();

    public ControlFlowGraph Build(LuaBlockSyntax block)
    {
        _graph = new ControlFlowGraph();
        var lastNode = BuildBlock(block, _graph.EntryNode);
        RecordScope(_graph.EntryNode);
        if (lastNode != _graph.ExitNode)
        {
            _graph.AddEdge(lastNode, _graph.ExitNode);
        }

        foreach (var gotoNode in _gotoNodes)
        {
            var scopeNode = FindScopeStart(gotoNode.Source);
            if (scopeNode is not null)
            {
                if (_scopeLabels.TryGetValue(scopeNode, out var labels))
                {
                    if (gotoNode.GotoStat.LabelName is { } label)
                    {
                        if (labels.TryGetValue(label.RepresentText, out var target))
                        {
                            _graph.AddEdge(gotoNode.Source, target);
                        }
                        else
                        {
                            label.PushDiagnostic(DiagnosticSeverity.Error, $"No Visible label {label.RepresentText}");
                        }
                    }
                    continue;
                }
            }
            gotoNode.GotoStat.Goto.PushDiagnostic(DiagnosticSeverity.Error, "Label not found");
        }

        var graph = _graph;
        _graph = null!;
        _loopExits.Clear();
        _scopeLabels.Clear();
        _gotoNodes.Clear();

        return graph;
    }

    private CfgNode BuildBlock(LuaBlockSyntax block, CfgNode firstCfgNode)
    {
        var currentBlock = firstCfgNode;
        foreach (var stat in block.StatList)
        {
            switch (stat)
            {
                case LuaIfStatSyntax ifStat:
                {
                    currentBlock = BuildIf(ifStat, currentBlock);
                    break;
                }
                case LuaWhileStatSyntax whileStat:
                {
                    currentBlock = BuildWhile(whileStat, currentBlock);
                    break;
                }
                case LuaRepeatStatSyntax repeatStat:
                {
                    currentBlock = BuildRepeat(repeatStat, currentBlock);
                    break;
                }
                case LuaReturnStatSyntax returnStat:
                {
                    currentBlock = BuildReturn(returnStat, currentBlock);
                    if (stat != block.StatList.Last())
                    {
                        currentBlock = _graph.CreateNode();
                    }

                    break;
                }
                case LuaForStatSyntax forStat:
                {
                    currentBlock = BuildForStat(forStat, currentBlock);
                    break;
                }
                case LuaForRangeStatSyntax forRangeStat:
                {
                    currentBlock = BuildForRangeStat(forRangeStat, currentBlock);
                    break;
                }
                case LuaDoStatSyntax doStat:
                {
                    currentBlock = BuildBlock(doStat.Block, currentBlock);
                    if (currentBlock == _graph.ExitNode)
                    {
                        currentBlock = _graph.CreateNode();
                    }

                    break;
                }
                case LuaBreakStatSyntax breakStat:
                {
                    currentBlock = BuildBreak(breakStat, currentBlock);
                    break;
                }
                case LuaGotoStatSyntax gotoStat:
                {
                    currentBlock = BuildGoto(gotoStat, currentBlock);
                    break;
                }
                case LuaLabelStatSyntax labelStat:
                {
                    currentBlock = BuildLabel(labelStat, currentBlock);
                    break;
                }
                default:
                {
                    currentBlock.Statements.Add(stat);
                    break;
                }
            }
        }

        return currentBlock;
    }

    private CfgNode BuildIf(LuaIfStatSyntax ifStat, CfgNode sourceBlock)
    {
        var condition = ifStat.Condition;

        var thenBlock = _graph.CreateNode();
        _graph.AddEdge(sourceBlock, thenBlock, condition);
        var lastBlocks = new List<CfgNode>();
        if (ifStat.ThenBlock is not null)
        {
            lastBlocks.Add(BuildBlock(ifStat.ThenBlock, thenBlock));
        }

        foreach (var elseIfOrElseClause in ifStat.IfClauseStatementList)
        {
            var elseIfCondition = elseIfOrElseClause.Condition;
            var elseIfBlock = _graph.CreateNode();
            _graph.AddEdge(sourceBlock, elseIfBlock, elseIfCondition);
            if (elseIfOrElseClause.Block is not null)
            {
                lastBlocks.Add(BuildBlock(elseIfOrElseClause.Block, elseIfBlock));
            }
        }

        var nextBlock = _graph.CreateNode();
        foreach (var lastBlock in lastBlocks)
        {
            if (lastBlock != _graph.ExitNode)
            {
                _graph.AddEdge(lastBlock, nextBlock);
            }
        }

        return nextBlock;
    }

    private CfgNode BuildWhile(LuaWhileStatSyntax whileStat, CfgNode sourceBlock)
    {
        var condition = whileStat.Condition;
        var loopBlock = _graph.CreateNode(CfgNodeKind.Loop);
        _graph.AddEdge(sourceBlock, loopBlock, condition);
        var bodyBlock = _graph.CreateNode();
        _graph.AddEdge(loopBlock, bodyBlock);

        var nextBlock = _graph.CreateNode();
        _loopExits.Add(loopBlock, nextBlock);
        if (whileStat.Block is not null)
        {
            var lastBlock = BuildBlock(whileStat.Block, bodyBlock);
            _graph.AddEdge(lastBlock, loopBlock);
            _graph.AddEdge(lastBlock, nextBlock);
        }

        return nextBlock;
    }

    private CfgNode BuildRepeat(LuaRepeatStatSyntax repeatStat, CfgNode sourceBlock)
    {
        var loopBlock = _graph.CreateNode(CfgNodeKind.Loop);
        _graph.AddEdge(sourceBlock, loopBlock);
        var bodyBlock = _graph.CreateNode();
        _graph.AddEdge(loopBlock, bodyBlock);

        var nextBlock = _graph.CreateNode();
        _loopExits.Add(loopBlock, nextBlock);
        if (repeatStat.Block is not null)
        {
            var lastBlock = BuildBlock(repeatStat.Block, bodyBlock);
            _graph.AddEdge(lastBlock, loopBlock, repeatStat.Condition);
            _graph.AddEdge(lastBlock, nextBlock);
        }

        return nextBlock;
    }

    private CfgNode BuildForStat(LuaForStatSyntax forStat, CfgNode sourceBlock)
    {
        var loopBlock = _graph.CreateNode(CfgNodeKind.Loop);
        _graph.AddEdge(sourceBlock, loopBlock);
        var bodyBlock = _graph.CreateNode();
        _graph.AddEdge(loopBlock, bodyBlock);

        var nextBlock = _graph.CreateNode();
        _loopExits.Add(loopBlock, nextBlock);
        if (forStat.Block is not null)
        {
            var lastBlock = BuildBlock(forStat.Block, bodyBlock);
            _graph.AddEdge(lastBlock, loopBlock);
            _graph.AddEdge(lastBlock, nextBlock);
        }

        return nextBlock;
    }

    private CfgNode BuildForRangeStat(LuaForRangeStatSyntax forRangeStat, CfgNode sourceBlock)
    {
        var loopBlock = _graph.CreateNode(CfgNodeKind.Loop);
        _graph.AddEdge(sourceBlock, loopBlock);
        var bodyBlock = _graph.CreateNode();
        _graph.AddEdge(loopBlock, bodyBlock);

        var nextBlock = _graph.CreateNode();
        _loopExits.Add(loopBlock, nextBlock);
        if (forRangeStat.Block is not null)
        {
            var lastBlock = BuildBlock(forRangeStat.Block, bodyBlock);
            _graph.AddEdge(lastBlock, loopBlock);
            _graph.AddEdge(lastBlock, nextBlock);
        }

        return nextBlock;
    }

    private CfgNode BuildReturn(LuaReturnStatSyntax returnStat, CfgNode sourceBlock)
    {
        sourceBlock.Statements.Add(returnStat);
        _graph.AddEdge(sourceBlock, _graph.ExitNode);
        return _graph.ExitNode;
    }

    private CfgNode BuildBreak(LuaBreakStatSyntax breakStat, CfgNode sourceBlock)
    {
        var loop = sourceBlock;
        while (loop is not null && loop.Kind != CfgNodeKind.Loop)
        {
            loop = loop.Incomings?.FirstOrDefault()?.Source;
        }

        var nextBlock = _graph.CreateNode();
        if (loop is null)
        {
            breakStat.PushDiagnostic(DiagnosticSeverity.Error, "Break statement outside of loop");
            return nextBlock;
        }

        if (_loopExits.TryGetValue(loop, out var exit))
        {
            _graph.AddEdge(sourceBlock, exit);
        }

        return nextBlock;
    }

    private CfgNode? FindScopeStart(CfgNode? node)
    {
        while (node is not null && !_scopeLabels.ContainsKey(node))
        {
            node = node.Incomings?.FirstOrDefault()?.Source;
        }

        return node;
    }

    private void RecordScope(CfgNode scopeNode)
    {
        if (!_scopeLabels.ContainsKey(scopeNode))
        {
            _scopeLabels[scopeNode] = new();
        }
    }

    private CfgNode BuildLabel(LuaLabelStatSyntax labelStat, CfgNode sourceBlock)
    {
        var labelBlock = _graph.CreateNode(CfgNodeKind.Label);
        _graph.AddEdge(sourceBlock, labelBlock);
        if (labelStat.Name is not null)
        {
            var scopeNode = FindScopeStart(sourceBlock);
            if (scopeNode is not null)
            {
                if (_scopeLabels.TryGetValue(scopeNode, out var labels))
                {
                    labels[labelStat.Name.RepresentText] = labelBlock;
                }
            }
        }

        return labelBlock;
    }

    private CfgNode BuildGoto(LuaGotoStatSyntax gotoStat, CfgNode sourceBlock)
    {
        var gotoNode = new GotoNode(sourceBlock, gotoStat);
        _gotoNodes.Add(gotoNode);
        return _graph.CreateNode();
    }
}
