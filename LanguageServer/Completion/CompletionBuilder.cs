using LanguageServer.Completion.CompleteProvider;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace LanguageServer.Completion;

public class CompletionBuilder
{
    private List<ICompleteProviderBase> Providers { get; } = [
        new RequireProvider(),
        new ResourcePathProvider(),
        new FuncParamProvider(),
        new LocalEnvProvider(),
        new GlobalProvider(),
        new KeywordsProvider(),
        new MemberProvider(),
        new ModuleProvider(),
        new DocProvider(),
        new SelfMemberProvider(),
        new PostfixProvider()
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
        catch (OperationCanceledException)
        {
            return new();
        }
    }
}