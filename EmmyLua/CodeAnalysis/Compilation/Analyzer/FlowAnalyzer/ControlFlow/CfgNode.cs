using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.FlowAnalyzer.ControlFlow;

public class CfgNode(int index, CfgNodeKind kind)
{
    public CfgNodeKind Kind { get; } = kind;

    public SourceRange Range { get; set; }

    public int Index { get; } = index;

    public void AddRange(SourceRange range)
    {
        Range = Range.Length == 0 ? range : Range.Merge(range);
    }
}
