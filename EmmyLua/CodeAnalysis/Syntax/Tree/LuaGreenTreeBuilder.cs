using System.Diagnostics;
using EmmyLua.CodeAnalysis.Compile.Parser;
using EmmyLua.CodeAnalysis.Diagnostics;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Kind;
using EmmyLua.CodeAnalysis.Syntax.Green;

namespace EmmyLua.CodeAnalysis.Syntax.Tree;

public class LuaGreenTreeBuilder(LuaParser parser)
{
    private LuaParser Parser { get; } = parser;

    private GreenNodeBuilder NodeBuilder { get; } = new();

    private List<Diagnostic> Diagnostics { get; } = new();

    // 多返回值
    public (GreenNode, List<Diagnostic>) Build()
    {
        Parser.Parse();
        Diagnostics.AddRange(Parser.Lexer.Diagnostics);
        var root = BuildTree();
        return (root, Diagnostics);
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
                            if (pEvent.Kind == LuaSyntaxKind.None)
                            {
                                break;
                            }
                            parents.Add(pEvent.Kind);
                            // 顺序不要反了
                            Parser.Events[pPosition] = pEvent with { Kind = LuaSyntaxKind.None, Parent = 0 };
                            pPosition = pEvent.Parent;
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
                    var nextTokenIndex = -1;
                    for (var j = i; j >= 0; j--)
                    {
                        if (Parser.Events[j] is MarkEvent.EatToken)
                        {
                            nextTokenIndex = j;
                            break;
                        }
                    }
                    if (nextTokenIndex > 0)
                    {
                        var range = Parser.Events[nextTokenIndex] switch
                        {
                            MarkEvent.EatToken(var tkRange, _) => tkRange,
                            _ => throw new UnreachableException(),
                        };
                        Diagnostics.Add(new Diagnostic(DiagnosticSeverity.Error, error.Err, range));
                    }
                    else
                    {
                        Diagnostics.Add(new Diagnostic(DiagnosticSeverity.Error, error.Err, new SourceRange(
                            Parser.Lexer.Document.Text.Length
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
