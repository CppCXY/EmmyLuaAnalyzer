using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

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

public class LabelSymbol(string name, LuaLabelStatSyntax labelStat)
    : Symbol(name, 0, labelStat, SymbolKind.Label, null, null, null)
{
    public LuaLabelStatSyntax LabelStat => labelStat;
}
