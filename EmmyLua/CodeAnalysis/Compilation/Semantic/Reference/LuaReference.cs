using EmmyLua.CodeAnalysis.Common;
using EmmyLua.CodeAnalysis.Syntax.Node;

namespace EmmyLua.CodeAnalysis.Compilation.Semantic.Reference;

public record LuaReference(ILocation Location, LuaSyntaxElement Element);
