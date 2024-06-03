using EmmyLua.CodeAnalysis.Syntax.Node;

namespace EmmyLua.CodeAnalysis.Compilation.Reference;

public record struct LuaReference(LuaElementPtr<LuaSyntaxElement> Ptr, ReferenceKind Kind);
