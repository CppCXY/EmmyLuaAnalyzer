using EmmyLua.CodeAnalysis.Compilation.Semantic;
using EmmyLua.CodeAnalysis.Workspace;
using EmmyLua.Configuration;
using LanguageServer.Server.Monitor;
using LanguageServer.Util;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;


namespace LanguageServer.Server;

public class ServerContext(ILanguageServerFacade server)
{
    private string MainWorkspacePath { get; set; } = string.Empty;
    
    private ReaderWriterLockSlim LockSlim { get; } = new();

    public LuaWorkspace LuaWorkspace { get; private set; } = LuaWorkspace.Create();

    public SettingManager SettingManager { get; } = new();

    public ILanguageServerFacade Server { get; } = server;

    private ProcessMonitor Monitor { get; } = new(server);

    public void StartServer(string workspacePath)
    {
        LockSlim.EnterWriteLock();
        try
        {
            MainWorkspacePath = workspacePath;
            LuaWorkspace.Monitor = Monitor;
            SettingManager.Watch(workspacePath);
            SettingManager.OnSettingChanged += OnConfigChanged;
            LuaWorkspace.Features = SettingManager.GetLuaFeatures();
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
            UpdateFeatures(features);
        }
        finally
        {
            LockSlim.ExitWriteLock();
        }
    }
    
    private void UpdateFeatures(LuaFeatures newFeatures)
    {
        var oldFeatures = LuaWorkspace.Features;
        var requirePatternChanged = newFeatures.RequirePattern.SequenceEqual(oldFeatures.RequirePattern);
        var excludeFoldersChanged = newFeatures.ExcludeFolders.SequenceEqual(oldFeatures.ExcludeFolders);
        var extensionsChanged = newFeatures.Extensions.SequenceEqual(oldFeatures.Extensions);
        if (requirePatternChanged || excludeFoldersChanged || extensionsChanged)
        {
            LuaWorkspace = LuaWorkspace.Create();
            LuaWorkspace.Monitor = Monitor;
            LuaWorkspace.Features = newFeatures;
            LuaWorkspace.LoadWorkspace(MainWorkspacePath);
            PushWorkspaceDiagnostics();
        }
        else // TODO check condition
        {
            LuaWorkspace.RefreshDiagnostics();
            PushWorkspaceDiagnostics();
        }
    }

    private void PushWorkspaceDiagnostics()
    {
        foreach (var document in LuaWorkspace.AllDocuments)
        {
            var diagnostics = LuaWorkspace.Compilation.GetDiagnostic(document.Id);
            Server.TextDocument.PublishDiagnostics(new PublishDiagnosticsParams()
            {
                Diagnostics = Container.From(diagnostics.Select(it => it.ToLspDiagnostic(document))),
                Uri = document.Uri,
            });
        }
    }
}