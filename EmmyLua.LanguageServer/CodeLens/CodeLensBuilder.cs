using EmmyLua.CodeAnalysis.Compilation.Search;
using EmmyLua.CodeAnalysis.Compilation.Semantic;
using EmmyLua.CodeAnalysis.Syntax.Kind;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.LanguageServer.Framework.Protocol.Model;
using EmmyLua.LanguageServer.Server;
using EmmyLua.LanguageServer.Util;


namespace EmmyLua.LanguageServer.CodeLens;

public class CodeLensBuilder
{
    private const string VscodeCommandName = "emmy.showReferences";
    private const string OtherCommandName = "editor.action.showReferences";

    public List<Framework.Protocol.Message.CodeLens.CodeLens> Build(SemanticModel semanticModel, ServerContext context)
    {
        var codeLens = new List<Framework.Protocol.Message.CodeLens.CodeLens>();
        var funcStats = semanticModel.Document
            .SyntaxTree
            .SyntaxRoot
            .Descendants
            .OfType<LuaFuncStatSyntax>();

        var document = semanticModel.Document;
        foreach (var funcStat in funcStats)
        {
            if (funcStat.FirstChildToken(LuaTokenKind.TkFunction) is { } funcToken)
            {
                codeLens.Add(new Framework.Protocol.Message.CodeLens.CodeLens()
                {
                    Range = funcToken.Range.ToLspRange(document),
                    Data = funcStat.UniqueId.ToString()
                });
            }
        }

        return codeLens;
    }

    public Framework.Protocol.Message.CodeLens.CodeLens Resolve(
        Framework.Protocol.Message.CodeLens.CodeLens codeLens, ServerContext context)
    {
        if (codeLens.Data?.Value is string uniqueIdString)
        {
            try
            {
                var ptr = LuaElementPtr<LuaSyntaxElement>.From(uniqueIdString);
                if (ptr.DocumentId is { } documentId)
                {
                    if (ptr.ToNode(context.LuaWorkspace) is LuaFuncStatSyntax
                        {
                            NameElement: { Parent: { } element } nameElement
                        })
                    {
                        var semanticModel = context.GetSemanticModel(documentId);
                        if (semanticModel is not null)
                        {
                            var references = semanticModel.FindReferences(element).ToList();
                            codeLens = new Framework.Protocol.Message.CodeLens.CodeLens()
                            {
                                Range = codeLens.Range,
                                Command = MakeCommand(references, nameElement, context)
                            };
                        }
                    }
                }
            }
            catch
            {
                // ignore
            }
        }

        return codeLens;
    }

    private static Command MakeCommand(List<ReferenceResult> results, LuaSyntaxElement element,
        ServerContext serverContext)
    {
        var range = element.Range;
        var line = element.Tree.Document.GetLine(range.StartOffset);
        var col = element.Tree.Document.GetCol(range.StartOffset);
        var position = new Position(line, col);
        var locations = results.Select(it => it.Location.ToLspLocation()).ToList();
        return new Command
        {
            Title = $"{results.Count - 1} usage",
            Name = serverContext.IsVscode ? VscodeCommandName : OtherCommandName,
            Arguments =
            [
                element.Tree.Document.Uri,
                new LSPAny(position),
                new LSPAny(locations)
            ]
        };
    }
}