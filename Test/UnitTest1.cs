using EmmyLua.CodeAnalysis.Compile;
using EmmyLua.CodeAnalysis.Syntax.Tree;
using EmmyLua.CodeAnalysis.Workspace;

namespace Test;

public class Tests
{
    [SetUp]
    public void Setup()
    {
    }

    [Test]
    public void Test1()
    {
        var workspace = LuaWorkspace.Create();
        workspace.AddDocument(LuaDocument.FromText(
            """
            ---@return fun(a,b,c):number, string
            function pairs(a)
            end

            for _, a in pairs({}) do
                print(a)
            end
            """, workspace.Features.Language));
    }
    
}