using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Workspace;
using LanguageServer.Server;
using OmniSharp.Extensions.JsonRpc;
using OmniSharp.Extensions.LanguageServer.Protocol;

namespace LanguageServer.DocumentRender;


[Parallel, Method("emmy/annotator")]
public class EmmyAnnotatorHandler(ServerContext context) : IJsonRpcRequestHandler<EmmyAnnotatorRequestParams, List<EmmyAnnotatorResponse>>
{
    private EmmyAnnotatorBuilder Builder { get; } = new();
    
    public Task<List<EmmyAnnotatorResponse>> Handle(EmmyAnnotatorRequestParams request, CancellationToken cancellationToken)
    {
        var documentUri = DocumentUri.From(request.uri);
        var uri = documentUri.ToUnencodedString();
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
