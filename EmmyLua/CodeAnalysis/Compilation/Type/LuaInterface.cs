using EmmyLua.CodeAnalysis.Compilation.Infer;

namespace EmmyLua.CodeAnalysis.Compilation.Type;

public class LuaInterface(string name) : LuaType(TypeKind.Interface), IGenericBase
{
    public string Name { get; } = name;

    public override IEnumerable<Symbol.Symbol> GetMembers(SearchContext context)
    {
        return context.FindMembers(Name);
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

    public virtual IEnumerable<ILuaType> GetSupers(SearchContext context)
    {
        return context.FindSupers(Name);
    }

    public override bool SubTypeOf(ILuaType other, SearchContext context)
    {
        var otherSubstitute = other.Substitute(context);
        return ReferenceEquals(this, otherSubstitute) ||
               (otherSubstitute is LuaInterface @interface &&
                string.Equals(Name, @interface.Name, StringComparison.CurrentCulture)) ||
               GetSupers(context).Any(it => it.SubTypeOf(otherSubstitute, context));
    }

    public override string ToDisplayString(SearchContext context)
    {
        return Name;
    }
}
