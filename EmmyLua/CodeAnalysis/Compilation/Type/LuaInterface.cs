using EmmyLua.CodeAnalysis.Compilation.Infer;

namespace EmmyLua.CodeAnalysis.Compilation.Type;

public class LuaInterface(string name) : LuaType(TypeKind.Interface), IGenericBase
{
    public string Name { get; } = name;

    protected override bool OnSubTypeOf(ILuaType other, SearchContext context)
    {
        if (other is ILuaNamedType namedType)
        {
            return string.Equals(Name, namedType.Name, StringComparison.CurrentCulture);
        }

        return context.FindSupers(Name).Any(it => it.SubTypeOf(other, context));
    }

    public override string ToDisplayString(SearchContext context)
    {
        return Name;
    }
}
