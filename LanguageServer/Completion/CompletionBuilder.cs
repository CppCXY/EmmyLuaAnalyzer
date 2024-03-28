using EmmyLua.CodeAnalysis.Compilation.Semantic;
using EmmyLua.CodeAnalysis.Syntax.Node;
using LanguageServer.Completion.CompleteProvider;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace LanguageServer.Completion;

public class CompletionBuilder
{
    private List<ICompleteProviderBase> Providers { get; } = [
        new RequireProvider()
    ];
    
    public List<CompletionItem> Build(SemanticModel semanticModel, LuaSyntaxToken token, CancellationToken cancellationToken)
    {
        var completeContext = new CompleteContext(semanticModel, token, cancellationToken);
        try
        {
            foreach (var provider in Providers)
            {
                provider.AddCompletion(completeContext);
                if (!completeContext.Continue)
                {
                    break;
                }
            }
            return completeContext.CompletionItems.ToList();
        }
        catch (OperationCanceledException e)
        {
            return new();
        }
    }
}