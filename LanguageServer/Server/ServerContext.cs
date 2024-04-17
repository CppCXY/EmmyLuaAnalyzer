using EmmyLua.CodeAnalysis.Compilation.Semantic;
using EmmyLua.CodeAnalysis.Workspace;
using EmmyLua.Configuration;
using LanguageServer.Server.Monitor;
using Microsoft.Extensions.Logging;
using OmniSharp.Extensions.LanguageServer.Protocol.Server;


namespace LanguageServer.Server;

public class ServerContext(ILogger<ServerContext> logger, ILanguageServerFacade server)
{
    private ReaderWriterLockSlim LockSlim { get; } = new();

    public LuaWorkspace LuaWorkspace { get; } = LuaWorkspace.Create();

    public SettingManager SettingManager { get; } = new();

    public ILanguageServerFacade Server { get; } = server;

    private ProcessMonitor Monitor { get; } = new(server);

    public void LoadWorkspace(string workspacePath)
    {
        LockSlim.EnterWriteLock();
        try
        {
            LuaWorkspace.Monitor = Monitor;
            SettingManager.Watch(workspacePath);
            LuaWorkspace.Features = SettingManager.GetLuaFeatures();
            LuaWorkspace.LoadWorkspace(workspacePath);
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
}