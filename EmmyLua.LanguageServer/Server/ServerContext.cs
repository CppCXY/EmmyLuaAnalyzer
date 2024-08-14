using System.Text.Json;
using EmmyLua.CodeAnalysis.Compilation.Search;
using EmmyLua.CodeAnalysis.Compilation.Semantic;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Workspace;
using EmmyLua.Configuration;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Client.PublishDiagnostics;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Configuration;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Initialize;
using EmmyLua.LanguageServer.Server.ClientConfig;
using EmmyLua.LanguageServer.Server.Editorconfig;
using EmmyLua.LanguageServer.Server.Monitor;
using EmmyLua.LanguageServer.Server.Resource;
using EmmyLua.LanguageServer.Util;

namespace EmmyLua.LanguageServer.Server;

public class ServerContext(Framework.Server.LanguageServer server)
{
    public bool IsVscode { get; private set; } = true;

    private HashSet<string> Extensions { get; } = new();

    private string MainWorkspacePath { get; set; } = string.Empty;

    private List<string> ExternalWorkspacePaths { get; } = new();

    private ReaderWriterLockSlim LockSlim { get; } = new();

    public LuaProject LuaProject { get; private set; } = LuaProject.CleanCreate();

    public SettingManager SettingManager { get; } = new();

    public Framework.Server.LanguageServer Server { get; } = server;

    private ProcessMonitor Monitor { get; } = new(server);

    public ResourceManager ResourceManager { get; } = new();

    private CancellationTokenSource? WorkspaceCancellationTokenSource { get; set; } = null;
    
    private EditorconfigWatcher EditorconfigWatcher { get; } = new();

    public async Task StartServerAsync(InitializeParams initializeParams)
    {
        IsVscode = string.Equals(initializeParams.ClientInfo?.Name, "Visual Studio Code",
            StringComparison.CurrentCultureIgnoreCase);

        if (IsVscode)
        {
            var config = await Server.Client.GetConfiguration(new()
            {
                Items =
                [
                    new ConfigurationItem()
                    {
                        Section = "files"
                    }
                ]
            }, CancellationToken.None);

            if (config.FirstOrDefault()?.Value is JsonDocument jsonDocument)
            {
                var filesConfig = jsonDocument.Deserialize<FilesConfig>(Server.JsonSerializerOptions)!;
                foreach (var (ext, language) in filesConfig.Associations)
                {
                    if (string.Equals(language, "lua", StringComparison.CurrentCultureIgnoreCase))
                    {
                        Extensions.Add(ext);
                    }
                }

                if (filesConfig.Encoding.Length > 0 && filesConfig.Encoding != "utf8")
                {
                    SettingManager.WorkspaceEncoding = filesConfig.Encoding;
                }
            }
        }

        StartServer(initializeParams);
    }

