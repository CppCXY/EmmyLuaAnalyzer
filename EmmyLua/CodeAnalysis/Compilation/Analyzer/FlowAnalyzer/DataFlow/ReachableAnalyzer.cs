using EmmyLua.CodeAnalysis.Compilation.Analyzer.FlowAnalyzer.ControlFlow;
using EmmyLua.CodeAnalysis.Compile.Diagnostic;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Tree;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.FlowAnalyzer.DataFlow;

public class ReachableAnalyzer(LuaCompilation compilation) : FlowAnalyzerBase(compilation)
{
    public override void Analyze(ControlFlowGraph cfg, LuaSyntaxTree tree)
    {
        var reachable = new Dictionary<CfgNode, bool>();
        foreach (var block in cfg.Nodes)
        {
            reachable[block] = false;
        }

        reachable[cfg.EntryNode] = true;

        bool changed;
        do
        {
            changed = false;
            foreach (var block in cfg.Nodes)
            {
                if (reachable[block])
                {
                    foreach (var successor in block.Successors)
                    {
                        if (!reachable[successor])
                        {
                            reachable[successor] = true;
                            changed = true;
                        }
                    }
                }
            }
        } while (changed);

        foreach (var block in cfg.Nodes)
        {
            if (!reachable[block] && block.Statements.Count != 0)
            {
                var start = block.Statements.First().Range.StartOffset;
                var range = new SourceRange(start, block.Statements.Last().Range.EndOffset - start);
                tree.PushDiagnostic(new Diagnostic(
                    DiagnosticSeverity.Hint,
                    DiagnosticCode.UnreachableCode,
                    "Unreachable code",
                    range,
                    DiagnosticTag.Unnecessary
                ));
            }
        }
    }
}
