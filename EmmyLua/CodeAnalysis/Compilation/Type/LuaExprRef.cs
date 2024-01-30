using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Type;

public class LuaExprRef(LuaExprSyntax expr, int retId = 0) : LuaTypeRef(expr)
{
    public LuaExprSyntax Expr => expr;

    public int RetId { get; } = retId;

    public override ILuaType GetType(SearchContext context)
    {
        var ty = base.GetType(context);
        if (ty is LuaMultiRetType multiRetType)
        {
            return multiRetType.GetRetType(RetId) ?? context.Compilation.Builtin.Unknown;
        }

        return ty;
    }
}
