namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public class Array : LuaType
{
    public ILuaType BaseSymbol { get; }

    public Array(ILuaType baseSymbol) : base(TypeKind.Array)
    {
        BaseSymbol = baseSymbol;
    }
}
