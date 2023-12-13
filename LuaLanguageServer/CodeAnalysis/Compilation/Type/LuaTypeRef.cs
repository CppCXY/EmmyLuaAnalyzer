using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Infer;
using LuaLanguageServer.CodeAnalysis.Compilation.Symbol;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public class LuaTypeRef(LuaSyntaxElement element) : LuaType(TypeKind.TypeRef)
{
    public ILuaType GetType(SearchContext context)
    {
        return context.Infer(element);
    }

    public override IEnumerable<ILuaSymbol> GetMembers(SearchContext context)
    {
        return GetType(context).GetMembers(context);
    }
}

