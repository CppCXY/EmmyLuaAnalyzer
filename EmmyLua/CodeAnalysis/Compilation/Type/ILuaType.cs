using EmmyLua.CodeAnalysis.Compilation.Infer;

namespace EmmyLua.CodeAnalysis.Compilation.Type;

public interface ILuaType
{
    public bool SubTypeOf(ILuaType other, SearchContext context);

    public ILuaType Substitute(SearchContext context);

    public ILuaType Substitute(SearchContext context, Dictionary<string, ILuaType> env);

    public TypeKind Kind { get; }

    public string ToDisplayString(SearchContext context);

    public bool IsNullable { get; }
}

public interface ILuaNamedType : ILuaType
{
    public string Name { get; }
}
