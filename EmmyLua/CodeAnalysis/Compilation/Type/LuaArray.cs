﻿using EmmyLua.CodeAnalysis.Compilation.Analyzer.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Analyzer.Infer;

namespace EmmyLua.CodeAnalysis.Compilation.Type;

public class LuaArray(ILuaType baseTy) : LuaType(TypeKind.Array)
{
    public ILuaType Base { get; } = baseTy;

    private VirtualDeclaration BaseDeclaration { get; } = new(baseTy);

    public override IEnumerable<Declaration> GetMembers(SearchContext context) => Enumerable.Empty<Declaration>();

    public override IEnumerable<Declaration> IndexMember(long index, SearchContext context)
    {
        yield return BaseDeclaration;
    }

    public override bool SubTypeOf(ILuaType other, SearchContext context)
    {
        return ReferenceEquals(this, other) ||
               other is LuaArray array && Base.SubTypeOf(array.Base, context);
    }

    protected override ILuaType OnSubstitute(SearchContext context)
    {
        var baseTySubstitute = Base.Substitute(context);
        if (ReferenceEquals(baseTySubstitute, Base))
        {
            return this;
        }

        return new LuaArray(baseTySubstitute);
    }
}
