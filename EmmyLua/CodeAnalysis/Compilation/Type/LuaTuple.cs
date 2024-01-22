using EmmyLua.CodeAnalysis.Compilation.Analyzer.Infer;
using EmmyLua.CodeAnalysis.Compilation.Symbol;

namespace EmmyLua.CodeAnalysis.Compilation.Type;

public class LuaTuple(IEnumerable<ILuaType> types) : LuaType(TypeKind.Tuple)
{
    public List<Symbol.Symbol> Declarations => types.Select(it=>new VirtualSymbol(it)).Cast<Symbol.Symbol>().ToList();

    public override IEnumerable<Symbol.Symbol> GetMembers(SearchContext context)
    {
        return Declarations;
    }

    public override IEnumerable<Symbol.Symbol> IndexMember(long index, SearchContext context)
    {
        if (index < Declarations.Count)
        {
            yield return Declarations[(int)index];
        }
    }

    public override bool SubTypeOf(ILuaType other, SearchContext context)
    {
        var otherSubstitute = other.Substitute(context);
        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (otherSubstitute is LuaTuple tuple)
        {
            if (tuple.Declarations.Count != Declarations.Count)
            {
                return false;
            }

            for (var i = 0; i < Declarations.Count; i++)
            {
                var luaType = Declarations[i].Type;
                var type = tuple.Declarations[i].Type;
                if (type != null && luaType != null && !luaType.SubTypeOf(type, context))
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
        return $"({string.Join(", ", Declarations.Select(it => it.Type?.ToDisplayString(context)))})";
    }
}
