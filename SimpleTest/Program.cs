using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Compile;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Workspace;

var workspace = LuaWorkspace.Create();
var document = LuaDocument.FromText(
    """
    ---你说的对
    ---但是元神
    ---
    ---可是雪啊
    ---@param a number @我懂了啊
    local t = {}
    """, new LuaLanguage());
workspace.AddDocument(document);
    
Console.WriteLine(document.SyntaxTree.SyntaxRoot.DebugSyntaxInspect());

var diagnostics = workspace.Compilation.GetDiagnostic(document.Id);
foreach (var diagnostic in diagnostics)
{
    Console.WriteLine(diagnostic);
}

