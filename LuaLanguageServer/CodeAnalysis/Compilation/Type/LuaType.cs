using LuaLanguageServer.CodeAnalysis.Compilation.Infer;
using LuaLanguageServer.CodeAnalysis.Syntax.Location;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public abstract class LuaType : ILuaType
{
    public LuaType(TypeKind kind)
    {
        Kind = kind;
    }

    public abstract IEnumerable<ILuaType> GetMembers(SearchContext context);

    public IEnumerable<ILuaNamedType> GetNamedMembers(SearchContext context)
    {
        return GetMembers(context).OfType<ILuaNamedType>();
    }

    public IEnumerable<ILuaNamedType> GetNamedMembers(string name, SearchContext context)
    {
        return GetNamedMembers(context).Where(x => x.Name == name);
    }

    public TypeKind Kind { get; }

    public ILuaNamedType? GetBaseType(SearchContext context) => null;

    public IEnumerable<ILuaNamedType> GetInterfaces(SearchContext context) => Enumerable.Empty<ILuaNamedType>();

    public IEnumerable<ILuaNamedType> GetAllInterface(SearchContext context) => Enumerable.Empty<ILuaNamedType>();
}
