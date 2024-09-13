using EmmyLua.CodeAnalysis.Compilation.Scope;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Declaration;

public record LuaDeclarationTree(
    Dictionary<SyntaxElementId, DeclarationScope> Scopes,
    DeclarationScope Root,
    Dictionary<SyntaxElementId, LuaSymbol> Declarations,
    Dictionary<SyntaxElementId, LuaElementPtr<LuaClosureExprSyntax>> RelatedClosure,
    Dictionary<SyntaxElementId, List<LuaElementPtr<LuaDocTagSyntax>>> AttachedDocs
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

    public LuaSymbol? FindLocalSymbol(LuaSyntaxElement element)
    {
        return Declarations.GetValueOrDefault(element.UniqueId);
    }

    public LuaElementPtr<LuaClosureExprSyntax>? GetElementRelatedClosure(LuaSyntaxElement element)
    {
        return RelatedClosure.GetValueOrDefault(element.UniqueId);
    }
}
