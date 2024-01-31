using EmmyLua.CodeAnalysis.Compilation.Infer;

namespace EmmyLua.CodeAnalysis.Compilation.Type;

public class LuaInterface(string name) : LuaType(TypeKind.Interface), IGenericBase
{
    public string Name { get; } = name;

    public override bool SubTypeOf(ILuaType other, SearchContext context)
    {
        var otherSubstitute = other.Substitute(context);
        // TODO 匿名表检查
        if (otherSubstitute is ILuaNamedType namedType)
        {
            return string.Equals(Name, namedType.Name, StringComparison.CurrentCulture);
        }

        return context.FindSupers(Name).Any(it => it.SubTypeOf(otherSubstitute, context));
    }

    public override string ToDisplayString(SearchContext context)
    {
        return Name;
    }
}
