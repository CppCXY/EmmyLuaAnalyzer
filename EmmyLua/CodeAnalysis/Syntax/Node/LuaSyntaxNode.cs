﻿using EmmyLua.CodeAnalysis.Compile.Kind;
using EmmyLua.CodeAnalysis.Syntax.Tree;

namespace EmmyLua.CodeAnalysis.Syntax.Node;

public class LuaSyntaxNode(int index, LuaSyntaxTree tree) : LuaSyntaxElement(index, tree)
{
    public LuaSyntaxKind Kind => Tree.GetSyntaxKind(ElementId);

    public ReadOnlySpan<char> Text => Tree.Document.Text.AsSpan(Range.StartOffset, Range.Length);
}
