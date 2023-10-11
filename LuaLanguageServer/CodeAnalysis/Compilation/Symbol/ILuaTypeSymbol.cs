namespace LuaLanguageServer.CodeAnalysis.Compilation.Symbol;

public interface ILuaTypeSymbol : ILuaSymbol
{
    public IEnumerable<ILuaSymbol> GetMembers();

    public IEnumerable<ILuaSymbol> GetMembers(string name);

    public IEnumerable<ILuaNamedTypeSymbol> GetTypeMembers();

    public IEnumerable<ILuaNamedTypeSymbol> GetTypeMembers(string name);

    public TypeKind TypeKind { get; }

    public ILuaNamedTypeSymbol? BaseType { get; }

    public IEnumerable<ILuaNamedTypeSymbol> Interfaces { get; }

    /// <summary>
    /// contains all interfaces
    /// </summary>
    public IEnumerable<ILuaNamedTypeSymbol> AllInterface { get; }

    public bool IsAnonymousType { get; }

    public bool IsTupleType { get; }
}
