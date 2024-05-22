using EmmyLua.CodeAnalysis.Diagnostics;
using EmmyLua.CodeAnalysis.Workspace;
using EmmyLua.Configuration;

namespace EmmyLua.Cli.Linter;

public class Linter(CheckOptions options)
{
    public int Run()
    {
        var workspacePath = options.Workspace;
        var settingManager = new SettingManager();
        settingManager.LoadSetting(workspacePath);
        var luaWorkspace = LuaWorkspace.Create(workspacePath, settingManager.GetLuaFeatures());
        var foundedError = false;
        foreach (var diagnostic in luaWorkspace.Compilation.GetAllDiagnostics())
        {
            if (diagnostic.Severity == DiagnosticSeverity.Error)
            {
                foundedError = true;
            }

            Console.WriteLine(diagnostic);
        }

        Console.WriteLine("Check done");
        return foundedError ? 1 : 0;
    }
}