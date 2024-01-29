using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Type;

public class LuaExprRef(LuaExprSyntax expr, int retId = 0) : LuaTypeRef(expr)
{
    public LuaExprSyntax Expr => expr;

    public int RetId { get; } = retId;
}
