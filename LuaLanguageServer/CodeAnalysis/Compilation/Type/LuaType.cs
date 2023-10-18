using LuaLanguageServer.CodeAnalysis.Compilation.Infer;
using LuaLanguageServer.CodeAnalysis.Syntax.Location;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public class LuaType : ILuaType
{
    public LuaType(TypeKind kind)
    {
        Kind = kind;
    }

    public ILuaType ContainingType => throw new NotImplementedException();

    public IEnumerable<LuaLocation> Locations { get; }

    public string DisplayName { get; }

    public virtual bool SubTypeOf(ILuaType other, SearchContext context)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<ILuaType> GetMembers(SearchContext context)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<ILuaNamedType> GetMembers(string name)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<ILuaNamedType> GetTypeMembers(SearchContext context)
    {
        throw new NotImplementedException();
    }

    public IEnumerable<ILuaNamedType> GetTypeMembers(string name)
    {
        throw new NotImplementedException();
    }

    public TypeKind Kind { get; }

    public ILuaNamedType? BaseType => null;

    public IEnumerable<ILuaNamedType> Interfaces => Enumerable.Empty<ILuaNamedType>();

    public IEnumerable<ILuaNamedType> AllInterface { get; }
}
