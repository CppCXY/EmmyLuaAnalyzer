using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer;

public class LuaExprRef(LuaExprSyntax expr, int retId = 0)
{
    public LuaExprSyntax Expr { get; } = expr;
    public int RetId { get; } = retId;
}

[Flags]
public enum ResolveState : int
{
    Resolved = 0,
    UnResolvedIndex = 0x001,
    UnResolvedType = 0x002,
    UnResolveReturn = 0x004,
}

public class UnResolved(ResolveState state)
{
    public ResolveState ResolvedState { get; set; } = state;
}

public class UnResolvedDeclaration(LuaDeclaration luaDeclaration, LuaExprRef? exprRef, ResolveState state) : UnResolved(state)
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

public class UnResolvedSource(DocumentId documentId, LuaBlockSyntax block, ResolveState state) : UnResolved(state)
{
    public DocumentId DocumentId { get; } = documentId;

    public LuaBlockSyntax Block { get; } = block;
}

public class AnalyzeContext(List<LuaDocument> documents)
{
    public List<LuaDocument> LuaDocuments { get; } = documents;

    public List<UnResolved> UnResolves { get; } = new();
}
