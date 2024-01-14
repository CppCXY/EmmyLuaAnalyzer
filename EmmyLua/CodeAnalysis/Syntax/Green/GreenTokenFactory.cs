using EmmyLua.CodeAnalysis.Kind;

namespace EmmyLua.CodeAnalysis.Syntax.Green;

using System.Collections.Generic;
using System.Threading;

public class GreenTokenFactory
{
    private static readonly ThreadLocal<Dictionary<LuaTokenKind, Dictionary<int, GreenNode>>> ThreadCaches =
        new(() => new Dictionary<LuaTokenKind, Dictionary<int, GreenNode>>());

    public static GreenTokenFactory Instance { get; } = new();

    public GreenNode Create(LuaTokenKind kind, int length)
    {
        var caches = ThreadCaches.Value;

        if (caches!.TryGetValue(kind, out var cache))
        {
            if (cache.TryGetValue(length, out var node))
            {
                return node;
            }
        }
        else
        {
            cache = new Dictionary<int, GreenNode>();
            caches.Add(kind, cache);
        }

        var greenNode = new GreenNode(kind, length);
        cache.Add(length, greenNode);
        return greenNode;
    }
}
