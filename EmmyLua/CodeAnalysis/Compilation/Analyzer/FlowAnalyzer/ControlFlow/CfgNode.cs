using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.FlowAnalyzer.ControlFlow;

public class CfgNode(int index, CfgNodeKind kind)
{
    public CfgNodeKind Kind { get; } = kind;

    public List<LuaElementPtr<LuaStatSyntax>> Statements { get; } = new();

    // public SourceRange Range
    // {
    //     get
    //     {
    //         if (Statements.Count == 0)
    //         {
    //             return SourceRange.Empty;
    //         }
    //
    //         var start = Statements.First().Range.StartOffset;
    //         var end = Statements.Last().Range.EndOffset;
    //         return new SourceRange(start, end - start);
    //     }
    // }

    public int Index { get; } = index;

    public void AddStatement(LuaStatSyntax stat)
    {
        Statements.Add(new(stat));
    }
}
