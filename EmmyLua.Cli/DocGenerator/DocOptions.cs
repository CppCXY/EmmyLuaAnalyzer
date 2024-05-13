using CommandLine;

namespace EmmyLua.Cli.DocGenerator;

public class DocOptions
{
    [Option('w', "workspace", Required = true, HelpText = "Workspace directory")]
    public string Workspace { get; set; } = string.Empty;

    [Option('p', "project", Required = true, HelpText = "Project name")]
    public string ProjectName { get; set; } = string.Empty;
    
    [Option('o', "output", Required = true, HelpText = "Output directory")]
    public string Output { get; set; } = string.Empty;

    [Option('d', "docs", Required = false, HelpText = "Docs directory")]
    public string DocsPath { get; set; } = "docs";
}