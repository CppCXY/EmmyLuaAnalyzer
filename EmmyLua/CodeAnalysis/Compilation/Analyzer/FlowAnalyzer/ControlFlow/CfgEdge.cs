namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.FlowAnalyzer.ControlFlow;

public readonly struct CfgEdge(int sourceIndex, int targetIndex)
{
    public int TargetIndex { get; } = targetIndex;

    public int SourceIndex { get; } = sourceIndex;
}
