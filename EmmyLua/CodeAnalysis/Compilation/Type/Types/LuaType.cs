using EmmyLua.CodeAnalysis.Compilation.Search;

namespace EmmyLua.CodeAnalysis.Compilation.Type.Types;

public abstract class LuaType(bool nullable = false)
{
    public bool IsNullable { get; } = nullable;

    public virtual bool IsUnknown { get; } = false;

    public bool IsSubTypeOf(LuaType? other, SearchContext context)
    {
        return other is not null && context.IsSubTypeOf(this, other);
    }

    public bool IsSameType(LuaType? other, SearchContext context)
    {
        return other is not null && context.IsSameType(this, other);
    }

    public virtual LuaType Instantiate(TypeSubstitution substitution)
    {
        return this;
    }
}
