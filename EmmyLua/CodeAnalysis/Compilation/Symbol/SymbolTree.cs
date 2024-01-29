using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Syntax.Tree;

namespace EmmyLua.CodeAnalysis.Compilation.Symbol;

public class SymbolTree(LuaSyntaxTree tree, IReadOnlyDictionary<LuaSyntaxElement, SymbolScope> scopeOwners)
{
    public LuaSyntaxTree SyntaxTree { get; } = tree;

    public SymbolScope? RootScope { get; internal set; }

    public int GetPosition(LuaSyntaxElement element) => element.Range.StartOffset;

    public Symbol? FindDeclaration(LuaSyntaxElement element, SearchContext context)
    {
        switch (element)
        {
            case LuaNameExprSyntax nameExpr:
            {
                if (nameExpr.Name is { } name)
                {
                    var scope = FindScope(nameExpr);
                    var symbol = scope?.FindNameExpr(nameExpr);
                    if (symbol is not null)
                    {
                        return symbol;
                    }
                    return context.Compilation.Stub.GlobalDeclaration
                        .Get(name.RepresentText).FirstOrDefault();;
                }

                break;
            }
            case LuaParamDefSyntax paramDef:
            {
                var scope = FindScope(paramDef);
                return scope?.FindParamDef(paramDef);
            }
            case LuaLocalNameSyntax localName:
            {
                var scope = FindScope(localName);
                return scope?.FindLocalName(localName);
            }
            case LuaIndexExprSyntax indexExpr:
            {
                return FindIndexDeclaration(indexExpr, context);
            }
        }

        return null;
    }

    private Symbol? FindIndexDeclaration(LuaIndexExprSyntax indexExpr, SearchContext context)
    {
        if (indexExpr.PrefixExpr is { } prefixExpr)
        {
            var prefixType = context.Infer(prefixExpr);
            switch (indexExpr)
            {
                case { DotOrColonIndexName: { } nameToken }:
                    return prefixType.IndexMember(nameToken.RepresentText, context).FirstOrDefault();
                case { IndexKeyExpr: LuaLiteralExprSyntax literal }:
                    return literal.Literal switch
                    {
                        LuaStringToken stringToken => prefixType.IndexMember(stringToken.Value, context).FirstOrDefault(),
                        LuaIntegerToken luaIntegerToken => prefixType.IndexMember(luaIntegerToken.Value, context)
                            .FirstOrDefault(),
                        _ => prefixType.IndexMember(literal.Literal.RepresentText, context).FirstOrDefault()
                    };
                case { IndexKeyExpr: { } expr }:
                {
                    var indexType = context.Infer(expr);
                    return prefixType.IndexMember(indexType, context).FirstOrDefault();
                }
            }
        }

        return null;
    }

    public SymbolScope? FindScope(LuaSyntaxElement element)
    {
        var cur = element;
        while (cur != null)
        {
            if (scopeOwners.TryGetValue(cur, out var scope))
            {
                return scope;
            }

            cur = cur.Parent;
        }

        return null;
    }

    public IEnumerable<Symbol> Symbols => RootScope?.Descendants ?? Enumerable.Empty<Symbol>();
}
