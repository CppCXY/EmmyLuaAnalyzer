using CommandLine;

namespace EmmyLua.Cli.Linter;

// ReSharper disable once ClassNeverInstantiated.Global
public class CheckOptions
{
    [Option('w', "workspace", Required = true, HelpText = "Workspace directory")]
    public string Workspace { get; set; } = string.Empty;
}