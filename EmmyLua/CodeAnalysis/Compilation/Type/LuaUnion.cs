﻿using EmmyLua.CodeAnalysis.Compilation.Analyzer.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Analyzer.Infer;

namespace EmmyLua.CodeAnalysis.Compilation.Type;

public class LuaUnion : LuaType
{
    private HashSet<ILuaType> _childTypes = new();

    private static bool IsValid(ILuaType symbol)
    {
        return symbol.Kind is TypeKind.Unknown;
    }

    public static ILuaType UnionType(ILuaType a, ILuaType b)
    {
        if (IsValid(a))
        {
            return b;
        }
        else if (IsValid(b))
        {
            return a;
        }
        else if (a is LuaUnion unionSymbol)
        {
            unionSymbol._childTypes.Add(b);
            return unionSymbol;
        }
        else if (b is LuaUnion unionSymbol2)
        {
            unionSymbol2._childTypes.Add(a);
            return unionSymbol2;
        }
        else
        {
            var union = new LuaUnion(a, b);
            return union._childTypes.Count == 1 ? union._childTypes.First() : union;
        }
    }

    public static void Process(ILuaType symbol, Func<ILuaType, bool> process)
    {
        if (symbol is LuaUnion unionSymbol)
        {
            foreach (var childSymbol in unionSymbol._childTypes)
            {
                if (!process(childSymbol))
                {
                    break;
                }
            }
        }
        else
        {
            process(symbol);
        }
    }

    public static void Each(ILuaType symbol, Action<ILuaType> action)
    {
        Process(symbol, s =>
        {
            action(s);
            return true;
        });
    }

    public LuaUnion(ILuaType a, ILuaType b) : base(TypeKind.Union)
    {
        _childTypes.Add(a);
        _childTypes.Add(b);
    }

    public ILuaType UnionType(ILuaType symbol)
    {
        if (symbol is LuaUnion unionSymbol)
        {
            foreach (var childSymbol in unionSymbol._childTypes)
            {
                _childTypes.Add(childSymbol);
            }
        }
        else
        {
            _childTypes.Add(symbol);
        }

        return this;
    }

    public override IEnumerable<Declaration> GetMembers(SearchContext context)
    {
        return _childTypes.SelectMany(it => it.GetMembers(context));
    }

    public override IEnumerable<Declaration> IndexMember(ILuaType ty, SearchContext context)
    {
        return _childTypes.SelectMany(it => it.IndexMember(ty, context));
    }

    public override IEnumerable<Declaration> IndexMember(string name, SearchContext context)
    {
        return _childTypes.SelectMany(it => it.IndexMember(name, context));
    }

    public override IEnumerable<Declaration> IndexMember(long index, SearchContext context)
    {
        return _childTypes.SelectMany(it => it.IndexMember(index, context));
    }

    public override bool SubTypeOf(ILuaType other, SearchContext context)
    {
         var otherSubstitute = other.Substitute(context);
         if (otherSubstitute is LuaUnion otherUnion)
         {
             return _childTypes.All(it => otherUnion._childTypes.Any(it2 => it.SubTypeOf(it2, context)));
         }

         return false;
    }
}

public static class UnionTypeExtensions
{
    public static ILuaType Union(this ILuaType a, ILuaType b)
    {
        return LuaUnion.UnionType(a, b);
    }
}
