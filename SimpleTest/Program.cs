using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Compile;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Workspace;

// var workspace = LuaWorkspace.Create();
var document = LuaDocument.FromText(
    """
    ---@alias AAA
    ---| "nonono" #好吧好吧
    """, new LuaLanguage(LuaLanguageLevel.LuaLatest));
// workspace.AddDocument(document);
    
Console.WriteLine(document.SyntaxTree.SyntaxRoot.DebugSyntaxInspect());

// var diagnostics = workspace.Compilation.GetDiagnostic(document.Id);
// foreach (var diagnostic in diagnostics)
// {
//     Console.WriteLine(diagnostic);
// }

