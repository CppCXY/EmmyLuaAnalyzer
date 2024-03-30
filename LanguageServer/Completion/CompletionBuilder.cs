using LanguageServer.Completion.CompleteProvider;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace LanguageServer.Completion;

public class CompletionBuilder
{
    private List<ICompleteProviderBase> Providers { get; } = [
        new RequireProvider(),
        new GlobalProvider(),
        new KeywordsProvider(),
        new MemberProvider(),
        new ModuleProvider(),
        new LocalEnvProvider()
    ];
    
    public List<CompletionItem> Build(CompleteContext completeContext)
    {
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