using EmmyLua.CodeAnalysis.Compilation.Scope;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Compilation.Type.Types;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Declaration;

public record LuaDeclarationTree(
    Dictionary<SyntaxElementId, DeclarationScope> Scopes,
    DeclarationScope Root
)
{
    public DeclarationScope? FindScope(LuaSyntaxElement element)
    {
        LuaSyntaxElement? cur = element;
        while (cur != null)
        {
            if (Scopes.TryGetValue(cur.UniqueId, out var scope))
            {
                return scope;
            }

            cur = cur.Parent;
        }

        return null;
    }
}
