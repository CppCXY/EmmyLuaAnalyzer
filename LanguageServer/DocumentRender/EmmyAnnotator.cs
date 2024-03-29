using MediatR;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace LanguageServer.DocumentRender;


public class EmmyAnnotatorRequestParams : IRequest<EmmyAnnotatorResponse>
{
    public string uri;
}

public enum EmmyAnnotatorType {
    Param,
    Global,
    DocType,
    Upvalue
}

public class RenderRange
{
    public Range range;
}

public class EmmyAnnotatorResponse
{
    public string uri;

    public RenderRange range;
    
    public EmmyAnnotatorType type;
}