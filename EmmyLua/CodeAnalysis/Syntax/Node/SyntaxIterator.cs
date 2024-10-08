using EmmyLua.CodeAnalysis.Compile.Kind;
using EmmyLua.CodeAnalysis.Syntax.Tree;

namespace EmmyLua.CodeAnalysis.Syntax.Node;

// TODO : future use this module to iterate syntax nodes
public readonly struct SyntaxIterator(int index, LuaSyntaxTree tree)
{
    private int RawKind => tree.GetRawKind(index);

    public LuaSyntaxKind Kind => (LuaSyntaxKind)RawKind;

    public LuaTokenKind TokenKind => (LuaTokenKind)RawKind;

    public bool IsNode => tree.IsNode(index);

    public bool IsToken => !IsNode;
}
