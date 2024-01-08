using EmmyLuaAnalyzer.CodeAnalysis.Compilation.Analyzer.Declaration;
using EmmyLuaAnalyzer.CodeAnalysis.Compilation.Analyzer.Infer;
using EmmyLuaAnalyzer.CodeAnalysis.Syntax.Node;

namespace EmmyLuaAnalyzer.CodeAnalysis.Compilation.Type;

public class LuaTypeRef(LuaSyntaxElement element) : LuaType(TypeKind.TypeRef)
{
    public ILuaType GetType(SearchContext context)
    {
        return context.Infer(element);
    }

    public override IEnumerable<Declaration> GetMembers(SearchContext context)
    {
        return GetType(context).GetMembers(context);
    }

    public override IEnumerable<Declaration> IndexMember(string name, SearchContext context)
    {
        return GetType(context).IndexMember(name, context);
    }

    public override IEnumerable<Declaration> IndexMember(long index, SearchContext context)
    {
        return GetType(context).IndexMember(index, context);
    }

    public override IEnumerable<Declaration> IndexMember(ILuaType ty, SearchContext context)
    {
        return GetType(context).IndexMember(ty, context);
    }

    public override bool SubTypeOf(ILuaType other, SearchContext context)
    {
        return GetType(context).SubTypeOf(other, context);
    }
}

