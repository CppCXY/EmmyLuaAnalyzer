using CommandLine;

namespace EmmyLua.Cli.Linter;

public class CheckOptions
{
    [Option('w', "workspace", Required = true, HelpText = "Workspace directory")]
    public string Workspace { get; set; } = string.Empty;
}