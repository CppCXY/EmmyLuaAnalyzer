using EmmyLua.CodeAnalysis.Diagnostics;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.FlowAnalyzer.ControlFlow;

public class CfgBuilder
{
    private ControlFlowGraph _graph = null!;

    private Dictionary<CfgNode, CfgNode> _loopExits = new();

    private Dictionary<string, CfgNode> _scopeLabels = new();

    private Stack<CfgNode> _loops = new();

    private class GotoNode(CfgNode source, LuaGotoStatSyntax gotoStat)
    {
        public LuaGotoStatSyntax GotoStat { get; } = gotoStat;
        public CfgNode Source { get; } = source;
    }

    private List<GotoNode> _gotoNodes = [];

    public ControlFlowGraph Build(LuaBlockSyntax block)
    {
        _graph = new ControlFlowGraph();
        var lastNode = BuildBlock(block, _graph.EntryNode);
        if (lastNode != _graph.ExitNode)
        {
            _graph.AddEdge(lastNode, _graph.ExitNode);
        }

        foreach (var gotoNode in _gotoNodes)
        {
            if (gotoNode.GotoStat.LabelName is { } label)
            {
                if (_scopeLabels.TryGetValue(label.RepresentText, out var target))
                {
                    _graph.AddEdge(gotoNode.Source, target);
                }
                else
                {
                    label.PushDiagnostic(DiagnosticSeverity.Error, $"No Visible label {label.RepresentText}");
                }
            }
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
                    if (!stat.Equals(block.StatList.Last()))
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
                    if (doStat.Block is not null)
                    {
                        currentBlock = BuildBlock(doStat.Block, currentBlock);
                    }

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
                    currentBlock.AddStatement(stat);
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

        var existElseClause = false;
        foreach (var elseIfOrElseClause in ifStat.IfClauseStatementList)
        {
            var elseIfCondition = elseIfOrElseClause.Condition;
            if (elseIfCondition is null)
            {
                existElseClause = true;
            }

            var elseIfBlock = _graph.CreateNode();
            _graph.AddEdge(sourceBlock, elseIfBlock, elseIfCondition);
            if (elseIfOrElseClause.Block is not null)
            {
                lastBlocks.Add(BuildBlock(elseIfOrElseClause.Block, elseIfBlock));
            }
        }

        var nextBlock = _graph.CreateNode();
        if (!existElseClause)
        {
            _graph.AddEdge(sourceBlock, nextBlock);
        }

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
        var loopNode = _graph.CreateNode(CfgNodeKind.Loop);
        _loops.Push(loopNode);
        _graph.AddEdge(sourceBlock, loopNode, condition);
        var bodyBlock = _graph.CreateNode();
        _graph.AddEdge(loopNode, bodyBlock);

        var nextBlock = _graph.CreateNode();
        _graph.AddEdge(sourceBlock, nextBlock);
        _loopExits.Add(loopNode, nextBlock);
        if (whileStat.Block is not null)
        {
            var lastBlock = BuildBlock(whileStat.Block, bodyBlock);
            _graph.AddEdge(lastBlock, loopNode);
            _graph.AddEdge(lastBlock, nextBlock);
        }

        _loops.Pop();
        return nextBlock;
    }

    private CfgNode BuildRepeat(LuaRepeatStatSyntax repeatStat, CfgNode sourceBlock)
    {
        var loopNode = _graph.CreateNode(CfgNodeKind.Loop);
        _loops.Push(loopNode);
        _graph.AddEdge(sourceBlock, loopNode);
        var bodyBlock = _graph.CreateNode();
        _graph.AddEdge(loopNode, bodyBlock);

        var nextBlock = _graph.CreateNode();
        _loopExits.Add(loopNode, nextBlock);
        if (repeatStat.Block is not null)
        {
            var lastBlock = BuildBlock(repeatStat.Block, bodyBlock);
            _graph.AddEdge(lastBlock, loopNode, repeatStat.Condition);
            _graph.AddEdge(lastBlock, nextBlock);
        }

        _loops.Pop();
        return nextBlock;
    }

    private CfgNode BuildForStat(LuaForStatSyntax forStat, CfgNode sourceBlock)
    {
        var loopNode = _graph.CreateNode(CfgNodeKind.Loop);
        _loops.Push(loopNode);
        _graph.AddEdge(sourceBlock, loopNode);
        var bodyBlock = _graph.CreateNode();
        _graph.AddEdge(loopNode, bodyBlock);

        var nextBlock = _graph.CreateNode();
        _loopExits.Add(loopNode, nextBlock);
        if (forStat.Block is not null)
        {
            var lastBlock = BuildBlock(forStat.Block, bodyBlock);
            _graph.AddEdge(lastBlock, loopNode);
            _graph.AddEdge(lastBlock, nextBlock);
        }

        _loops.Pop();
        return nextBlock;
    }

    private CfgNode BuildForRangeStat(LuaForRangeStatSyntax forRangeStat, CfgNode sourceBlock)
    {
        var loopNode = _graph.CreateNode(CfgNodeKind.Loop);
        _loops.Push(loopNode);
        _graph.AddEdge(sourceBlock, loopNode);
        var bodyBlock = _graph.CreateNode();
        _graph.AddEdge(loopNode, bodyBlock);

        var nextBlock = _graph.CreateNode();
        _graph.AddEdge(sourceBlock, nextBlock);
        _loopExits.Add(loopNode, nextBlock);
        if (forRangeStat.Block is not null)
        {
            var lastBlock = BuildBlock(forRangeStat.Block, bodyBlock);
            _graph.AddEdge(lastBlock, loopNode);
            _graph.AddEdge(lastBlock, nextBlock);
        }

        _loops.Pop();
        return nextBlock;
    }

    private CfgNode BuildReturn(LuaReturnStatSyntax returnStat, CfgNode sourceBlock)
    {
        sourceBlock.AddStatement(returnStat);
        _graph.AddEdge(sourceBlock, _graph.ExitNode);
        return _graph.ExitNode;
    }

    private CfgNode BuildBreak(LuaBreakStatSyntax breakStat, CfgNode sourceBlock)
    {
        CfgNode? loop = null;
        if (_loops.Count != 0)
        {
            loop = _loops.Peek();
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

    private CfgNode BuildLabel(LuaLabelStatSyntax labelStat, CfgNode sourceBlock)
    {
        var labelBlock = _graph.CreateNode(CfgNodeKind.Label);
        _graph.AddEdge(sourceBlock, labelBlock);
        if (labelStat.Name is {RepresentText: {} name })
        {
            if (!_scopeLabels.TryAdd(name, labelBlock))
            {
                // labelStat.PushDiagnostic(DiagnosticSeverity.Error, $"Label {name} already defined");
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
