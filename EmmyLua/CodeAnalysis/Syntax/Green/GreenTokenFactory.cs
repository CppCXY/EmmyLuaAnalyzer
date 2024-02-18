using EmmyLua.CodeAnalysis.Kind;

namespace EmmyLua.CodeAnalysis.Syntax.Green;

using System.Collections.Generic;

public class GreenTokenFactory
{
    private static readonly Dictionary<LuaTokenKind, Dictionary<int, GreenNode>> Caches = new();

    public GreenNode Create(LuaTokenKind kind, int length)
    {
        if (Caches.TryGetValue(kind, out var cache))
        {
            if (cache.TryGetValue(length, out var node))
            {
                return node;
            }
        }
        else
        {
            cache = new Dictionary<int, GreenNode>();
            Caches.Add(kind, cache);
        }

        var greenNode = new GreenNode(kind, length);
        cache.Add(length, greenNode);
        return greenNode;
    }
}
