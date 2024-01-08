using EmmyLuaAnalyzer.CodeAnalysis.Compilation.Analyzer.Infer;
using EmmyLuaAnalyzer.CodeAnalysis.Compilation.Type;
using EmmyLuaAnalyzer.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLuaAnalyzer.CodeAnalysis.Syntax.Node;

namespace EmmyLuaAnalyzer.CodeAnalysis.Compilation.TypeOperator;

public class IndexOperator(ILuaType key, ILuaType ret) : ILuaOperator
{
    ILuaType? GetMemberType(LuaExprSyntax expr, SearchContext context)
    {
        return key.AcceptExpr(expr, context) ? ret : null;
    }
}
