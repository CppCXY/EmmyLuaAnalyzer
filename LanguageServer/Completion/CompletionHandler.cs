using EmmyLua.CodeAnalysis.Workspace;
using LanguageServer.Configuration;
using LanguageServer.Util;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace LanguageServer.Completion;

// ReSharper disable once ClassNeverInstantiated.Global
public class CompletionHandler(LuaWorkspace workspace, LuaConfig config) : CompletionHandlerBase
{
    private CompletionBuilder Builder { get; } = new();

    private CompletionDocumentResolver DocumentResolver { get; } = new(workspace);

    protected override CompletionRegistrationOptions CreateRegistrationOptions(CompletionCapability capability,
        ClientCapabilities clientCapabilities)
    {
        return new()
        {
            DocumentSelector = ToSelector.ToTextDocumentSelector(workspace),
            ResolveProvider = true,
            TriggerCharacters = new List<string> { ".", ":", "(", "[", "\"", "\'", ",", "@" },
            CompletionItem = new()
            {
                LabelDetailsSupport = true
            }
        };
    }

    public override Task<CompletionList> Handle(CompletionParams request, CancellationToken cancellationToken)
    {
        var uri = request.TextDocument.Uri.ToUnencodedString();
        var semanticModel = workspace.Compilation.GetSemanticModel(uri);
        if (semanticModel is not null)
        {
            var context = new CompleteContext(semanticModel, request.Position, cancellationToken, config.DotLuaRc.Completion);

            var completions = Builder.Build(context);
            return Task.FromResult(CompletionList.From(completions));
        }

        return Task.FromResult(new CompletionList());
    }

    public override Task<CompletionItem> Handle(CompletionItem request, CancellationToken cancellationToken)
    {
        return Task.FromResult(DocumentResolver.Resolve(request));
    }
}