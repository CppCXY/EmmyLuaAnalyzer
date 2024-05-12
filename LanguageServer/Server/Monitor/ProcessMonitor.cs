using EmmyLua.CodeAnalysis.Workspace;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;

namespace LanguageServer.Server.Monitor;

public class ProcessMonitor(ILanguageServerFacade languageServerFacade) : LuaWorkspaceMonitor
{
    enum ProcessState
    {
        None,
        Running,
    }
    
    private ProcessState State { get; set; } = ProcessState.None;
    
    private int DiagnosticCount { get; set; }
    
    public override void OnStartLoadWorkspace()
    {
        State = ProcessState.Running;
        DiagnosticCount = 0;
        languageServerFacade.SendNotification("emmy/setServerStatus", new ServerStatusParams
        {
            Health = "ok",
            Loading = true,
            Message = "Loading workspace"
        });
        languageServerFacade.SendNotification("emmy/progressReport", new ProgressReport
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
            DiagnosticCount = 0;
            languageServerFacade.SendNotification("emmy/progressReport", new ProgressReport
            {
                Text = "Finished!",
                Percent = 1
            });
            languageServerFacade.SendNotification("emmy/setServerStatus", new ServerStatusParams
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
            languageServerFacade.SendNotification("emmy/progressReport", new ProgressReport
            {
                Text = $"{text} analyzing",
                Percent = 0.5
            });
        }
    }
    
    public override void OnDiagnosticChecking(string path, int total)
    {
        if (State == ProcessState.Running)
        {
            DiagnosticCount++;
            languageServerFacade.SendNotification("emmy/progressReport", new ProgressReport
            {
                Text = $"checking {Path.GetFileName(path)} {DiagnosticCount}/{total}",
                Percent = 0.5
            });
        }
    }
    
    public override void OnStartDiagnosticCheck()
    {
        if (State == ProcessState.None)
        {
            State = ProcessState.Running;
            languageServerFacade.SendNotification("emmy/progressReport", new ProgressReport
            {
                Text = "checking diagnostics",
                Percent = 0.5
            });
        }
    }
    
    public override void OnFinishDiagnosticCheck()
    {
        if (State == ProcessState.Running)
        {
            State = ProcessState.None;
            DiagnosticCount = 0;
            languageServerFacade.SendNotification("emmy/progressReport", new ProgressReport
            {
                Text = "Check finished!",
                Percent = 1
            });
        }
    }
}