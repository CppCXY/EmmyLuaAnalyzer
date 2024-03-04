using EmmyLua.CodeAnalysis.Compilation.Symbol;
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
public enum ResolveState: int
{
    Resolved = 0,
    UnResolvedIndex = 0x001,
    UnResolvedType = 0x002,
    UnResolveReturn = 0x004,
}

public class UnResolveDeclaration(Declaration declaration, LuaExprRef? exprRef, ResolveState state)
{
    public Declaration Declaration { get; } = declaration;
    public LuaExprRef? ExprRef { get; } = exprRef;

    public bool IsTypeDeclaration { get; set; } = false;

    public ResolveState ResolvedState { get; set; } = state;
}

public class AnalyzeContext(List<LuaDocument> documents)
{
    public List<LuaDocument> LuaDocuments { get; } = documents;

    public List<UnResolveDeclaration> UnResolveDeclarations { get; } = new();

    public Dictionary<LuaBlockSyntax, List<LuaExprSyntax>> MainBlockReturns { get; } = new();
}
