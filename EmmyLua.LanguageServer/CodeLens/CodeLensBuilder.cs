using EmmyLua.CodeAnalysis.Compilation.Search;
using EmmyLua.CodeAnalysis.Compilation.Semantic;
using EmmyLua.CodeAnalysis.Syntax.Kind;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.LanguageServer.Server;
using EmmyLua.LanguageServer.Util;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json.Serialization;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace EmmyLua.LanguageServer.CodeLens;

public class CodeLensBuilder
{
    private const string VscodeCommandName = "emmy.showReferences";
    private const string OtherCommandName = "editor.action.showReferences";

    private static readonly JsonSerializer Serializer = new JsonSerializer()
    {
        ContractResolver = new CamelCasePropertyNamesContractResolver()
    };

    public CodeLensContainer Build(SemanticModel semanticModel, ServerContext context)
    {
        var codeLens = new List<OmniSharp.Extensions.LanguageServer.Protocol.Models.CodeLens>();
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
                codeLens.Add(new OmniSharp.Extensions.LanguageServer.Protocol.Models.CodeLens()
                {
                    Range = funcToken.Range.ToLspRange(document),
                    Data = funcStat.UniqueId.ToString()
                });
            }
        }

        return new CodeLensContainer(codeLens);
    }

    public OmniSharp.Extensions.LanguageServer.Protocol.Models.CodeLens Resolve(
        OmniSharp.Extensions.LanguageServer.Protocol.Models.CodeLens codeLens, ServerContext context)
    {
        if (codeLens.Data?.Type == JTokenType.String && codeLens.Data.Value<string>() is { } uniqueIdString)
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
                            codeLens = codeLens with
                            {
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
                JToken.FromObject(position, Serializer),
                JToken.FromObject(locations, Serializer)
            ]
        };
    }
}