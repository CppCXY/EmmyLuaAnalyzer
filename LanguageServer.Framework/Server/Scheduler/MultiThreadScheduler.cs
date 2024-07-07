namespace EmmyLua.LanguageServer.Framework.Server.Scheduler;

public class MultiThreadScheduler : IScheduler
{
    public void Schedule(Func<Task> action)
    {
        Task.Run(action);
    }
}
