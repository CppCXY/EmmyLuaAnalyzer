namespace EmmyLua.LanguageServer.Framework.Server.Scheduler;

public class SingleThreadScheduler : IScheduler
{
    public void Schedule(Func<Task> action)
    {
        action().Wait();
    }
}
