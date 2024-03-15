using EmmyLua.CodeAnalysis.Compile;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Workspace;

var workspace = LuaWorkspace.Create();
var document = LuaDocument.FromText(
    """
    ---@enum A : B @好了好了
    ---| yes @是的
    ---| no @不是
    local c = {
        aaa = 123
    }
    print(c.aaa)
    if c.aaa then
        return x
    end
    
    
    yes.it = 123
    """, new LuaLanguage());
workspace.AddDocument(document);
    
Console.WriteLine(document.SyntaxTree.SyntaxRoot.DebugSyntaxInspect());

var diagnostics = workspace.Compilation.GetDiagnostic(document.Id);
foreach (var diagnostic in diagnostics)
{
    Console.WriteLine(diagnostic);
}