using EmmyLua.CodeAnalysis.Workspace;
using LanguageServer.Util;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace LanguageServer.Completion;

// ReSharper disable once ClassNeverInstantiated.Global
public class CompletionHandler(LuaWorkspace workspace) : CompletionHandlerBase
{
    private CompletionBuilder Builder { get; } = new();

    protected override CompletionRegistrationOptions CreateRegistrationOptions(CompletionCapability capability,
        ClientCapabilities clientCapabilities)
    {
        return new()
        {
            DocumentSelector = ToSelector.ToTextDocumentSelector(workspace),
            ResolveProvider = true,
            TriggerCharacters = new List<string> { ".", ":", "(", "[", "\"", "\'", "," },
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
            var context = new CompleteContext(semanticModel, request.Position, cancellationToken);

            var completions = Builder.Build(context);
            return Task.FromResult(CompletionList.From(completions));
        }

        return Task.FromResult(new CompletionList());
    }

    public override Task<CompletionItem> Handle(CompletionItem request, CancellationToken cancellationToken)
    {
        return Task.FromResult(request);
    }
}