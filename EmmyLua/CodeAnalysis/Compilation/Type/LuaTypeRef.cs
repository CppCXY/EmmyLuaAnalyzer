using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Syntax.Node;

namespace EmmyLua.CodeAnalysis.Compilation.Type;

public class LuaTypeRef(LuaSyntaxElement element) : LuaType(TypeKind.TypeRef)
{
    public virtual ILuaType GetType(SearchContext context)
    {
        return context.Infer(element);
    }

    protected override bool OnSubTypeOf(ILuaType other, SearchContext context)
    {
        return GetType(context).SubTypeOf(other, context);
    }

    public override string ToDisplayString(SearchContext context)
    {
        return GetType(context).ToDisplayString(context);
    }

    protected override ILuaType OnSubstitute(SearchContext context)
    {
        return GetType(context);
    }
}
