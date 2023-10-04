using LuaLanguageServer.CodeAnalysis.Syntax.Node;

namespace LuaLanguageServer.CodeAnalysis.Syntax.Walker;

public interface ILuaNodeWalker
{
    public void WalkIn(LuaSyntaxNode node);

    public void WalkOut(LuaSyntaxNode node);
}

public interface ILuaElementWalker
{
    public void WalkIn(LuaSyntaxElement element);

    public void WalkOut(LuaSyntaxElement element);
}
