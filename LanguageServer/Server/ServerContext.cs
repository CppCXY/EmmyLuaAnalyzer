using EmmyLua.CodeAnalysis.Compilation.Semantic;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Workspace;
using EmmyLua.Configuration;
using LanguageServer.Server.Monitor;
using LanguageServer.Util;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;


namespace LanguageServer.Server;

public class ServerContext(ILogger<ServerContext> logger, ILanguageServerFacade server)
{
    private ReaderWriterLockSlim LockSlim { get; } = new();

    public LuaWorkspace LuaWorkspace { get; } = LuaWorkspace.Create();

    public SettingManager SettingManager { get; } = new();

    public ILanguageServerFacade Server { get; } = server;

    private ProcessMonitor Monitor { get; } = new(server);

    public void StartServer(string workspacePath)
    {
        LockSlim.EnterWriteLock();
        try
        {
            LuaWorkspace.Monitor = Monitor;
            SettingManager.Watch(workspacePath);
            SettingManager.OnSettingChanged += OnConfigChanged;
            LuaWorkspace.UpdateFeatures(SettingManager.GetLuaFeatures());
            LuaWorkspace.LoadWorkspace(workspacePath);
            PushWorkspaceDiagnostics();
        }
        finally
        {
            LockSlim.ExitWriteLock();
        }
    }

    public void ReadyWrite(Action action)
    {
        LockSlim.EnterWriteLock();
        try
        {
            action();
        }
        finally
        {
            LockSlim.ExitWriteLock();
        }
    }

    public void ReadyRead(Action action)
    {
        LockSlim.EnterReadLock();
        try
        {
            action();
        }
        finally
        {
            LockSlim.ExitReadLock();
        }
    }

    public SemanticModel? GetSemanticModel(string uri)
    {
        return LuaWorkspace.Compilation.GetSemanticModel(uri);
    }

    private void OnConfigChanged(SettingManager settingManager)
    {
        LockSlim.EnterWriteLock();
        try
        {
            var features = settingManager.GetLuaFeatures();
            LuaWorkspace.UpdateFeatures(features);
        }
        finally
        {
            LockSlim.ExitWriteLock();
        }
    }

    private void PushWorkspaceDiagnostics()
    {
        foreach (var tree in LuaWorkspace.Compilation.SyntaxTrees)
        {
            var document = tree.Document;
            var diagnostics = tree.Diagnostics;
            Server.TextDocument.PublishDiagnostics(new PublishDiagnosticsParams()
            {
                Diagnostics = Container.From(diagnostics.Select(it => it.ToLspDiagnostic(document))),
                Uri = document.Uri,
            });
        }
    }
}