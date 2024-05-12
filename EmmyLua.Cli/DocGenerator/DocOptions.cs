using CommandLine;

namespace EmmyLua.Cli.DocGenerator;

public class DocOptions
{
    [Option('w', "workspace", Required = true, HelpText = "Workspace directory")]
    public string Workspace { get; set; } = string.Empty;

    [Option('o', "output", Required = true, HelpText = "Output directory")]
    public string Output { get; set; } = string.Empty;
}