using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Compilation.Symbol;

namespace EmmyLua.CodeAnalysis.Compilation.Type;

public class LuaTuple(IEnumerable<ILuaType> types) : LuaType(TypeKind.Tuple)
{
    public List<ILuaType> Types => types.ToList();

    public override bool SubTypeOf(ILuaType other, SearchContext context)
    {
        var otherSubstitute = other.Substitute(context);
        if (otherSubstitute is LuaTuple tuple)
        {
            if (tuple.Types.Count != Types.Count)
            {
                return false;
            }

            for (var i = 0; i < Types.Count; i++)
            {
                var luaType = Types[i];
                var type = tuple.Types[i];
                if (!luaType.SubTypeOf(type, context))
                {
                    return false;
                }
            }

            return true;
        }

        return false;
    }

    public override string ToDisplayString(SearchContext context)
    {
        return $"({string.Join(", ", types.Select(it => it.ToDisplayString(context)))})";
    }
}
