using System.Diagnostics;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Compile;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Workspace;

var stopwatch = Stopwatch.StartNew();

var workspace = LuaWorkspace.Create();
// var _ = workspace.Compilation.GetAllDiagnosticsParallel().ToList();

stopwatch.Stop();

var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

Console.WriteLine($"Elapsed time: {elapsedMilliseconds} ms");
// var document = LuaDocument.FromText(
//     """
//     ---@alias AAA
//     ---| "nonono" #好吧好吧
//     """, new LuaLanguage(LuaLanguageLevel.LuaLatest));
// workspace.AddDocument(document);
    
// Console.WriteLine(document.SyntaxTree.SyntaxRoot.DebugSyntaxInspect());

// foreach (var diagnostic in diagnostics)
// {
//     Console.WriteLine(diagnostic);
// }

