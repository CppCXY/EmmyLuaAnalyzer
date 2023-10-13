using LuaLanguageServer.CodeAnalysis.Compilation.Infer;
using LuaLanguageServer.CodeAnalysis.Syntax.Location;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Symbol.Type;

public class TypeSymbol : ILuaTypeSymbol
{
    public TypeSymbol(TypeKind kind)
    {
        TypeKind = kind;
    }

    public bool Equals(ILuaSymbol? other)
    {
        return other is TypeSymbol symbol &&
               TypeKind == symbol.TypeKind;
    }

    public ILuaSymbol? ContainingSymbol { get; }

    public ILuaTypeSymbol ContainingType => throw new NotImplementedException();

    public SymbolKind Kind => SymbolKind.Type;

    public IEnumerable<LuaLocation> Locations { get; }

    public string DisplayName { get; }

    public virtual bool SubTypeOf(ILuaTypeSymbol other, SearchContext context)
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

    public ILuaNamedTypeSymbol? BaseType => null;

    public IEnumerable<ILuaNamedTypeSymbol> Interfaces => Enumerable.Empty<ILuaNamedTypeSymbol>();

    public IEnumerable<ILuaNamedTypeSymbol> AllInterface { get; }
}
