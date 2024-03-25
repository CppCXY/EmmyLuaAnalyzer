using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;

namespace EmmyLua.CodeAnalysis.Compilation.Semantic.Reference;

public class LuaReference(LuaLocation location, LuaSyntaxElement element)
{
    public LuaLocation Location { get; } = location;

    public LuaSyntaxElement Element { get; } = element;
}
