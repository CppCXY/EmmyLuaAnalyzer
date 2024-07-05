using System.Text;
using System.Text.Json;
using EmmyLua.LanguageServer.Framework.Protocol.JsonRpc;
using EmmyLua.LanguageServer.Framework.Server.Reader;

namespace EmmyLua.LanguageServer.Framework.Server;

public class LanguageServer(Stream input, Stream output)
{
    private Stream Input { get; } = input;

    private Stream Output { get; } = output;

    public static LanguageServer From(Stream input, Stream output)
    {
        return new LanguageServer(input, output);
    }

    public void SendNotification(NotificationMessage notification)
    {
        // var json = JsonSerializer.Serialize(notification);
        // var bytes = Encoding.UTF8.GetBytes(json);
        // Output.Write(bytes);
    }

    public async Task Run()
    {
        var reader = new JsonProtocolReader(Input);
        try
        {
            while (true)
            {
                var message = await reader.ReadAsync();
                switch (message)
                {
                    case RequestMessage request:
                        HandleRequest(request);
                        break;
                    case NotificationMessage notification:
                        HandleNotification(notification);
                        break;
                }
            }
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
        }
    }

    private void HandleRequest(RequestMessage request)
    {
        if (request.Method == "initialize")
        {
            // var response = new ResponseMessage(request.Id, new InitializeResult());
            // var json = JsonSerializer.Serialize(response);
            // var bytes = Encoding.UTF8.GetBytes(json);
            // Output.Write(bytes);
        }
    }

    private void HandleNotification(NotificationMessage notification)
    {
        if (notification.Method == "initialized")
        {
            Console.WriteLine("Server initialized");
        }
    }
}