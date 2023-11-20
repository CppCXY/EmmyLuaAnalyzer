using LuaLanguageServer.CodeAnalysis.Compilation.Infer;
using LuaLanguageServer.CodeAnalysis.Compilation.Type;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Symbol;

public abstract class LuaSymbol : ILuaSymbol
{
    public ILuaType ContainingType { get; }

    public LuaSymbol(ILuaType containingType)
    {
        ContainingType = containingType;
    }

    public abstract ILuaType GetType(SearchContext context);

    public abstract bool MatchKey(IndexKey key, SearchContext context);
}

