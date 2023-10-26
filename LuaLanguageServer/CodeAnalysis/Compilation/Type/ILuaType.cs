using LuaLanguageServer.CodeAnalysis.Compilation.Infer;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public interface ILuaType
{
    public IEnumerable<LuaTypeMember> GetMembers(SearchContext context);

    public IEnumerable<LuaTypeMember> IndexMember(IndexKey key, SearchContext context);

    public TypeKind Kind { get; }

    public ILuaNamedType? GetBaseType(SearchContext context);

    public IEnumerable<Interface> GetInterfaces(SearchContext context);

    /// <summary>
    /// contains all interfaces
    /// </summary>
    public IEnumerable<Interface> GetAllInterface(SearchContext context);
}

public abstract record IndexKey
{
    public record Integer(long Value) : IndexKey;

    public record String(string Value) : IndexKey;

    public record Ty(ILuaType Value) : IndexKey;
}

public interface ILuaNamedType : ILuaType
{
    public string Name { get; }
}


