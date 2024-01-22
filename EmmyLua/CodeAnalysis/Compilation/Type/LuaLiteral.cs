using EmmyLua.CodeAnalysis.Compilation.Analyzer.Infer;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Type;

public class LuaLiteral(LuaSyntaxToken token) : LuaType(TypeKind.Literal)
{
    public bool IsInteger => token is LuaIntegerToken;

    public bool IsString => token is LuaStringToken;

    public long IntegerValue => (token as LuaIntegerToken)?.Value ?? 0;

    public string StringValue => (token as LuaStringToken)?.Value ?? "";

    public override IEnumerable<Symbol.Symbol> GetMembers(SearchContext context)
    {
        return Enumerable.Empty<Symbol.Symbol>();
    }

    public override string ToDisplayString(SearchContext context)
    {
        if (IsInteger)
        {
            return IntegerValue.ToString();
        }
        else if (IsString)
        {
            return $"'{StringValue}'";
        }

        return string.Empty;
    }

}
