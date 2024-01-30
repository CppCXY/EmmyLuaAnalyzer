using EmmyLua.CodeAnalysis.Compilation.Infer;

namespace EmmyLua.CodeAnalysis.Compilation.Type;

public class LuaClass(string name) : LuaType(TypeKind.Class), IGenericBase
{
    public string Name { get; } = name;

    public IEnumerable<Symbol.Symbol> GetRawMembers(SearchContext context)
    {
        return context.FindMembers(Name);
    }

    public override IEnumerable<Symbol.Symbol> GetMembers(SearchContext context)
    {
        return GetRawMembers(context).Concat(GetSupers(context).SelectMany(t => t.GetMembers(context)));
    }

    public virtual IEnumerable<ILuaType> GetSupers(SearchContext context)
    {
        return context.FindSupers(Name);
    }

    public override IEnumerable<Symbol.Symbol> IndexMember(string name, SearchContext context)
    {
        return GetMembers(context).Where(it => string.Compare(it.Name, name, StringComparison.CurrentCulture) == 0);
    }

    public override IEnumerable<Symbol.Symbol> IndexMember(long index, SearchContext context)
    {
        var key = $"[{index}]";
        return GetMembers(context).Where(it => string.Compare(it.Name, key, StringComparison.CurrentCulture) == 0);
    }

    // public override IEnumerable<Declaration> IndexMember(ILuaType ty, SearchContext context)
    // {
    //     return GetMembers(context).Where(it => it.Name == ty.Name);
    // }

    public override bool SubTypeOf(ILuaType other, SearchContext context)
    {
        var otherSubstitute = other.Substitute(context);
        return ReferenceEquals(this, otherSubstitute) ||
               (otherSubstitute is LuaClass @class &&
                string.Equals(Name, @class.Name, StringComparison.CurrentCulture)) ||
               GetSupers(context).Any(it => it.SubTypeOf(otherSubstitute, context));
    }

    public override string ToDisplayString(SearchContext context)
    {
        return Name;
    }
}
