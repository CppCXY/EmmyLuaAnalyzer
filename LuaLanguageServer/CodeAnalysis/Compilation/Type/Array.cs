using LuaLanguageServer.CodeAnalysis.Compilation.Infer;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public class Array : LuaType
{
    public ILuaType BaseSymbol { get; }

    public Array(ILuaType baseSymbol) : base(TypeKind.Array)
    {
        BaseSymbol = baseSymbol;
    }

    public override IEnumerable<ILuaType> GetMembers(SearchContext context) => Enumerable.Empty<ILuaType>();
}
