using System.Diagnostics;
using EmmyLua.CodeAnalysis.Document;


var stopwatch = Stopwatch.StartNew();

// var workspace = LuaWorkspace.Create();



stopwatch.Stop();

var elapsedMilliseconds = stopwatch.ElapsedMilliseconds;

Console.WriteLine($"Elapsed time: {elapsedMilliseconds} ms");
var document = LuaDocument.FromText(
    """
    call(-1)
    """, new LuaLanguage(LuaLanguageLevel.LuaLatest));
// workspace.AddDocument(document);
//     
Console.WriteLine(document.SyntaxTree.SyntaxRoot.DebugSyntaxInspect());
//
// foreach (var diagnostic in workspace.Compilation.GetAllDiagnostics())
// {
//     Console.WriteLine(diagnostic);
// }

