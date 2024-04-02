using MediatR;
using OmniSharp.Extensions.JsonRpc;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace LanguageServer.DocumentRender;

[Parallel, Method("emmy/annotator")]
public class EmmyAnnotatorRequestParams : IRequest<List<EmmyAnnotatorResponse>>
{
    public string uri;
}

// TODO fix proto, 这里只是为了兼容老版本的emmylua的渲染
public enum EmmyAnnotatorType
{
    Param = 0,
    Global = 1,
    Upvalue = 3
}

public class RenderRange(Range _range)
{
    public Range range = _range;
}

public class EmmyAnnotatorResponse(string _uri, EmmyAnnotatorType _type)
{
    public string uri = _uri;

    public List<RenderRange> ranges = new List<RenderRange>();

    public EmmyAnnotatorType type = _type;
}