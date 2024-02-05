using EmmyLua.CodeAnalysis.Compilation.Infer;

namespace EmmyLua.CodeAnalysis.Compilation.Type;

public abstract class LuaType(TypeKind kind) : ILuaType
{
    public virtual bool SubTypeOf(ILuaType other, SearchContext context)
    {
        var otherSubstitute = other.Substitute(context);
        if (otherSubstitute is Unknown) return true;
        if (!context.TryAddSubType(this)) return false;

        var result = OnSubTypeOf(otherSubstitute, context);

        context.RemoveSubType(this);
        return result;
    }

    protected virtual bool OnSubTypeOf(ILuaType other, SearchContext context)
    {
        return false;
    }

    public ILuaType Substitute(SearchContext context)
    {
        if (!context.TryAddSubstitute(this)) return this;

        var ty = OnSubstitute(context);
        context.RemoveSubstitute(this);
        return ty;
    }

    public ILuaType Substitute(SearchContext context, Dictionary<string, ILuaType> env)
    {
        if (!context.TryAddSubstitute(this)) return this;
        context.EnvSearcher.PushEnv(env);
        var ty = OnSubstitute(context);
        context.RemoveSubstitute(this);
        context.EnvSearcher.PopEnv();
        return ty;
    }

    protected virtual ILuaType OnSubstitute(SearchContext context)
    {
        return this;
    }

    public TypeKind Kind { get; } = kind;

    public virtual string ToDisplayString(SearchContext context)
    {
        return string.Empty;
    }

    public virtual bool IsNullable => false;
}
