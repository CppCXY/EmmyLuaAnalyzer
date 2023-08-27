using LuaLanguageServer.LuaCore.Kind;

namespace LuaLanguageServer.LuaCore.Syntax.Green;

/// <summary>
/// 红绿树中的绿树节点
/// </summary>
public struct GreenNode
{
    public LuaSyntaxKind Kind { get; }

    public int SlotCount { get; }

    public int FullWidth { get; }

    public GreenNode(LuaSyntaxKind kind, int slotCount, int fullWidth)
    {
        Kind = kind;
        SlotCount = slotCount;
        FullWidth = fullWidth;
    }
}

public struct GreenToken
{
    public LuaTokenKind Kind { get; }

    public int FullWidth { get; }

    public GreenToken(LuaTokenKind kind, int fullWidth)
    {
        Kind = kind;
        FullWidth = fullWidth;
    }
}

