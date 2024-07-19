using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server.Options;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Completion;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using EmmyLua.LanguageServer.Server;

namespace EmmyLua.LanguageServer.Completion;

// ReSharper disable once ClassNeverInstantiated.Global
public class CompletionHandler(ServerContext context) : CompletionHandlerBase
{
    private CompletionBuilder Builder { get; } = new();

    private CompletionDocumentResolver DocumentResolver { get; } = new();
    
    protected override Task<CompletionResponse?> Handle(CompletionParams request, CancellationToken token)
    {
        var uri = request.TextDocument.Uri.UnescapeUri;
        CompletionResponse? response = null;
        // using var _ = await context.ReadyWorkspaceAsync();
        context.ReadyRead(() =>
        {
            var semanticModel = context.GetSemanticModel(uri);
            if (semanticModel is not null)
            {
                var completeContext = new CompleteContext(semanticModel, request.Position, token, context);
                var completions = Builder.Build(completeContext);
                response = new CompletionResponse(completions);
            }
        });

        return Task.FromResult(response)!;
    }

    protected override Task<CompletionItem> Resolve(CompletionItem item, CancellationToken token)
    {
        context.ReadyRead(() =>
        {
            item = DocumentResolver.Resolve(item, context);
        });
        
        return Task.FromResult(item);
    }

    public override void RegisterCapability(ServerCapabilities serverCapabilities,
        ClientCapabilities clientCapabilities)
    {
        serverCapabilities.CompletionProvider = new CompletionOptions()
        {
            ResolveProvider = true,
            TriggerCharacters = [".", ":", "(", "[", "\"", "\'", ",", "@", "\\", "/"],
            CompletionItem = new()
            {
                LabelDetailsSupport = true
            }
        };
    }
}