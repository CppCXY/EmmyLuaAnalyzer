using System.Diagnostics;
using LuaLanguageServer.LuaCore.Compile.Parser;
using LuaLanguageServer.LuaCore.Compile.Source;
using LuaLanguageServer.LuaCore.Kind;
using LuaLanguageServer.LuaCore.Syntax.Diagnostic;
using LuaLanguageServer.LuaCore.Syntax.Green;

namespace LuaLanguageServer.LuaCore.Syntax.Tree;

public class LuaGreenTreeBuilder
{
    private LuaParser Parser { get; }

    private GreenNodeBuilder NodeBuilder { get; }

    private List<Diagnostic.Diagnostic> Diagnostics { get; } = new();

    public LuaGreenTreeBuilder(LuaParser parser)
    {
        Parser = parser;
        NodeBuilder = new GreenNodeBuilder();
    }

    // 多返回值
    public (GreenNode, List<Diagnostic.Diagnostic>) Build()
    {
        var tree = BuildTree();
        return (tree, Diagnostics);
    }

    private GreenNode BuildTree()
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
                    EatToken(token);
                    break;
                }
                case MarkEvent.Error error:
                {
                    var nextTokenIndex = Parser.Events.FindIndex(i, it => it is MarkEvent.EatToken);
                    if (nextTokenIndex > 0)
                    {
                        var range = Parser.Events[nextTokenIndex] switch
                        {
                            MarkEvent.EatToken(var tkRange, _) => tkRange,
                            _ => throw new UnreachableException(),
                        };
                        Diagnostics.Add(new Diagnostic.Diagnostic(DiagnosticSeverity.Error, error.Err, range));
                    }
                    else
                    {
                        Diagnostics.Add(new Diagnostic.Diagnostic(DiagnosticSeverity.Error, error.Err, new SourceRange(
                            Parser.Lexer.Source.Text.Length
                        )));
                    }

                    break;
                }
                case MarkEvent.NodeEnd:
                {
                    FinishNode();
                    break;
                }
            }
        }

        FinishNode();
        return NodeBuilder.Finish();
    }

    private void StartNode(LuaSyntaxKind kind)
    {
        NodeBuilder.StartNode(kind);
    }

    private void FinishNode()
    {
        NodeBuilder.FinishNode();
    }

    private void EatToken(MarkEvent.EatToken token)
    {
        NodeBuilder.EatToken(token.Kind, token.Range);
    }
}
