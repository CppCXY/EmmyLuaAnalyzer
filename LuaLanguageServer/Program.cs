// using LuaLanguageServer.LanguageServer;

// var server = new LanguageServer();
// await server.StartAsync(args);

using System.Diagnostics;
using LuaLanguageServer.CodeAnalysis.Compilation.Declaration;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;
using LuaLanguageServer.CodeAnalysis.Syntax.Tree;
using LuaLanguageServer.CodeAnalysis.Workspace;

var tree = LuaSyntaxTree.ParseText(
    """
    d = [[danshi
        t = [[你说的对]]
    e = "但是yx"
    f = "可是啊"

    local t = [==[afafa]==]
    """);
Console.WriteLine(tree.SyntaxRoot.DebugSyntaxInspect());

// var dtree = DeclarationTree.From(tree);
// var tk = tree.SyntaxRoot.NodeAt(1, 0);
// if (tk is LuaNameExprSyntax name)
// {
//     var declaration = dtree.FindDeclaration(name);
//     Console.Write(declaration);
// }

// Console.Write(tree.SyntaxRoot.DebugSyntaxInspect());
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
