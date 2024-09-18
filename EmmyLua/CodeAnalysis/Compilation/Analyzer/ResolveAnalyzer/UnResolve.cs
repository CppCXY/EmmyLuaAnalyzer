using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Compilation.Type.TypeInfo;
using EmmyLua.CodeAnalysis.Compilation.Type.Types;
using EmmyLua.CodeAnalysis.Compile.Kind;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.ResolveAnalyzer;

[Flags]
public enum ResolveState
{
    Resolved = 0,
    UnResolvedIndex = 0x001,
    UnResolvedType = 0x002,
    UnResolveReturn = 0x004,
    UnResolvedParameters = 0x008,
}

public record LuaExprRef(LuaExprSyntax Expr, int RetId = 0);

public record UnResolved(ResolveState ResolvedState);

public record UnResolvedSymbol(
    LuaSymbol LuaSymbol,
    LuaExprRef? ExprRef,
    ResolveState ResolvedState)
    : UnResolved(ResolvedState);

// public record UnResolvedDocSymbol(
//     LuaSymbol LuaSymbol,
//     LuaTypeId Id,
//     ResolveState ResolvedState)
//     : UnResolved(ResolvedState);

// public record UnResolvedDocOperator(
//     LuaTypeInfo TypeInfo,
//     LuaNamedType NamedType,
//     TypeOperatorKind Kind,
//     SyntaxElementId Id,
//     List<LuaTypeId> TypeIds,
//     ResolveState ResolvedState)
//     : UnResolved(ResolvedState);

public record UnResolvedMethod(SyntaxElementId Id, LuaBlockSyntax Block, ResolveState ResolvedState)
    : UnResolved(ResolvedState);

public record UnResolvedSource(LuaDocumentId DocumentId, LuaBlockSyntax Block, ResolveState ResolvedState)
    : UnResolved(ResolvedState);

public record UnResolvedForRangeParameter(
    List<LuaSymbol> Parameters,
    List<LuaExprSyntax> ExprList)
    : UnResolved(ResolveState.UnResolvedType);

public record UnResolvedClosureParameters(
    List<LuaSymbol> Parameters,
    LuaCallExprSyntax CallExpr,
    int Index) : UnResolved(ResolveState.UnResolvedParameters);
