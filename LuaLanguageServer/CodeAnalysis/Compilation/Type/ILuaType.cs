using LuaLanguageServer.CodeAnalysis.Compilation.Infer;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public interface ILuaType
{
    public IEnumerable<ILuaType> GetMembers(SearchContext context);

    public IEnumerable<ILuaNamedType> GetTypeMembers(SearchContext context);

    public IEnumerable<ILuaNamedType> GetTypeMembers(string name);

    public TypeKind Kind { get; }

    public ILuaNamedType? BaseType { get; }

    public IEnumerable<ILuaNamedType> Interfaces { get; }

    /// <summary>
    /// contains all interfaces
    /// </summary>
    public IEnumerable<ILuaNamedType> AllInterface { get; }
}
