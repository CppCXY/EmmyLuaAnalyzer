using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Compile;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Workspace;

var workspace = LuaWorkspace.Create();
var document = LuaDocument.FromText(
    """
    ---@class A<T>
    ---@field x T
    local t = {}
    
    ---@type A<number>
    local d = 123
    
    """, new LuaLanguage());
workspace.AddDocument(document);
    
Console.WriteLine(document.SyntaxTree.SyntaxRoot.DebugSyntaxInspect());

var diagnostics = workspace.Compilation.GetDiagnostic(document.Id);
foreach (var diagnostic in diagnostics)
{
    Console.WriteLine(diagnostic);
}

