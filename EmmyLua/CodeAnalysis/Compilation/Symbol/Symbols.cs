using EmmyLua.CodeAnalysis.Compilation.Type;


namespace EmmyLua.CodeAnalysis.Compilation.Symbol;

public class VirtualSymbol(string name, ILuaType? declarationType)
    : Symbol(name, 0, null, SymbolKind.Virtual, null, null, declarationType)
{
    public VirtualSymbol(ILuaType? declarationType)
        : this(string.Empty, declarationType)
    {
    }
}

public class AssignSymbol(
    string name,
    Symbol? prev)
    : Symbol(name, 0, null, SymbolKind.Assign, null, prev, null);
