using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Compilation.Search;
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

        var searchContext = new SearchContext(luaWorkspace.Compilation, new SearchContextFeatures());
        var documents = luaWorkspace.AllDocuments.ToList();
        foreach (var document in documents)
        {
            var diagnostics = luaWorkspace.Compilation.GetDiagnostics(document.Id, searchContext);
            foreach (var diagnostic in diagnostics)
            {
                if (diagnostic.Severity == DiagnosticSeverity.Error)
                {
                    foundedError = true;
                }

                var location = document.GetLocation(diagnostic.Range, 1);
                Console.WriteLine($"{location}: {diagnostic.Severity}: {diagnostic.Message} ({diagnostic.Code})");
            }
        }

        Console.WriteLine("Check done!");
        return foundedError ? 1 : 0;
    }
}