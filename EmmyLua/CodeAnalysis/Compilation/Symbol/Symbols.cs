using EmmyLua.CodeAnalysis.Compilation.Type;

namespace EmmyLua.CodeAnalysis.Compilation.Symbol;

public class AssignSymbol(
    string name,
    int position,
    Symbol? prev,
    LuaExprRef? exprRef
)
    : Symbol(name, position, null, SymbolKind.Assign, null, prev, null)
{
    public LuaExprRef? ExprRef => exprRef;
}
