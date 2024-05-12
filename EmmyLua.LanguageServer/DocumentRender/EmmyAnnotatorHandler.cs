using EmmyLua.LanguageServer.Server;
using OmniSharp.Extensions.JsonRpc;
using OmniSharp.Extensions.LanguageServer.Protocol;

namespace EmmyLua.LanguageServer.DocumentRender;


[Parallel, Method("emmy/annotator")]
public class EmmyAnnotatorHandler(ServerContext context) : IJsonRpcRequestHandler<EmmyAnnotatorRequestParams, List<EmmyAnnotatorResponse>>
{
    private EmmyAnnotatorBuilder Builder { get; } = new();
    
    public Task<List<EmmyAnnotatorResponse>> Handle(EmmyAnnotatorRequestParams request, CancellationToken cancellationToken)
    {
        var documentUri = DocumentUri.From(request.Uri);
        var uri = documentUri.ToUri().AbsoluteUri;
        var response = new List<EmmyAnnotatorResponse>();
        context.ReadyRead(() =>
        {
            var semanticModel = context.GetSemanticModel(uri);
            if (semanticModel is not null)
            {
                response = Builder.Build(semanticModel);
            }
        });

        return Task.FromResult(response);
    }
}
