// using LuaLanguageServer.LanguageServer;

// var server = new LanguageServer();
// await server.StartAsync(args);

using LuaLanguageServer.CodeAnalysis.Syntax.Tree;

var tree = LuaSyntaxTree.ParseText(
    """
    ---@class t
    local t = 123
    print(1231,242,24,123)

    ---@param a number
    ---@return number
    function ff(a)
    end
    """);

Console.Write(tree.SyntaxRoot.DebugInspect());
