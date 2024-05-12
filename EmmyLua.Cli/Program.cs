using CommandLine;
using EmmyLua.Cli.DocGenerator;
using EmmyLua.Cli.Linter;

if (args.Length == 0)
{
    Console.WriteLine("Please enter a target: doc/check");
}

switch (args.First())
{
    case "doc":
    {
        Parser.Default
            .ParseArguments<DocOptions>(args.Skip(1))
            .WithParsed<DocOptions>(o =>
            {
                var docGenerator = new DocGenerator(o);
                docGenerator.Generate();
            });
        break;
    }
    case "check":
    {
        Parser.Default
            .ParseArguments<CheckOptions>(args.Skip(1))
            .WithParsed<CheckOptions>(o =>
            {
                var check = new Linter(o);
                var exitCode = check.Run();
                Environment.Exit(exitCode);
            });
        break;
    }
    default:
    {
        Console.WriteLine("Unknown target");
        break;
    }
}