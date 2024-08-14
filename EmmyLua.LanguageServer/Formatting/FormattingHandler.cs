using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Client.ClientCapabilities;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Server;
using EmmyLua.LanguageServer.Framework.Protocol.Message.DocumentFormatting;
using EmmyLua.LanguageServer.Framework.Protocol.Model;
using EmmyLua.LanguageServer.Framework.Protocol.Model.TextEdit;
using EmmyLua.LanguageServer.Framework.Server.Handler;
using EmmyLua.LanguageServer.Server;
using EmmyLua.LanguageServer.Util;

namespace EmmyLua.LanguageServer.Formatting;

public class FormattingHandler(ServerContext context) : DocumentFormattingHandlerBase
{
    private FormattingBuilder Builder { get; } = new();

    protected override Task<DocumentFormattingResponse?> Handle(DocumentFormattingParams request,
        CancellationToken token)
    {
        var uri = request.TextDocument.Uri.UnescapeUri;
        DocumentFormattingResponse? response = null;
        context.ReadyRead(() =>
        {
            var semanticModel = context.GetSemanticModel(uri);
            if (semanticModel is not null)
            {
                var filePath = semanticModel.Document.Path;
                var formattedCode = Builder.Format(semanticModel.Document.Text, filePath);
                if (formattedCode.Length > 0)
                {
                    if (!context.IsVscode)
                    {
                        formattedCode = formattedCode.Replace("\r\n", "\n");
                    }
                    
                    response = new DocumentFormattingResponse(new TextEdit()
                    {
                        Range = new DocumentRange(
                            new Position(0, 0),
                            new Position(semanticModel.Document.TotalLine + 1, 0)),
                        NewText = formattedCode
                    });
                }
            }
        });

        return Task.FromResult(response);
    }

    protected override Task<DocumentFormattingResponse?> Handle(DocumentRangeFormattingParams request,
        CancellationToken token)
    {
        var uri = request.TextDocument.Uri.UnescapeUri;
        DocumentFormattingResponse? response = null;
        context.ReadyRead(() =>
        {
            var semanticModel = context.GetSemanticModel(uri);
            if (semanticModel is not null)
            {
                var range = request.Range;
                var startLine = range.Start.Line;
                var startChar = 0;
                var endLine = range.End.Line;
                var endChar = 0;
                var path = semanticModel.Document.Path;
                var formattedCode = Builder.RangeFormat(semanticModel.Document.Text, path,
                    ref startLine, ref startChar,
                    ref endLine, ref endChar);

                if (formattedCode.Length > 0)
                {
                    if (!context.IsVscode)
                    {
                        formattedCode = formattedCode.Replace("\r\n", "\n");
                    }
                    
                    response = new DocumentFormattingResponse(new TextEdit()
                    {
                        Range = new DocumentRange(
                            new Position(startLine, startChar),
                            new Position(endLine + 1, 0)),
                        NewText = formattedCode
                    });
                }
            }
        });

        return Task.FromResult(response);
    }

    protected override Task<DocumentFormattingResponse?> Handle(DocumentRangesFormattingParams request,
        CancellationToken token)
    {
        throw new NotImplementedException();
    }

    protected override Task<DocumentFormattingResponse?> Handle(DocumentOnTypeFormattingParams request,
        CancellationToken token)
    {
        throw new NotImplementedException();
    }

    public override void RegisterCapability(ServerCapabilities serverCapabilities,
        ClientCapabilities clientCapabilities)
    {
        serverCapabilities.DocumentFormattingProvider = true;
        serverCapabilities.DocumentRangeFormattingProvider = true;
    }
}