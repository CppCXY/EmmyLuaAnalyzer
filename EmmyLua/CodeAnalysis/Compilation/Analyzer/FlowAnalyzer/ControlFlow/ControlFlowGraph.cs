using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Syntax.Tree;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.FlowAnalyzer.ControlFlow;

// TODO: ControlFlowGraph
public class ControlFlowGraph
{
    private BasicBlock FirstBlock { get; } = new();

    public void Parse(LuaSyntaxTree tree)
    {
        BasicBlock previousBlock = FirstBlock;

        if (tree.SyntaxRoot.Block?.StatList is { } statList)
        {
            foreach (var stat in statList)
            {
                if (stat is LuaIfStatSyntax)
                {
                    var block = new BasicBlock();
                    previousBlock.Successors.Add(block);
                    previousBlock = block;
                }
                else
                {
                    previousBlock.Statements.Add(stat);
                }
            }
        }
    }

    private BasicBlock ParseSyntaxBlock(LuaBlockSyntax block)
    {
        var basicBlock = new BasicBlock();
        foreach (var stat in block.StatList)
        {
            basicBlock.Statements.Add(stat);
        }
        return basicBlock;
    }
}
