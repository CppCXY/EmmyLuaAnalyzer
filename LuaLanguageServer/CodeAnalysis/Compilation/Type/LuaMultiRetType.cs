using LuaLanguageServer.CodeAnalysis.Compilation.Infer;
using LuaLanguageServer.CodeAnalysis.Compilation.Symbol;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public class LuaMultiRetType : LuaType
{
    private readonly List<ILuaType> _types = new();

    public LuaMultiRetType(List<ILuaType> rets) : base(TypeKind.MultiRet)
    {
        _types.AddRange(rets);
    }

    public override IEnumerable<ILuaSymbol> GetMembers(SearchContext context)
    {
        return Enumerable.Empty<ILuaSymbol>();
    }

    public ILuaType? GetRetType(int index)
    {
        return index < _types.Count ? _types[index] : null;
    }
}
