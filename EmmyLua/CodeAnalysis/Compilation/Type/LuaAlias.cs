using EmmyLua.CodeAnalysis.Compilation.Analyzer.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Analyzer.Infer;
using EmmyLua.CodeAnalysis.Compilation.Symbol;

namespace EmmyLua.CodeAnalysis.Compilation.Type;

public class LuaAlias(string name, ILuaType baseType) : LuaType(TypeKind.Alias), ILuaNamedType
{
    public string Name { get; } = name;

    public ILuaType BaseType { get; } = baseType;

    public override IEnumerable<Symbol.Symbol> GetMembers(SearchContext context)
    {
        return BaseType.GetMembers(context);
    }

    public override IEnumerable<Symbol.Symbol> IndexMember(string name, SearchContext context)
    {
        return BaseType.IndexMember(name, context);
    }

    public override IEnumerable<Symbol.Symbol> IndexMember(long index, SearchContext context)
    {
        return BaseType.IndexMember(index, context);
    }

    public override IEnumerable<Symbol.Symbol> IndexMember(ILuaType ty, SearchContext context)
    {
        return BaseType.IndexMember(ty, context);
    }

    public override bool SubTypeOf(ILuaType other, SearchContext context)
    {
        return BaseType.SubTypeOf(other, context);
    }

    protected override ILuaType OnSubstitute(SearchContext context)
    {
        return BaseType.Substitute(context);
    }

    public override string ToDisplayString(SearchContext context)
    {
        return Name;
    }
}
