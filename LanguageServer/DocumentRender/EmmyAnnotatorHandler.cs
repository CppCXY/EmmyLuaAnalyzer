using EmmyLua.CodeAnalysis.Workspace;
using OmniSharp.Extensions.JsonRpc;
using OmniSharp.Extensions.LanguageServer.Protocol;

namespace LanguageServer.DocumentRender;


[Parallel, Method("emmy/annotator")]
public class EmmyAnnotatorHandler(LuaWorkspace workspace) : IJsonRpcRequestHandler<EmmyAnnotatorRequestParams, List<EmmyAnnotatorResponse>>
{
    private EmmyAnnotatorBuilder Builder { get; } = new();
    
    public Task<List<EmmyAnnotatorResponse>> Handle(EmmyAnnotatorRequestParams request, CancellationToken cancellationToken)
    {
        var documentUri = DocumentUri.From(request.uri);
        var uri = documentUri.ToUnencodedString();
        var semanticModel = workspace.Compilation.GetSemanticModel(uri);
        if (semanticModel is not null)
        {
            return Task.FromResult(Builder.Build(semanticModel));
        }
        return Task.FromResult(new List<EmmyAnnotatorResponse>());
    }
}
