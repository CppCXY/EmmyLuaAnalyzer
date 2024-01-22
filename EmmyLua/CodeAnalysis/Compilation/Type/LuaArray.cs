using EmmyLua.CodeAnalysis.Compilation.Analyzer.Infer;
using EmmyLua.CodeAnalysis.Compilation.Symbol;

namespace EmmyLua.CodeAnalysis.Compilation.Type;

public class LuaArray(ILuaType baseTy) : LuaType(TypeKind.Array)
{
    public ILuaType Base { get; } = baseTy;

    private VirtualSymbol BaseSymbol { get; } = new(baseTy);

    public override IEnumerable<Symbol.Symbol> GetMembers(SearchContext context) => Enumerable.Empty<Symbol.Symbol>();

    public override IEnumerable<Symbol.Symbol> IndexMember(long index, SearchContext context)
    {
        yield return BaseSymbol;
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

    public override string ToDisplayString(SearchContext context)
    {
        return $"{Base.ToDisplayString(context)}[]";
    }
}
