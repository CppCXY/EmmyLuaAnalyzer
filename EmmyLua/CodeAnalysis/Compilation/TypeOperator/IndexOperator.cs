using EmmyLua.CodeAnalysis.Compilation.Analyzer.Infer;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.TypeOperator;

public class IndexOperator(ILuaType key, ILuaType ret) : ILuaOperator
{
    ILuaType? GetMemberType(LuaExprSyntax expr, SearchContext context)
    {
        return key.AcceptExpr(expr, context) ? ret : null;
    }
}
