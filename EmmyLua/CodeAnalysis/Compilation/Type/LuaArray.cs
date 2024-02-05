using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Compilation.Symbol;

namespace EmmyLua.CodeAnalysis.Compilation.Type;

public class LuaArray(ILuaType baseTy) : LuaType(TypeKind.Array)
{
    public ILuaType Base { get; } = baseTy;

    protected override bool OnSubTypeOf(ILuaType other, SearchContext context)
    {
        return other is LuaArray array && Base.SubTypeOf(array.Base, context);
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

    public override string ToDisplayString(SearchContext context)
    {
        return $"{Base.ToDisplayString(context)}[]";
    }
}
