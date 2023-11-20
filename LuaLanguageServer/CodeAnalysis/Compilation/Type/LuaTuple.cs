using LuaLanguageServer.CodeAnalysis.Compilation.Infer;
using LuaLanguageServer.CodeAnalysis.Compilation.Symbol;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public class LuaTuple : LuaType
{
    private readonly List<ILuaType> _types = new();

    public LuaTuple(IEnumerable<ILuaType> symbols) : base(TypeKind.Tuple)
    {
        _types.AddRange(symbols);
    }

    public override IEnumerable<ILuaSymbol> GetMembers(SearchContext context)
    {
        // return _types.Select((it, i) => new TupleMember((ulong)i, it, this));
        throw new NotImplementedException();
    }
}
