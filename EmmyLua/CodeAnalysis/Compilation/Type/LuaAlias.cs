using EmmyLua.CodeAnalysis.Compilation.Analyzer.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Analyzer.Infer;

namespace EmmyLua.CodeAnalysis.Compilation.Type;

public class LuaAlias(string name, ILuaType baseType) : LuaType(TypeKind.Alias), ILuaNamedType
{
    public string Name { get; } = name;

    public ILuaType BaseType { get; } = baseType;

    public override IEnumerable<Declaration> GetMembers(SearchContext context)
    {
        return BaseType.GetMembers(context);
    }

    public override IEnumerable<Declaration> IndexMember(string name, SearchContext context)
    {
        return BaseType.IndexMember(name, context);
    }

    public override IEnumerable<Declaration> IndexMember(long index, SearchContext context)
    {
        return BaseType.IndexMember(index, context);
    }

    public override IEnumerable<Declaration> IndexMember(ILuaType ty, SearchContext context)
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
}
