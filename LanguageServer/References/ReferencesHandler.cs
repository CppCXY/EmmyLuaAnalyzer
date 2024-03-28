using EmmyLua.CodeAnalysis.Workspace;
using LanguageServer.Util;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace LanguageServer.References;

// ReSharper disable once ClassNeverInstantiated.Global
public class ReferencesHandler(LuaWorkspace workspace) : ReferencesHandlerBase
{
    protected override ReferenceRegistrationOptions CreateRegistrationOptions(ReferenceCapability capability,
        ClientCapabilities clientCapabilities)
    {
        return new ReferenceRegistrationOptions()
        {
            DocumentSelector = ToSelector.ToTextDocumentSelector(workspace)
        };
    }

    public override Task<LocationContainer?> Handle(ReferenceParams request, CancellationToken cancellationToken)
    {
        var uri = request.TextDocument.Uri.ToUnencodedString();
        var semanticModel = workspace.Compilation.GetSemanticModel(uri);
        if (semanticModel is not null)
        {
            var document = semanticModel.Document;
            var pos = request.Position;
            var node = document.SyntaxTree.SyntaxRoot.NodeAt(pos.Line, pos.Character);
            if (node is not null)
            {
                var references = semanticModel.FindReferences(node);
                return Task.FromResult<LocationContainer?>(LocationContainer.From(
                    references.Select(it => it.Location.ToLspLocation())
                ));
            }
        }

        return Task.FromResult<LocationContainer?>(null);
    }
}