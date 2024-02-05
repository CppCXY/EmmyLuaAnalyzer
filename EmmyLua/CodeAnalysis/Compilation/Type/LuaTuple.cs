using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Compilation.Symbol;

namespace EmmyLua.CodeAnalysis.Compilation.Type;

public class LuaTuple(IEnumerable<ILuaType> types) : LuaType(TypeKind.Tuple)
{
    public List<ILuaType> Types => types.ToList();

    protected override bool OnSubTypeOf(ILuaType other, SearchContext context)
    {
        if (other is LuaTuple tuple)
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
