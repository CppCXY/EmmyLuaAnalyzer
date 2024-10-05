using System.Collections;
using EmmyLua.CodeAnalysis.Compilation.Search;
using EmmyLua.CodeAnalysis.Compilation.Type.Visitor;

namespace EmmyLua.CodeAnalysis.Compilation.Type.Types;

/// <summary>
/// LuaType被设计为树的形式, 非常近似于语法树本身
/// </summary>
public abstract class LuaType
{
    public bool IsSubTypeOf(LuaType? other, SearchContext context)
    {
        return other is not null && context.IsSubTypeOf(this, other);
    }

    public bool IsSameType(LuaType? other, SearchContext context)
    {
        return other is not null && context.IsSameType(this, other);
    }

    public abstract IEnumerable<LuaType> ChildrenTypes { get; }

    public abstract IEnumerable<LuaType> DescendantTypes { get; }

    public void Visit(LuaTypeVisitor visitor)
    {
        visitor.Visit(this);
    }

    public override string ToString()
    {
        return "luaType";
    }
}
