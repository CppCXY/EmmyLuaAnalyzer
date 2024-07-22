using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Rename;
using EmmyLua.LanguageServer.Framework.Protocol.Model;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using EmmyLua.LanguageServer.Server;

namespace EmmyLua.LanguageServer.Rename;

// ReSharper disable once ClassNeverInstantiated.Global
public class RenameHandler(ServerContext context) : RenameHandlerBase
{
    private RenameBuilder Builder { get; } = new(context);
    
    protected override Task<WorkspaceEdit?> Handle(RenameParams request, CancellationToken token)
    {
        var uri = request.TextDocument.Uri.UnescapeUri;
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
        
        return Task.FromResult(workspaceEdit);
    }

    protected override Task<PrepareRenameResponse> Handle(PrepareRenameParams request, CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public override void RegisterCapability(ServerCapabilities serverCapabilities, ClientCapabilities clientCapabilities)
    {
        serverCapabilities.RenameProvider = true;
    }
}