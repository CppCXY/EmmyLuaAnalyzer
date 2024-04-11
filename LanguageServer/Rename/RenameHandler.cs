using LanguageServer.Server;
using LanguageServer.Util;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace LanguageServer.Rename;

// ReSharper disable once ClassNeverInstantiated.Global
public class RenameHandler(ServerContext context) : RenameHandlerBase
{
    private RenameBuilder Builder { get; } = new();
    
    protected override RenameRegistrationOptions CreateRegistrationOptions(RenameCapability capability,
        ClientCapabilities clientCapabilities)
    {
        return new RenameRegistrationOptions()
        {
            DocumentSelector = ToSelector.ToTextDocumentSelector(context.LuaWorkspace),
            PrepareProvider = false
        };
    }

    public override Task<WorkspaceEdit?> Handle(RenameParams request, CancellationToken cancellationToken)
    {
        var uri = request.TextDocument.Uri.ToUnencodedString();
        WorkspaceEdit? workspaceEdit = null;
        context.ReadyRead(() =>
        {
            var semanticModel = context.GetSemanticModel(uri);
            if (semanticModel is not null)
            {
                var document = semanticModel.Document;
                var pos = request.Position;
                var node = document.SyntaxTree.SyntaxRoot.NameNodeAt(pos.Line, pos.Character);
                if (node is not null)
                {
                    var newName = request.NewName;
                    var changes = Builder.Build(semanticModel, node, newName);
                    workspaceEdit = new WorkspaceEdit()
                    {
                        Changes = changes
                    };
                }
            }
        });
        
        return Task.FromResult<WorkspaceEdit?>(workspaceEdit);
    }
}