using LuaLanguageServer.CodeAnalysis.Compilation.Infer;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public interface ILuaType
{
    public IEnumerable<ILuaType> GetMembers(SearchContext context);

    public IEnumerable<ILuaNamedType> GetNamedMembers(SearchContext context);

    public IEnumerable<ILuaNamedType> GetNamedMembers(string name, SearchContext context);

    public TypeKind Kind { get; }

    public ILuaNamedType? GetBaseType(SearchContext context);

    public IEnumerable<ILuaNamedType> GetInterfaces(SearchContext context);

    /// <summary>
    /// contains all interfaces
    /// </summary>
    public IEnumerable<ILuaNamedType> GetAllInterface(SearchContext context);
}
