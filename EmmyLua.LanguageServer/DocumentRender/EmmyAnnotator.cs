using MediatR;
using Newtonsoft.Json;
using OmniSharp.Extensions.JsonRpc;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace EmmyLua.LanguageServer.DocumentRender;

[Parallel, Method("emmy/annotator")]
public class EmmyAnnotatorRequestParams : IRequest<List<EmmyAnnotatorResponse>>
{
    [JsonProperty("uri", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public string Uri { get; set; } = string.Empty;
}

// TODO fix proto, 这里只是为了兼容老版本的emmylua的渲染
public enum EmmyAnnotatorType
{
    Param = 0,
    Global = 1,
    Upvalue = 3
}

public class RenderRange(Range range)
{
    [JsonProperty("range", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public Range Range = range;
}

public class EmmyAnnotatorResponse(string uri, EmmyAnnotatorType type)
{
    [JsonProperty("uri", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public string Uri = uri;

    [JsonProperty("ranges", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public List<RenderRange> Ranges = [];
    
    [JsonProperty("type", Required = Required.Default, NullValueHandling = NullValueHandling.Ignore)]
    public EmmyAnnotatorType Type = type;
}