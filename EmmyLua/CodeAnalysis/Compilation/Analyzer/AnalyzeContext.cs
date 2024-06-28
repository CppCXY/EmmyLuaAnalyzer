using EmmyLua.CodeAnalysis.Compilation.Analyzer.FlowAnalyzer.ControlFlow;
using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer;

public class LuaExprRef(LuaExprSyntax expr, int retId = 0)
{
    public LuaExprSyntax Expr { get; } = expr;
    public int RetId { get; } = retId;
}

[Flags]
public enum ResolveState
{
    Resolved = 0,
    UnResolvedIndex = 0x001,
    UnResolvedType = 0x002,
    UnResolveReturn = 0x004,
    UnResolvedParameters = 0x008,
}

public record UnResolved(ResolveState ResolvedState);

public record UnResolvedDeclaration(
    LuaDeclaration LuaDeclaration,
    LuaExprRef? ExprRef,
    ResolveState ResolvedState)
    : UnResolved(ResolvedState);

public record UnResolvedMethod(LuaMethodType MethodType, LuaBlockSyntax Block, ResolveState ResolvedState)
    : UnResolved(ResolvedState);

public record UnResolvedSource(LuaDocumentId DocumentId, LuaBlockSyntax Block, ResolveState ResolvedState)
    : UnResolved(ResolvedState);

public record UnResolvedForRangeParameter(
    List<LuaDeclaration> Parameters,
    List<LuaExprSyntax> ExprList)
    : UnResolved(ResolveState.UnResolvedType);

public record UnResolvedClosureParameters(
    List<LuaDeclaration> Parameters,
    LuaCallExprSyntax CallExpr,
    int Index) : UnResolved(ResolveState.UnResolvedParameters);

public class AnalyzeContext(List<LuaDocument> documents)
{
    public List<LuaDocument> LuaDocuments { get; } = documents;

    public List<UnResolved> UnResolves { get; } = [];

    public Dictionary<SyntaxElementId, ControlFlowGraph> ControlFlowGraphs { get; } = new();

    public ControlFlowGraph? GetControlFlowGraph(LuaBlockSyntax block)
    {
        return ControlFlowGraphs.GetValueOrDefault(block.UniqueId);
    }
}
