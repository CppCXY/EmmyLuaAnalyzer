using EmmyLua.CodeAnalysis.Document;

namespace EmmyLua.CodeAnalysis.Compilation.Scope;

// TODO: record generic type name for LuaTypeScope
public record LuaTypeScope(string TypeName, List<SourceRange> Ranges);
