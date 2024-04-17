using EmmyLua.CodeAnalysis.Workspace;
using LanguageServer.Monitor;
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
            health = "ok",
            loading = true,
            message = "Loading workspace"
        });
        languageServerFacade.SendNotification("emmy/progressReport", new ProgressReport
        {
            text = "Loading workspace",
            percent = 0
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
                text = "Finished!",
                percent = 1
            });
            languageServerFacade.SendNotification("emmy/setServerStatus", new ServerStatusParams
            {
                health = "ok",
                loading = false,
                message = "EmmyLua Language Server"
            });
        }
    }
    
    public override void OnAnalyzing(string text)
    {
        if (State == ProcessState.Running)
        {
            languageServerFacade.SendNotification("emmy/progressReport", new ProgressReport
            {
                text = $"{text} analyzing",
                percent = 0.5
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
                text = $"checking {Path.GetFileName(path)} {DiagnosticCount}/{total}",
                percent = 0.5
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
                text = "checking diagnostics",
                percent = 0.5
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
                text = "Check finished!",
                percent = 1
            });
        }
    }
}