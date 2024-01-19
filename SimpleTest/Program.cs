using EmmyLua.CodeAnalysis.Compile;
using EmmyLua.CodeAnalysis.Workspace;

var document = LuaDocument.FromText(
    """
    ---@param b number
    ---@param a string
    ---@return fun(a,b,c):number, string
    function pairs(a, b)
    end
    
    pairs({})
    """, new LuaLanguage());
var tree = document.SyntaxTree;
Console.Write(tree.SyntaxRoot.DebugSyntaxInspect());