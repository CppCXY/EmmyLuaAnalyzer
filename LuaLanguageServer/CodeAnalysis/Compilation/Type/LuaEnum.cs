﻿using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Declaration;
using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Infer;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public class LuaEnum(string name, ILuaType luaType) : LuaType(TypeKind.Enum), ILuaNamedType
{
    public string Name { get; } = name;

    public ILuaType BaseType { get; } = luaType;

    public override IEnumerable<Declaration> GetMembers(SearchContext context)
    {
        return context.FindMembers(BaseType);
    }

    public override bool SubTypeOf(ILuaType other, SearchContext context)
    {
        return ReferenceEquals(this, other) ||
               other is LuaEnum @enum && string.Equals(Name, @enum.Name, StringComparison.CurrentCulture);
    }
}
