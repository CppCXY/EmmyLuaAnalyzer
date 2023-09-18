// using LuaLanguageServer.LanguageServer;

// var server = new LanguageServer();
// await server.StartAsync(args);

using System.Diagnostics;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;
using LuaLanguageServer.CodeAnalysis.Syntax.Tree;
using LuaLanguageServer.CodeAnalysis.Workspace;

var tree = LuaSyntaxTree.ParseText(
    """
    ---@enum ys
    ---| 你说的对 # The left side of the device
    ---| 但是 # The right side of the device
    ---| ys # The top side of the device
    """);

Console.Write(tree.SyntaxRoot.DebugGreenInspect());
foreach (var diagnostic in tree.Diagnostics)
{
    Console.WriteLine(diagnostic);
}
// var block = tree.SyntaxRoot.Descendants.OfType<LuaCallExprSyntax>().FirstOrDefault();
// if (block != null)
// {
//     foreach (var commentSyntax in block.Comments)
//     {
//         Console.WriteLine(commentSyntax);
//     }
// }

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
