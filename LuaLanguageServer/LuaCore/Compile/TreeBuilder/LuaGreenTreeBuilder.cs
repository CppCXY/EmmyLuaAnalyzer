using LuaLanguageServer.LuaCore.Compile.Parser;
using LuaLanguageServer.LuaCore.Kind;
using LuaLanguageServer.LuaCore.Syntax.Tree;

namespace LuaLanguageServer.LuaCore.Compile.TreeBuilder;

public class LuaGreenTreeBuilder
{
    private LuaSyntaxTree Tree { get; }

    private LuaParser Parser { get; }

    public LuaGreenTreeBuilder( LuaSyntaxTree tree, LuaParser parser)
    {
        Tree = tree;
        Parser = parser;
    }

    public void BuildTree()
    {
        StartNode(LuaSyntaxKind.Source);

        var parents = new List<LuaSyntaxKind>();
        for (var i = 0; i < Parser.Events.Count; i++)
        {
            var markEvent = Parser.Events[i];
            switch (markEvent)
            {
                case MarkEvent.NodeStart(_, LuaSyntaxKind.None):
                    break;
                case MarkEvent.NodeStart nodeStart:
                {
                    parents.Add(nodeStart.Kind);
                    var pPosition = nodeStart.Parent;
                    while (pPosition > 0)
                    {
                        if (Parser.Events[pPosition] is MarkEvent.NodeStart pEvent)
                        {
                            parents.Add(pEvent.Kind);
                            pPosition = pEvent.Parent;
                            Parser.Events[pPosition] = pEvent with { Kind = LuaSyntaxKind.None };
                        }
                        else
                        {
                            break;
                        }
                    }

                    // 反向遍历parents
                    for (var j = parents.Count - 1; j >= 0; j--)
                    {
                        var parent = parents[j];
                        StartNode(parent);
                    }

                    parents.Clear();
                    break;
                }
                case MarkEvent.EatToken token:
                {
                    EatTriavias();
                    EatToken();
                    break;
                }
                case MarkEvent.Error error:
                    break;
                case MarkEvent.NodeEnd nodeEnd:
                    parents.RemoveAt(parents.Count - 1);
                    break;
            }
        }

        FinishNode();
    }

    private void StartNode(LuaSyntaxKind kind)
    {

    }

    private void FinishNode()
    {

    }

    private void EatTriavias()
    {

    }

    private void EatToken()
    {

    }
}
