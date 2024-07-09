using System.Collections.Concurrent;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Union;

namespace EmmyLua.LanguageServer.Framework.Server.RequestManager;

public class RequestTokenManager
{
    private ConcurrentDictionary<int, CancellationTokenSource> _requestTokens = new();

    public CancellationToken Create(StringOrInt id)
    {
        if (id.StringValue is null)
        {
            var token = new CancellationTokenSource();
            _requestTokens[id.IntValue] = token;
            return token.Token;
        }
        else
        {
            return CancellationToken.None;
        }
    }

    public void ClearToken(StringOrInt id)
    {
        if (id.StringValue is null)
        {
            _requestTokens.TryRemove(id.IntValue, out _);
        }
    }

    public void CancelToken(StringOrInt id)
    {
        if (id.StringValue is null)
        {
            if (_requestTokens.TryRemove(id.IntValue, out var token))
            {
                token.Cancel();
            }
        }
    }
}