    private void StartServer(InitializeParams initializeParams)
    {
        LockSlim.EnterWriteLock();
        try
        {
            var rootPath = string.Empty;
            if (initializeParams.RootUri is { } rootUri)
            {
                rootPath = rootUri.FileSystemPath;
            }

            if (rootPath.Length > 0)
            {
                MainWorkspacePath = rootPath;
                LuaProject.Monitor = Monitor;
                SettingManager.SupportMultiEncoding();
                SettingManager.Watch(MainWorkspacePath);
                SettingManager.OnSettingChanged += OnConfigChanged;
                SettingManager.WorkspaceExtensions = Extensions;
                LuaProject.MainWorkspacePath = MainWorkspacePath;
                LuaProject.Features = SettingManager.GetLuaFeatures();
                LuaProject.InitStdLib();
                if (IsVscode && initializeParams.WorkspaceFolders is { } workspaceFolders)
                {
                    foreach (var workspaceFolder in workspaceFolders)
                    {
                        var path = workspaceFolder.Uri.FileSystemPath;
                        if (path != MainWorkspacePath)
                        {
                            ExternalWorkspacePaths.Add(path);
                            LuaProject.LoadWorkspace(path);
                            EditorconfigWatcher.LoadWorkspaceEditorconfig(path);
                        }
                    }
                }
                // TODO: read config from initializeOptions

                LuaProject.LoadMainWorkspace(MainWorkspacePath);
                EditorconfigWatcher.LoadWorkspaceEditorconfig(MainWorkspacePath);
                ResourceManager.Config = SettingManager.GetResourceConfig();
                WorkspaceCancellationTokenSource = new CancellationTokenSource();
                PushWorkspaceDiagnostics();
            }
            else
            {
                LuaProject.InitStdLib();
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
        return LuaProject.Compilation.GetSemanticModel(uri);
    }

    public SemanticModel? GetSemanticModel(LuaDocumentId documentId)
    {
        return LuaProject.Compilation.GetSemanticModel(documentId);
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
        var oldFeatures = LuaProject.Features;
        var workspaceNeedReload = false;
        workspaceNeedReload |= !newFeatures.RequirePattern.SequenceEqual(oldFeatures.RequirePattern);
        workspaceNeedReload |= !newFeatures.ExcludeFolders.SequenceEqual(oldFeatures.ExcludeFolders);
        workspaceNeedReload |= !newFeatures.ExcludeGlobs.SequenceEqual(oldFeatures.ExcludeGlobs);
        workspaceNeedReload |= !newFeatures.Includes.SequenceEqual(oldFeatures.Includes);
        workspaceNeedReload |= !newFeatures.WorkspaceRoots.SequenceEqual(oldFeatures.WorkspaceRoots);
        workspaceNeedReload |= !newFeatures.ThirdPartyRoots.SequenceEqual(oldFeatures.ThirdPartyRoots);
        if (workspaceNeedReload)
        {
            LuaProject = LuaProject.CleanCreate();
            LuaProject.Monitor = Monitor;
            LuaProject.MainWorkspacePath = MainWorkspacePath;
            LuaProject.Features = newFeatures;
            LuaProject.InitStdLib();
            foreach (var workspacePath in ExternalWorkspacePaths)
            {
                LuaProject.LoadWorkspace(workspacePath);
            }

            LuaProject.LoadMainWorkspace(MainWorkspacePath);
            PushWorkspaceDiagnostics();
        }
        else // TODO check condition
        {
            LuaProject.Features = newFeatures;
            PushWorkspaceDiagnostics();
        }
    }

    private void PushWorkspaceDiagnostics()
    {
        WorkspaceCancellationTokenSource?.Cancel();
        WorkspaceCancellationTokenSource = new CancellationTokenSource();
        _ = Task.Run(async () => { await PushWorkspaceDiagnosticsAsync(WorkspaceCancellationTokenSource.Token); },
            WorkspaceCancellationTokenSource.Token);
    }

    private async Task PushWorkspaceDiagnosticsAsync(CancellationToken cancellationToken)
    {
        Monitor.OnStartDiagnosticCheck();
        var documents = LuaProject.AllDocuments.ToList();
        var diagnosticCount = documents.Count;
        var context = new ThreadLocal<SearchContext>(() =>
            new SearchContext(LuaProject.Compilation, new SearchContextFeatures()));
        try
        {
            var tasks = new List<Task>();
            var currentCount = 0;
            foreach (var document in LuaProject.AllDocuments)
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
                        var diagnostics = LuaProject.Compilation.GetDiagnostics(document.Id, context.Value!);
                        Server.Client.PublishDiagnostics(new PublishDiagnosticsParams()
                        {
                            Diagnostics = diagnostics.Select(it => it.ToLspDiagnostic(document)).ToList(),
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
        GC.Collect();
    }

    public void UpdateDocument(string uri, string text, CancellationToken cancellationToken)
    {
        LuaDocumentId documentId = LuaDocumentId.VirtualDocumentId;
        ReadyWrite(() =>
        {
            LuaProject.UpdateDocumentByUri(uri, text);
            documentId = LuaProject.GetDocumentIdByUri(uri) ?? LuaDocumentId.VirtualDocumentId;
        });

        if (documentId != LuaDocumentId.VirtualDocumentId)
        {
            PushDocumentDiagnostics(documentId);
        }
    }

    public void RemoveDocument(string uri)
    {
        ReadyWrite(() => { LuaProject.RemoveDocumentByUri(uri); });
    }

    public void PushDocumentDiagnostics(LuaDocumentId documentId)
    {
        LockSlim.EnterReadLock();
        try
        {
            var document = LuaProject.GetDocument(documentId);
            if (document is null)
            {
                return;
            }

            var context = new SearchContext(LuaProject.Compilation, new SearchContextFeatures());
            var diagnostics = LuaProject.Compilation.GetDiagnostics(document.Id, context);
            Server.Client.PublishDiagnostics(new PublishDiagnosticsParams()
            {
                Diagnostics = diagnostics.Select(it => it.ToLspDiagnostic(document)).ToList(),
                Uri = document.Uri,
            });
        }
        finally
        {
            LockSlim.ExitReadLock();
        }
    }
}