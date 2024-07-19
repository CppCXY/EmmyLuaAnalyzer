using EmmyLua.LanguageServer.Framework.Protocol.JsonRpc;
using EmmyLua.LanguageServer.Framework.Server.Scheduler;

namespace EmmyLua.LanguageServer.Server.Scheduler;

public class EmmyScheduler : IScheduler
{
    public void Schedule(Func<Message, Task> action, Message message)
    {
        if (message is NotificationMessage requestMessage)
        {
            switch (requestMessage.Method)
            {
                case "textDocument/didChange":
                {
                    action(message).Wait();
                    return;
                }
            }
        }
            
        Task.Run(() => action(message));
    }
}