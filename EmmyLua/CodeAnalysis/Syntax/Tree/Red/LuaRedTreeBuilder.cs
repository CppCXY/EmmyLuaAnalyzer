using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Tree.Green;

namespace EmmyLua.CodeAnalysis.Syntax.Tree.Red;

public class LuaRedTreeBuilder
{
    public List<RedNode> Build(GreenNode greenRoot, int totalCount)
    {
        var redNodes = new List<RedNode>(totalCount) { new RedNode(greenRoot.RawKind, new SourceRange(0, greenRoot.Length), -1, -1, -1) };

        var parentIndex = 0;
        var queue = new Queue<(int, GreenNode)>();
        queue.Enqueue((parentIndex, greenRoot));
        while (queue.Count != 0)
        {
            var (nodeIndex, greenNode) = queue.Dequeue();
            var startOffset = redNodes[nodeIndex].Range.StartOffset;
            var childStartIndex = redNodes.Count;
            var childEndIndex = -1;
            foreach (var childGreen in greenNode.Children)
            {
                var childIndex = redNodes.Count;
                childEndIndex = childIndex;
                redNodes.Add(new RedNode(childGreen.RawKind, new SourceRange(startOffset, childGreen.Length), nodeIndex,
                    -1, -1));

                startOffset += childGreen.Length;
                if (childGreen.IsNode)
                {
                    queue.Enqueue((childIndex, childGreen));
                }
            }

            if (childEndIndex != -1)
            {
                redNodes[nodeIndex] = redNodes[nodeIndex] with
                {
                    ChildStart = childStartIndex, ChildEnd = childEndIndex
                };
            }
        }

        return redNodes;
    }
}
