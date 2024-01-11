using EmmyLua.CodeAnalysis.Compilation.Analyzer.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Analyzer.Infer;

namespace EmmyLua.CodeAnalysis.Compilation.Type;

public class LuaTuple(IEnumerable<ILuaType> types) : LuaType(TypeKind.Tuple)
{
    public List<Declaration> Declarations => types.Select(it=>new VirtualDeclaration(it)).Cast<Declaration>().ToList();

    public override IEnumerable<Declaration> GetMembers(SearchContext context)
    {
        return Declarations;
    }

    public override IEnumerable<Declaration> IndexMember(long index, SearchContext context)
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
}
