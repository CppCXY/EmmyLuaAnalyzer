using LuaLanguageServer.CodeAnalysis.Compilation.Infer;
using LuaLanguageServer.CodeAnalysis.Kind;
using LuaLanguageServer.CodeAnalysis.Syntax.Location;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;

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

    public virtual IEnumerable<ILuaType> IndexMember(IndexKey key, SearchContext context)
    {
        switch (key)
        {
            case IndexKey.String str:
            {
                return GetNamedMembers(str.Value, context);
            }
            default:
            {
                return Enumerable.Empty<ILuaType>();
            }
        }
    }

    public TypeKind Kind { get; }

    public ILuaNamedType? GetBaseType(SearchContext context) => null;

    public IEnumerable<Interface> GetInterfaces(SearchContext context) => Enumerable.Empty<Interface>();

    public IEnumerable<Interface> GetAllInterface(SearchContext context) => Enumerable.Empty<Interface>();
}
