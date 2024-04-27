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

public class UnResolved(ResolveState state)
{
    public ResolveState ResolvedState { get; set; } = state;
}

public class UnResolvedDeclaration(LuaDeclaration luaDeclaration, LuaExprRef? exprRef, ResolveState state)
    : UnResolved(state)
{
    public LuaDeclaration LuaDeclaration { get; } = luaDeclaration;

    public LuaExprRef? ExprRef { get; } = exprRef;

    public bool IsTypeDeclaration { get; set; } = false;
}

public class UnResolvedMethod(LuaMethodType methodType, LuaBlockSyntax block, ResolveState state) : UnResolved(state)
{
    public LuaMethodType MethodType { get; } = methodType;

    public LuaBlockSyntax Block { get; } = block;
}

public class UnResolvedSource(LuaDocumentId documentId, LuaBlockSyntax block, ResolveState state) : UnResolved(state)
{
    public LuaDocumentId DocumentId { get; } = documentId;

    public LuaBlockSyntax Block { get; } = block;
}

public class UnResolvedForRangeParameter(
    List<ParamDeclaration> parameterLuaDeclarations,
    List<LuaExprSyntax> exprList) : UnResolved(ResolveState.UnResolvedType)
{
    public List<ParamDeclaration> ParameterLuaDeclarations { get; } = parameterLuaDeclarations;

    public List<LuaExprSyntax> ExprList { get; } = exprList;
}

public class UnResolvedClosureParameters(
    List<ParamDeclaration> parameterLuaDeclarations,
    LuaCallExprSyntax callExprSyntax,
    int index) : UnResolved(ResolveState.UnResolvedParameters)
{
    public List<ParamDeclaration> ParameterLuaDeclarations { get; } = parameterLuaDeclarations;

    public LuaCallExprSyntax CallExprSyntax { get; } = callExprSyntax;

    public int Index { get; } = index;
}

public class AnalyzeContext(List<LuaDocument> documents)
{
    public List<LuaDocument> LuaDocuments { get; } = documents;

    public List<UnResolved> UnResolves { get; } = new();

    public Dictionary<long, ControlFlowGraph> ControlFlowGraphs { get; } = new();

    public ControlFlowGraph? GetControlFlowGraph(LuaBlockSyntax block)
    {
        return ControlFlowGraphs.GetValueOrDefault(block.UniqueId);
    }
}
