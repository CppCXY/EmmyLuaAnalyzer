using LuaLanguageServer.LuaCore.Syntax.Tree;

var tree = LuaSyntaxTree.ParseText(
    """
    --- 你说的对但是__
    ---@class A {a :number} # 1231313
    local t= 123
    """);


Console.Write(tree.GreenRoot);
