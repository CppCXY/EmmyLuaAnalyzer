using EmmyLua.CodeAnalysis.Compile;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Workspace;

var workspace = LuaWorkspace.Create();
var document = LuaDocument.FromText(
    """
    ---@param a? string
    function f(a, b, c)
        print(a, b, c)
    end
    
    f(nil, 2, 3)
    """, new LuaLanguage());
workspace.AddDocument(document);
    
Console.WriteLine(document.SyntaxTree.SyntaxRoot.DebugSyntaxInspect());

var diagnostics = workspace.Compilation.GetDiagnostic(document.Id);
foreach (var diagnostic in diagnostics)
{
    Console.WriteLine(diagnostic);
}