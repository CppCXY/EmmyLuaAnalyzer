using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Type;

namespace EmmyLua.CodeAnalysis.Compilation.Symbol;

// TODO
public class LuaSymbol(LuaSyntaxElement element)
{
    public string Name { get; } = string.Empty;

    public LuaType Type => Builtin.Unknown;
}
