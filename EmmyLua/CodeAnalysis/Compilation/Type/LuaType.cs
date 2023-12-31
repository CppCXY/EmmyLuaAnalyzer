﻿using EmmyLua.CodeAnalysis.Compilation.Analyzer.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Analyzer.Infer;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;


namespace EmmyLua.CodeAnalysis.Compilation.Type;

public abstract class LuaType(TypeKind kind) : ILuaType
{
    public abstract IEnumerable<Declaration> GetMembers(SearchContext context);

    public virtual IEnumerable<Declaration> IndexMember(string name, SearchContext context) =>
        Enumerable.Empty<Declaration>();

    public virtual IEnumerable<Declaration> IndexMember(long index, SearchContext context) =>
        Enumerable.Empty<Declaration>();

    public virtual IEnumerable<Declaration> IndexMember(ILuaType ty, SearchContext context) =>
        Enumerable.Empty<Declaration>();

    public virtual bool SubTypeOf(ILuaType other, SearchContext context)
    {
        return ReferenceEquals(this, other);
    }

    public virtual bool AcceptExpr(LuaExprSyntax expr, SearchContext context)
    {
        var ty = context.Infer(expr);
        return SubTypeOf(ty, context);
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
}
