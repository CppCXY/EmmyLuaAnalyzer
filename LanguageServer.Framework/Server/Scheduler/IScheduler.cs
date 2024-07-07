namespace EmmyLua.LanguageServer.Framework.Server.Scheduler;

public interface IScheduler
{
    public void Schedule(Func<Task> action);
}
