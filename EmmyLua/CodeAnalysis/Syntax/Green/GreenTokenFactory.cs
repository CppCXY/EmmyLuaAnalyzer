using EmmyLua.CodeAnalysis.Kind;

namespace EmmyLua.CodeAnalysis.Syntax.Green;

using System.Collections.Generic;


public class GreenTokenFactory
{
    private struct Key(LuaTokenKind kind, int length)
    {
        public LuaTokenKind Kind { get; } = kind;
        public int Length { get; } = length;

        public override int GetHashCode()
        {
            return HashCode.Combine(Kind, Length);
        }
    }

    private static readonly Dictionary<Key, GreenNode> Caches = new();

    public GreenNode Create(LuaTokenKind kind, int length)
    {
        var key = new Key(kind, length);
        if (Caches.TryGetValue(key, out var node))
        {
            return node;
        }
        var green = new GreenNode(kind, length);
        Caches.Add(key, green);
        return green;
    }
}
