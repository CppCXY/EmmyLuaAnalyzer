using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;

namespace EmmyLua.CodeAnalysis.Compilation.Semantic.Reference;

public record LuaReference(LuaLocation Location, LuaSyntaxElement Element);
