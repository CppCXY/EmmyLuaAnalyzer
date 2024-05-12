using Docfx;
using EmmyLua.Cli.DocGenerator.Markdown;
using EmmyLua.Cli.DocGenerator.Proto;
using EmmyLua.CodeAnalysis.Workspace;
using EmmyLua.Configuration;
using YamlDotNet.Serialization;

namespace EmmyLua.Cli.DocGenerator;

public class DocGenerator(DocOptions options)
{
    private string ApisPath { get; } = Path.Combine(options.Output, "apis");

    public async Task<int> Run()
    {
        if (!Directory.Exists(options.Output))
        {
            Directory.CreateDirectory(options.Output);
        }

        try
        {
            if (Directory.Exists(ApisPath))
            {
                Directory.Delete(ApisPath, true);
            }
        }
        catch (IOException ex)
        {
            Console.WriteLine($"can not delete directory {ApisPath}: {ex.Message}");
        }
        catch (UnauthorizedAccessException ex)
        {
            Console.WriteLine($"not enough permission {ApisPath}: {ex.Message}");
        }

        Directory.CreateDirectory(ApisPath);

        DocfxInit();
        var luaWorkspace = LoadLuaWorkspace();
        GenerateApis(luaWorkspace);
        await Docfx.Docset.Build(Path.Combine(options.Output, "docfx.json"));

        return 0;
    }

    private void DocfxInit()
    {
        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Template", "docfx.json");
        var docfxTemplate = File.ReadAllText(path);
        var docfxJson = docfxTemplate.Replace("{projectName}", options.ProjectName);
        File.WriteAllText(Path.Combine(options.Output, "docfx.json"), docfxJson);
        File.WriteAllText(Path.Combine(options.Output, "index.md"), $"# {options.ProjectName}\n");
        GenerateToc(options.Output, new List<TocItem>()
        {
            new TocItem()
            {
                Name = "Apis",
                Href = "apis/"
            }
        });
    }

    private LuaWorkspace LoadLuaWorkspace()
    {
        var workspacePath = options.Workspace;
        var settingManager = new SettingManager();
        settingManager.LoadSetting(workspacePath);
        return LuaWorkspace.Create(workspacePath, settingManager.GetLuaFeatures());
    }

    private void GenerateApis(LuaWorkspace luaWorkspace)
    {
        var tocItems = new List<TocItem>();
        foreach (var module in luaWorkspace.ModuleGraph.GetAllModules())
        {
            if (module.Workspace == luaWorkspace.MainWorkspace)
            {
                var renderer = new ModuleDoc(luaWorkspace.Compilation, module);
                var text = renderer.Build();
                var fileName = $"{module.ModulePath}.md";
                tocItems.Add(new TocItem()
                {
                    Name = module.ModulePath,
                    Href = fileName
                });
                File.WriteAllText(Path.Combine(ApisPath, fileName), text);
            }
        }

        GenerateToc(ApisPath, tocItems);
    }

    private void GenerateToc(string path, List<TocItem> tocItems)
    {
        var serializer = new SerializerBuilder().Build();
        var yaml = serializer.Serialize(tocItems);
        File.WriteAllText(Path.Combine(path, "toc.yml"), yaml);
    }
}