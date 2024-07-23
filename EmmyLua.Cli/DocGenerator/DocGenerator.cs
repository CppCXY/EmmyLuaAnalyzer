using Docfx;
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
        var rootTocItems = new List<TocItem>();
        DocfxInit();

        CopyReadmeAndDocs(rootTocItems);
        GenerateApis(rootTocItems);
        GenerateToc(options.Output, rootTocItems);

        await Docset.Build(Path.Combine(options.Output, "docfx.json"));
        return 0;
    }

    private void DocfxInit()
    {
        if (!Directory.Exists(options.Output))
        {
            Directory.CreateDirectory(options.Output);
        }

        if (!Directory.Exists(ApisPath))
        {
            Directory.CreateDirectory(ApisPath);
        }

        var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Template", "docfx.json");
        var docfxTemplate = File.ReadAllText(path);
        var docfxJson = docfxTemplate.Replace("{projectName}", options.ProjectName);
        File.WriteAllText(Path.Combine(options.Output, "docfx.json"), docfxJson);
    }

    private void CopyReadmeAndDocs(List<TocItem> rootTocItems)
    {
        rootTocItems.Add(new TocItem()
        {
            Name = "Home",
            Href = "docs/"
        });

        var rootReadme = Path.Combine(options.Workspace, "README.md");
        if (File.Exists(rootReadme))
        {
            File.Copy(rootReadme, Path.Combine(options.Output, "index.md"));
        }

        if (Directory.Exists(options.DocsPath))
        {
            if (!Directory.Exists(Path.Combine(options.Output, "docs")))
            {
                Directory.CreateDirectory(Path.Combine(options.Output, "docs"));
            }

            var tocItems = new List<TocItem>();
            foreach (var file in Directory.EnumerateFiles(options.DocsPath))
            {
                var fileName = Path.GetFileName(file);
                File.Copy(file, Path.Combine(options.Output, "docs", fileName));
                tocItems.Add(new TocItem()
                {
                    Name = fileName,
                    Href = fileName
                });
            }

            GenerateToc(Path.Combine(options.Output, "docs"), tocItems);
        }
    }

    private LuaProject LoadLuaWorkspace()
    {
        var workspacePath = options.Workspace;
        var settingManager = new SettingManager();
        settingManager.LoadSetting(workspacePath);
        return LuaProject.Create(workspacePath, settingManager.GetLuaFeatures());
    }

    // TODO: Generate APIs
    private void GenerateApis(List<TocItem> rootTocItems)
    {
        // var luaWorkspace = LoadLuaWorkspace();
        // var tocItems = new List<TocItem>();
        // foreach (var module in luaWorkspace.ModuleManager.GetAllModules())
        // {
        //     if (module.Workspace == luaWorkspace.MainWorkspacePath)
        //     {
        //         var renderer = new ModuleDoc(luaWorkspace.Compilation, module);
        //         var text = renderer.Build();
        //         var fileName = $"{module.ModulePath}.md";
        //         tocItems.Add(new TocItem()
        //         {
        //             Name = module.ModulePath,
        //             Href = fileName
        //         });
        //         File.WriteAllText(Path.Combine(ApisPath, fileName), text);
        //     }
        // }
        //
        // rootTocItems.Add(new TocItem()
        // {
        //     Name = "APIs",
        //     Href = "apis/"
        // });
        // GenerateToc(ApisPath, tocItems);
    }

    private void GenerateToc(string path, List<TocItem> tocItems)
    {
        var serializer = new SerializerBuilder().Build();
        var yaml = serializer.Serialize(tocItems);
        File.WriteAllText(Path.Combine(path, "toc.yml"), yaml);
    }
}