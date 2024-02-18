using EmmyLua.CodeAnalysis.Workspace;

namespace EmmyLua.Plugin;

public interface IPlugin
{
    string Name { get; }
    string Description { get; }
    string Version { get; }
    string Author { get; }

    void OnLoad(LuaWorkspace workspace);
    void OnUnload(LuaWorkspace workspace);


}
