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

public class UnResolveDeclaration(Declaration declaration, LuaExprRef? exprRef)
{
    public Declaration Declaration { get; } = declaration;
    public LuaExprRef? ExprRef { get; } = exprRef;
}

public class AnalyzeContext(List<LuaDocument> documents)
{
    public List<LuaDocument> LuaDocuments { get; } = documents;

    public List<UnResolveDeclaration> UnResolveDeclarations { get; } = new();

    public Dictionary<LuaBlockSyntax, List<LuaExprSyntax>> MainBlockReturns { get; } = new();

    public Dictionary<LuaFuncBodySyntax, LuaMethodType> Methods { get; } = new();
}
