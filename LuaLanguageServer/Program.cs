// using LuaLanguageServer.LanguageServer;

// var server = new LanguageServer();
// await server.StartAsync(args);

using System.Diagnostics;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;
using LuaLanguageServer.CodeAnalysis.Syntax.Tree;
using LuaLanguageServer.CodeAnalysis.Workspace;

var tree = LuaSyntaxTree.ParseText(
    """
    ---如来
    ---如来他来了吗
    ---@class A {
    ---     a: number
    --- } # 如来他来了吗
    ---@field a
    --- number # 好的呢
    """);

Console.Write(tree.SyntaxRoot.DebugGreenInspect());
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
