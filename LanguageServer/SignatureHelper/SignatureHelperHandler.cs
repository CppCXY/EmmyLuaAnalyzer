using EmmyLua.CodeAnalysis.Workspace;
using LanguageServer.Server;
using LanguageServer.Util;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace LanguageServer.SignatureHelper;

// ReSharper disable once ClassNeverInstantiated.Global
public class SignatureHelperHandler(ServerContext context) : SignatureHelpHandlerBase
{
    private SignatureHelperBuilder Builder { get; } = new();

    protected override SignatureHelpRegistrationOptions CreateRegistrationOptions(SignatureHelpCapability capability,
        ClientCapabilities clientCapabilities)
    {
        return new()
        {
            DocumentSelector = ToSelector.ToTextDocumentSelector(context.LuaWorkspace),
            TriggerCharacters = new[] { "(", "," },
        };
    }

    public override Task<SignatureHelp?> Handle(SignatureHelpParams request, CancellationToken cancellationToken)
    {
        var uri = request.TextDocument.Uri.ToUnencodedString();
        SignatureHelp? signatureHelp = null;
        context.ReadyRead(() =>
        {
            var semanticModel = context.GetSemanticModel(uri);
            if (semanticModel is null)
            {
                return;
            }

            var position = request.Position;
            var triggerToken =
                semanticModel.Document.SyntaxTree.SyntaxRoot.TokenLeftBiasedAt(position.Line, position.Character);
            if (triggerToken is not null)
            {
                signatureHelp = Builder.Build(semanticModel, triggerToken, request);
            }
        });

        return Task.FromResult<SignatureHelp?>(signatureHelp);
    }
}