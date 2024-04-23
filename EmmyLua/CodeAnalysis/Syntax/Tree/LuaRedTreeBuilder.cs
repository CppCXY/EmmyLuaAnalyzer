using System.Collections.Immutable;
using EmmyLua.CodeAnalysis.Syntax.Green;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Syntax.Tree;

public class LuaRedTreeBuilder
{
    public LuaSourceSyntax Build(GreenNode greenRoot, LuaSyntaxTree tree)
    {
        var root = SyntaxFactory.CreateSyntax(greenRoot, tree, null, 0) as LuaSourceSyntax;
        var queue = new Queue<(LuaSyntaxElement, GreenNode)>();
        queue.Enqueue((root!, greenRoot));
        while (queue.Count != 0)
        {
            var (node, greenNode) = queue.Dequeue();
            var startOffset = node.Range.StartOffset;
            var childrenElement = new List<LuaSyntaxElement>();
            foreach (var childGreen in greenNode.Children)
            {
                var childNode = SyntaxFactory.CreateSyntax(childGreen, tree, node, startOffset);
                childNode.ChildPosition = childrenElement.Count;
                childrenElement.Add(childNode);
                startOffset += childGreen.Length;
                if (childGreen.IsNode)
                {
                    queue.Enqueue((childNode, childGreen));
                }
            }

            node.ChildrenElements = childrenElement.ToImmutableArray();
        }

        return root!;
    }
}
