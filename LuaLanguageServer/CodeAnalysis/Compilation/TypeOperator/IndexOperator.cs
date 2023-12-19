using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Infer;
using LuaLanguageServer.CodeAnalysis.Compilation.Type;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace LuaLanguageServer.CodeAnalysis.Compilation.TypeOperator;

public class IndexOperator(ILuaType key, ILuaType ret) : ILuaOperator
{
    ILuaType? GetMemberType(LuaExprSyntax expr, SearchContext context)
    {
        return key.AcceptExpr(expr, context) ? ret : null;
    }
}
