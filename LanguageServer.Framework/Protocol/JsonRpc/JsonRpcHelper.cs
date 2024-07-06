using System.Reflection;

namespace EmmyLua.LanguageServer.Framework.Protocol.JsonRpc;

public static class JsonRpcHelper
{
    private static Dictionary<string, Type> JsonRpcMethods { get; } = new();

    public static Type GetMethodType(string method)
    {
        return JsonRpcMethods[method];
    }
    
    static JsonRpcHelper()
    {
        var assembly = Assembly.GetExecutingAssembly();
        foreach (var type in assembly.GetTypes())
        {
            var jsonRpcAttribute = JsonRpcAttribute.From(type);
            if (jsonRpcAttribute is not null)
            {
                JsonRpcMethods[jsonRpcAttribute.Method] = type;
            }
        }    
    }
}