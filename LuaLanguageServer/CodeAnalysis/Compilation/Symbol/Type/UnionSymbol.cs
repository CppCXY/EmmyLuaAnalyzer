using LuaLanguageServer.CodeAnalysis.Compilation.Infer;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Symbol.Type;

public class UnionSymbol : TypeSymbol
{
    private HashSet<ILuaSymbol> _childSymbols = new();

    private static bool IsValid(ILuaSymbol symbol)
    {
        return symbol.Kind is SymbolKind.Unknown or SymbolKind.Void or SymbolKind.Nil;
    }

    public static ILuaSymbol Union(ILuaSymbol a, ILuaSymbol b)
    {
        if (IsValid(a))
        {
            return b;
        }
        else if (IsValid(b))
        {
            return a;
        }
        else if (a is UnionSymbol unionSymbol)
        {
            unionSymbol._childSymbols.Add(b);
            return unionSymbol;
        }
        else if (b is UnionSymbol unionSymbol2)
        {
            unionSymbol2._childSymbols.Add(a);
            return unionSymbol2;
        }
        else
        {
            var union = new UnionSymbol(a, b);
            return union._childSymbols.Count == 1 ? union._childSymbols.First() : union;
        }
    }

    public static void Process(ILuaSymbol symbol, Func<ILuaSymbol, bool> process)
    {
        if (symbol is UnionSymbol unionSymbol)
        {
            foreach (var childSymbol in unionSymbol._childSymbols)
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

    public static void Each(ILuaSymbol symbol, Action<ILuaSymbol> action)
    {
        Process(symbol, s =>
        {
            action(s);
            return true;
        });
    }

    public UnionSymbol(ILuaSymbol a, ILuaSymbol b) : base(SymbolKind.Union)
    {
        _childSymbols.Add(a);
        _childSymbols.Add(b);
    }

    public ILuaSymbol Union(ILuaSymbol symbol)
    {
        if (symbol is UnionSymbol unionSymbol)
        {
            foreach (var childSymbol in unionSymbol._childSymbols)
            {
                _childSymbols.Add(childSymbol);
            }
        }
        else
        {
            _childSymbols.Add(symbol);
        }

        return this;
    }

    public override bool SubTypeOf(ILuaSymbol symbol, SearchContext context)
    {
        return base.SubTypeOf(symbol, context)
               || _childSymbols.Any(childSymbol => childSymbol.SubTypeOf(symbol, context));
    }

    public override IEnumerable<ILuaSymbol> Members
    {
        get
        {
            foreach (var childSymbol in _childSymbols)
            {
                foreach (var member in childSymbol.Members)
                {
                    yield return member;
                }
            }
        }
    }
}

public static class UnionSymbolExtensions
{
    public static ILuaSymbol Union(this ILuaSymbol a, ILuaSymbol b)
    {
        return UnionSymbol.Union(a, b);
    }
}
