// using LuaLanguageServer.LanguageServer;

// var server = new LanguageServer();
// await server.StartAsync(args);

using System.Diagnostics;
using LuaLanguageServer.CodeAnalysis.Syntax.Tree;
using LuaLanguageServer.CodeAnalysis.Workspace;

var tree = LuaSyntaxTree.ParseText(
    """
    return "\n";
    """);

// Console.Write(tree.SyntaxRoot.DebugSyntaxInspect());
foreach(var diagnostic in tree.Diagnostics)
{
    Console.WriteLine(diagnostic);
}

// 计算执行时间
// var sw = new Stopwatch();
// sw.Start();
// var w = LuaWorkspace.Create("");
//
// sw.Stop();
// Console.WriteLine($"耗时: {sw.ElapsedMilliseconds} ms");
//
// var compilation = w.Compilation;
// foreach(var diagnostic in compilation.GetDiagnostics(1))
// {
//     Console.WriteLine(diagnostic);
// }
