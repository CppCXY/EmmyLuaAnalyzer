using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Syntax.Node;

namespace EmmyLua.CodeAnalysis.Compilation.Type;

public class LuaTypeRef(LuaSyntaxElement element) : LuaType(TypeKind.TypeRef)
{
    public ILuaType GetType(SearchContext context)
    {
        return context.Infer(element);
    }

    public override IEnumerable<Symbol.Symbol> GetMembers(SearchContext context)
    {
        return GetType(context).GetMembers(context);
    }

    public override IEnumerable<Symbol.Symbol> IndexMember(string name, SearchContext context)
    {
        return GetType(context).IndexMember(name, context);
    }

    public override IEnumerable<Symbol.Symbol> IndexMember(long index, SearchContext context)
    {
        return GetType(context).IndexMember(index, context);
    }

    public override IEnumerable<Symbol.Symbol> IndexMember(ILuaType ty, SearchContext context)
    {
        return GetType(context).IndexMember(ty, context);
    }

    public override bool SubTypeOf(ILuaType other, SearchContext context)
    {
        return GetType(context).SubTypeOf(other, context);
    }

    public override string ToDisplayString(SearchContext context)
    {
        return GetType(context).ToDisplayString(context);
    }
}

