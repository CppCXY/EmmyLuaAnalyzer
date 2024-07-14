using System.Text.Json;
using EmmyLua.CodeAnalysis.Workspace;
using EmmyLua.LanguageServer.Framework.Protocol.JsonRpc;

namespace EmmyLua.LanguageServer.Server.Monitor;

public class ProcessMonitor(Framework.Server.LanguageServer server) : LuaWorkspaceMonitor
{
    enum ProcessState
    {
        None,
        Running,
    }

    private ProcessState State { get; set; } = ProcessState.None;

    private Framework.Server.LanguageServer Server { get; } = server;

    public void Send(string method, object @params)
    {
        Server.SendNotification(new NotificationMessage(method,
            JsonSerializer.SerializeToDocument(@params, server.JsonSerializerOptions))).Wait();
    }

    public override void OnStartLoadWorkspace()
    {
        State = ProcessState.Running;
        Send("emmy/setServerStatus", new ServerStatusParams()
        {
            Health = "ok",
            Loading = true,
            Message = "Loading workspace"
        });
        Send("emmy/progressReport", new ProgressReport()
        {
            Text = "Loading workspace",
            Percent = 0
        });
    }

    public override void OnFinishLoadWorkspace()
    {
        if (State == ProcessState.Running)
        {
            State = ProcessState.None;
            Send("emmy/progressReport", new ProgressReport
            {
                Text = "Finished!",
                Percent = 1
            });
            Send("emmy/setServerStatus", new ServerStatusParams
            {
                Health = "ok",
                Loading = false,
                Message = "EmmyLua Language Server"
            });
        }
    }

    public override void OnAnalyzing(string text)
    {
        if (State == ProcessState.Running)
        {
            Send("emmy/progressReport", new ProgressReport
            {
                Text = $"{text} analyzing",
                Percent = 0.5
            });
        }
    }

    public void OnDiagnosticChecking(int count, int total)
    {
        if (State == ProcessState.Running)
        {
            Send("emmy/progressReport", new ProgressReport
            {
                Text = $"checking {count}/{total}",
                Percent = (double)count / total
            });
            // _messageQueue.Add(message);
        }
    }

    public void OnStartDiagnosticCheck()
    {
        if (State == ProcessState.None)
        {
            State = ProcessState.Running;
            Send("emmy/progressReport", new ProgressReport
            {
                Text = "checking diagnostics",
                Percent = 0.5
            });
        }
    }

    public void OnFinishDiagnosticCheck()
    {
        if (State == ProcessState.Running)
        {
            State = ProcessState.None;
            Send("emmy/progressReport", new ProgressReport
            {
                Text = "Check finished!",
                Percent = 1
            });
        }
    }
}