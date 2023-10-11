using LuaLanguageServer.CodeAnalysis.Compilation.Infer;
using LuaLanguageServer.CodeAnalysis.Syntax.Location;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Symbol.Type;

public class AliasSymbol : ILuaNamedTypeSymbol
{
    public AliasSymbol(string name)
    {
        Name = name;
    }

    public bool Equals(ILuaSymbol? other)
    {
        throw new NotImplementedException();
    }

    public ILuaSymbol? ContainingSymbol { get; }
    public SymbolKind Kind { get; }
    public string Name { get; }
    public IEnumerable<LuaLocation> Locations { get; }
    public bool SubTypeOf(ILuaSymbol symbol, SearchContext context)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<ILuaSymbol> GetMembers()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<ILuaSymbol> GetMembers(string name)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<ILuaNamedTypeSymbol> GetTypeMembers()
    {
        throw new NotImplementedException();
    }

    public IEnumerable<ILuaNamedTypeSymbol> GetTypeMembers(string name)
    {
        throw new NotImplementedException();
    }

    public TypeKind TypeKind { get; }
    public ILuaNamedTypeSymbol? BaseType { get; }
    public IEnumerable<ILuaNamedTypeSymbol> Interfaces { get; }
    public IEnumerable<ILuaNamedTypeSymbol> AllInterface { get; }
    public bool IsAnonymousType { get; }
    public bool IsTupleType { get; }
    public IEnumerable<string> MemberNames { get; }
    public string DisplayName { get; }
}
