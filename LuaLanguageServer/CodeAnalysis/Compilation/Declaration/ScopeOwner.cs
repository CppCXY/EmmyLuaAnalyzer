using System.Diagnostics;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Declaration;

public abstract record ScopeOwner
{
    public record Block(LuaBlockSyntax LuaBlock) : ScopeOwner;

    public record Func(LuaFuncStatSyntax LuaFuncStat) : ScopeOwner;

    public record ForRange(LuaForRangeStatSyntax LuaForRangeStat) : ScopeOwner;

    public record For(LuaForStatSyntax LuaFor) : ScopeOwner;

    public record Closure(LuaClosureExprSyntax LuaClosure) : ScopeOwner;

    public override int GetHashCode()
    {
        return this switch
        {
            Block block => block.LuaBlock.GetHashCode(),
            Func func => func.LuaFuncStat.GetHashCode(),
            ForRange forRange => forRange.LuaForRangeStat.GetHashCode(),
            For @for => @for.LuaFor.GetHashCode(),
            Closure closure => closure.LuaClosure.GetHashCode(),
            _ => throw new UnreachableException()
        };
    }
};
