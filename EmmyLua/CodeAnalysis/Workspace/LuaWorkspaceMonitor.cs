namespace EmmyLua.CodeAnalysis.Workspace;

// ReSharper disable once ClassNeverInstantiated.Global
public class LuaWorkspaceMonitor
{
    public virtual void OnStartLoadWorkspace()
    {
    }

    public virtual void OnFinishLoadWorkspace()
    {
    }

    public virtual void OnAnalyzing(string text)
    {
    }

    public virtual void OnDiagnosticChecking(string path, int total)
    {
    }

    public virtual void OnStartDiagnosticCheck()
    {
    }

    public virtual void OnFinishDiagnosticCheck()
    {
    }
}
