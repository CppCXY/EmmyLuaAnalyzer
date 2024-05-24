using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Compilation.Semantic;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Workspace;
using EmmyLua.Configuration;
using EmmyLua.LanguageServer.Server.Monitor;
using EmmyLua.LanguageServer.Server.Resource;
using EmmyLua.LanguageServer.Configuration;
using EmmyLua.LanguageServer.Util;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;


namespace EmmyLua.LanguageServer.Server;

public class ServerContext(ILanguageServerFacade server)
{
    public bool IsVscode { get; set; } = true;

    private string MainWorkspacePath { get; set; } = string.Empty;

    private List<string> ExternalWorkspacePaths { get; } = new();

    private ReaderWriterLockSlim LockSlim { get; } = new();

    public LuaWorkspace LuaWorkspace { get; private set; } = LuaWorkspace.CleanCreate();

    public SettingManager SettingManager { get; } = new();

    public ILanguageServerFacade Server { get; } = server;

    private ProcessMonitor Monitor { get; } = new(server);

    public ResourceManager ResourceManager { get; } = new();

    private CancellationTokenSource? CancellationTokenSource { get; set; } = null;

    public void StartServer(InitializeParams initializeParams)
    {
        LockSlim.EnterWriteLock();
        try
        {
            CancellationTokenSource?.Cancel();
            IsVscode = string.Equals(initializeParams.ClientInfo?.Name, "Visual Studio Code",
                StringComparison.CurrentCultureIgnoreCase);
            if (initializeParams.RootPath is { } rootPath)
            {
                MainWorkspacePath = rootPath;
                LuaWorkspace.Monitor = Monitor;
                SettingManager.Watch(MainWorkspacePath);
                SettingManager.OnSettingChanged += OnConfigChanged;
                LuaWorkspace.Features = SettingManager.GetLuaFeatures();
                LuaWorkspace.InitStdLib();
                if (IsVscode && initializeParams.WorkspaceFolders is { } workspaceFolders)
                {
                    foreach (var workspaceFolder in workspaceFolders)
                    {
                        var path = workspaceFolder.Uri.ToUri().LocalPath;
                        if (path != MainWorkspacePath)
                        {
                            ExternalWorkspacePaths.Add(path);
                            LuaWorkspace.LoadWorkspace(path);
                        }
                    }
                }

                LuaWorkspace.LoadMainWorkspace(MainWorkspacePath);
                ResourceManager.Config = SettingManager.GetResourceConfig();
                CancellationTokenSource = new CancellationTokenSource();
                PushWorkspaceDiagnostics();
            }
            else
            {
                LuaWorkspace.InitStdLib();
            }
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

    public SemanticModel? GetSemanticModel(LuaDocumentId documentId)
    {
        return LuaWorkspace.Compilation.GetSemanticModel(documentId);
    }

    private void OnConfigChanged(SettingManager settingManager)
    {
        LockSlim.EnterWriteLock();
        try
        {
            var features = settingManager.GetLuaFeatures();
            UpdateFeatures(features);
            ResourceManager.Config = SettingManager.GetResourceConfig();
        }
        finally
        {
            LockSlim.ExitWriteLock();
        }
    }

    private void UpdateFeatures(LuaFeatures newFeatures)
    {
        var oldFeatures = LuaWorkspace.Features;
        var workspaceNeedReload = false;
        workspaceNeedReload |= !newFeatures.RequirePattern.SequenceEqual(oldFeatures.RequirePattern);
        workspaceNeedReload |= !newFeatures.ExcludeFolders.SequenceEqual(oldFeatures.ExcludeFolders);
        workspaceNeedReload |= !newFeatures.Extensions.SequenceEqual(oldFeatures.Extensions);
        workspaceNeedReload |= !newFeatures.WorkspaceRoots.SequenceEqual(oldFeatures.WorkspaceRoots);
        workspaceNeedReload |= !newFeatures.ThirdPartyRoots.SequenceEqual(oldFeatures.ThirdPartyRoots);
        if (workspaceNeedReload)
        {
            LuaWorkspace = LuaWorkspace.CleanCreate();
            LuaWorkspace.Monitor = Monitor;
            LuaWorkspace.Features = newFeatures;
            LuaWorkspace.InitStdLib();
            foreach (var workspacePath in ExternalWorkspacePaths)
            {
                LuaWorkspace.LoadWorkspace(workspacePath);
            }

            LuaWorkspace.LoadMainWorkspace(MainWorkspacePath);
            PushWorkspaceDiagnostics();
        }
        else // TODO check condition
        {
            LuaWorkspace.Features = newFeatures;
            PushWorkspaceDiagnostics();
        }
    }

    // 可能不能异步
    private void PushWorkspaceDiagnostics()
    {
        CancellationTokenSource?.Cancel();
        CancellationTokenSource = new CancellationTokenSource();
        _ = Task.Run(async () => { await PushWorkspaceDiagnosticsAsync(CancellationTokenSource.Token); },
            CancellationTokenSource.Token);
    }

    private async Task PushWorkspaceDiagnosticsAsync(CancellationToken cancellationToken)
    {
        Monitor.OnStartDiagnosticCheck();
        var documents = LuaWorkspace.AllDocuments.ToList();
        var diagnosticCount = documents.Count;
        var context = new ThreadLocal<SearchContext>(() =>
            new SearchContext(LuaWorkspace.Compilation, new SearchContextFeatures()));
        try
        {
            var tasks = new List<Task>();
            var currentCount = 0;
            foreach (var document in LuaWorkspace.AllDocuments)
            {
                tasks.Add(Task.Run(() =>
                {
                    if (cancellationToken.IsCancellationRequested)
                    {
                        return;
                    }

                    var count = Interlocked.Increment(ref currentCount);
                    Monitor.OnDiagnosticChecking(count, diagnosticCount);
                    LockSlim.EnterReadLock();
                    try
                    {
                        // ReSharper disable once AccessToDisposedClosure
                        var diagnostics = LuaWorkspace.Compilation.GetDiagnostics(document.Id, context.Value!);
                        Server.TextDocument.PublishDiagnostics(new PublishDiagnosticsParams()
                        {
                            Diagnostics = Container.From(diagnostics.Select(it => it.ToLspDiagnostic(document))),
                            Uri = document.Uri,
                        });
                    }
                    finally
                    {
                        LockSlim.ExitReadLock();
                    }
                }, cancellationToken));
            }

            await Task.WhenAll(tasks);
        }
        finally
        {
            context.Dispose();
        }

        Monitor.OnFinishDiagnosticCheck();
    }
}