using EmmyLua.CodeAnalysis.Compile;
using EmmyLua.CodeAnalysis.Workspace;

var document = LuaDocument.FromText(
    """
    ---@param b number
    ---@param a string
    """, new LuaLanguage());
var tree = document.SyntaxTree;
Console.Write(tree.SyntaxRoot.DebugSyntaxInspect());