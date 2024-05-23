using System.Collections.Concurrent;
using EmmyLua.CodeAnalysis.Workspace;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;

namespace EmmyLua.LanguageServer.Server.Monitor;

public class ProcessMonitor(ILanguageServerFacade languageServerFacade) : LuaWorkspaceMonitor
{
    enum ProcessState
    {
        None,
        Running,
    }

    private ProcessState State { get; set; } = ProcessState.None;

    private ILanguageServerFacade LanguageServerFacade { get; } = languageServerFacade;

    // private readonly BlockingCollection<ProgressReport> _messageQueue = new();
    //
    // private CancellationTokenSource? CancellationTokenSource { get; set; } = null;
    //
    // private async Task ProcessMessagesAsync(CancellationToken cancellationToken)
    // {
    //     while (!cancellationToken.IsCancellationRequested)
    //     {
    //         ProgressReport? sendMessage = null;
    //         while (_messageQueue.TryTake(out var message))
    //         {
    //             if (sendMessage is null)
    //             {
    //                 sendMessage = message;
    //             }
    //             else if (sendMessage.Percent < message.Percent)
    //             {
    //                 sendMessage = message;
    //             }
    //         }
    //
    //         if (sendMessage is not null)
    //         {
    //             LanguageServerFacade.SendNotification("emmy/progressReport", sendMessage);
    //         }
    //
    //         await Task.Delay(100, cancellationToken); // 确保每200ms发送一次
    //     }
    // }

    public override void OnStartLoadWorkspace()
    {
        State = ProcessState.Running;
        LanguageServerFacade.SendNotification("emmy/setServerStatus", new ServerStatusParams
        {
            Health = "ok",
            Loading = true,
            Message = "Loading workspace"
        });
        LanguageServerFacade.SendNotification("emmy/progressReport", new ProgressReport
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
            LanguageServerFacade.SendNotification("emmy/progressReport", new ProgressReport
            {
                Text = "Finished!",
                Percent = 1
            });
            LanguageServerFacade.SendNotification("emmy/setServerStatus", new ServerStatusParams
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
            LanguageServerFacade.SendNotification("emmy/progressReport", new ProgressReport
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
            var message = new ProgressReport
            {
                Text = $"checking {count}/{total}",
                Percent = (double)count / total
            };
            
            LanguageServerFacade.SendNotification("emmy/progressReport", message);
            // _messageQueue.Add(message);
        }
    }

    public void OnStartDiagnosticCheck()
    {
        if (State == ProcessState.None)
        {
            State = ProcessState.Running;
            // CancellationTokenSource = new CancellationTokenSource();
            // _ = ProcessMessagesAsync(CancellationTokenSource.Token);
            LanguageServerFacade.SendNotification("emmy/progressReport", new ProgressReport
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
            // _messageQueue.CompleteAdding(); // 停止添加新的消息
            // CancellationTokenSource?.Cancel();
            // CancellationTokenSource = null;
            LanguageServerFacade.SendNotification("emmy/progressReport", new ProgressReport
            {
                Text = "Check finished!",
                Percent = 1
            });
        }
    }
}