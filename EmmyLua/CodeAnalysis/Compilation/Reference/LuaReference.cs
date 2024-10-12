using EmmyLua.CodeAnalysis.Syntax.Node;

namespace EmmyLua.CodeAnalysis.Compilation.Reference;

public record struct LuaReference(LuaPtr<LuaSyntaxElement> Ptr, ReferenceKind Kind);
