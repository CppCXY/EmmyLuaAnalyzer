using System.Reflection;

namespace EmmyLua.LanguageServer.Framework.Protocol.JsonRpc;

[AttributeUsage(AttributeTargets.Method | AttributeTargets.Class)]
public class JsonRpcAttribute(string method) : Attribute
{
    public string Method { get; } = method;

    public static JsonRpcAttribute? From(MethodInfo method) => method.GetCustomAttribute<JsonRpcAttribute>();
    
    public static JsonRpcAttribute? From(Type type) => type.GetCustomAttribute<JsonRpcAttribute>();
}