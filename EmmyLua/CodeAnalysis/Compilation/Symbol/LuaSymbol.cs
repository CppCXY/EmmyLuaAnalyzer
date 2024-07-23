using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Syntax.Node;

namespace EmmyLua.CodeAnalysis.Compilation.Symbol;

// TODO
public class LuaSymbol(LuaSyntaxElement element)
{
    public string Name { get; } = string.Empty;

    public LuaType Type => Builtin.Unknown;
}
