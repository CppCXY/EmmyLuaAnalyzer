using LuaLanguageServer.CodeAnalysis.Syntax.Node;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;
using LuaLanguageServer.CodeAnalysis.Syntax.Tree;
using LuaLanguageServer.CodeAnalysis.Syntax.Walker;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Declaration;

public class DeclarationTreeBuilder : ILuaNodeWalker
{
    private DeclarationScope? _topScope = null;

    private DeclarationScope? _curScope = null;

    private Stack<DeclarationScope> _scopes = new();

    private Dictionary<LuaSyntaxElement, DeclarationScope> _scopeOwners = new();

    private DeclarationTree _tree;

    public static DeclarationTree Build(LuaSyntaxTree tree)
    {
        var builder = new DeclarationTreeBuilder(tree);
        tree.SyntaxRoot.Accept(builder);
        return builder._tree;
    }

    public DeclarationTreeBuilder(LuaSyntaxTree tree)
    {
        _tree = new DeclarationTree(tree, _scopeOwners);
    }

    private Declaration? FindNameExpr(LuaNameExprSyntax nameExpr)
    {
        return FindScope(nameExpr)?.FindNameExpr(nameExpr)?.FirstDeclaration;
    }

    private DeclarationScope? FindScope(LuaSyntaxNode element)
    {
        LuaSyntaxElement? cur = element;
        while (cur != null)
        {
            if (_scopeOwners.TryGetValue(cur, out var scope))
            {
                return scope;
            }

            cur = cur.Parent;
        }

        return null;
    }

    private int GetPosition(LuaSyntaxElement element) => element.Green.Range.StartOffset;

    private Declaration CreateDeclaration(string name, LuaSyntaxElement element, DeclarationFlag flag)
    {
        var first = element switch
        {
            LuaNameExprSyntax nameExpr => FindNameExpr(nameExpr),
            _ => null
        };
        return new Declaration(name, GetPosition(element), element, flag, _curScope, first);
    }

    private DeclarationScope Push(LuaSyntaxElement element)
    {
        var position = GetPosition(element);
        return element switch
        {
            LuaLocalStatSyntax => Push(new LocalStatDeclarationScope(_tree, position, _curScope),
                element),
            LuaRepeatStatSyntax => Push(new RepeatStatDeclarationScope(_tree, position, _curScope),
                element),
            LuaForRangeStatSyntax => Push(new ForRangeStatDeclarationScope(_tree, position, _curScope), element),
            _ => Push(new DeclarationScope(_tree, position, _curScope), element)
        };
    }

    private DeclarationScope Push(DeclarationScope scope, LuaSyntaxElement element)
    {
        _scopes.Push(scope);
        _topScope ??= scope;
        _scopeOwners.Add(element, scope);
        _curScope?.Add(scope);
        _curScope = scope;
        return scope;
    }

    private void Pop()
    {
        if (_scopes.Count != 0)
        {
            _scopes.Pop();
        }

        _curScope = _scopes.Count != 0 ? _scopes.Peek() : _topScope;
    }

    public void WalkIn(LuaSyntaxNode node)
    {
        switch (node)
        {
            case LuaLocalNameSyntax localNameSyntax:
            {
                if (localNameSyntax.Name is { } name)
                {
                    var declaration = CreateDeclaration(name.RepresentText, localNameSyntax, DeclarationFlag.Local);
                    _curScope?.Add(declaration);
                }

                break;
            }
            case LuaParamListSyntax paramListSyntax:
            {
                foreach (var param in paramListSyntax.Params)
                {
                    if (param.Name is { } name)
                    {
                        var declaration = CreateDeclaration(name.RepresentText, param,
                            DeclarationFlag.Local);
                        _curScope?.Add(declaration);
                    }
                }

                break;
            }
            case LuaFuncStatSyntax funcStatSyntax:
            {
                if (funcStatSyntax is { IsLocal: true, LocalName.Name: { } name })
                {
                    var declaration = CreateDeclaration(name.RepresentText, funcStatSyntax,
                        DeclarationFlag.Local | DeclarationFlag.Function);
                    _curScope?.Add(declaration);
                }
                // TODO global or redefine function
                // else if (funcStatSyntax is { IsMethod: true, MethodName: { } methodName, Name: { } name2 })
                // {
                //     var declaration = CreateDeclaration(name2.RepresentText, funcStatSyntax,
                //         DeclarationFlag.ClassMember | DeclarationFlag.Function);
                //     if (methodName.PrefixExpr is { } parentExpr)
                //     {
                //         FindNameExpr(parentExpr)?.AddField(declaration);
                //     }
                //     else
                //     {
                //
                //     }
                // }

                break;
            }
            case LuaAssignStatSyntax assignStatSyntax:
            {
                foreach (var varExpr in assignStatSyntax.VarList)
                {
                    switch (varExpr)
                    {
                        case LuaNameExprSyntax nameExpr:
                        {
                            if (nameExpr.Name is { } name)
                            {
                                var flags = FindNameExpr(nameExpr)?.Flags ?? DeclarationFlag.Global;
                                var declaration = CreateDeclaration(name.RepresentText, nameExpr, flags);
                                _curScope?.Add(declaration);
                            }

                            break;
                        }
                        // TODO
                        // case LuaIndexExprSyntax indexExpr:
                        // {
                        //     // if (indexExpr.Name is { } name)
                        //     // {
                        //     //     var declaration = _curScope?.Find(indexExpr.PrefixExpr);
                        //     //     declaration?.AddField(
                        //     //         CreateDeclaration(name.RepresentText, indexExpr, DeclarationFlag.ClassMember)
                        //     //     );
                        //     // }
                        //
                        //     break;
                        // }
                    }
                }

                break;
            }
        }

        if (IsScopeOwner(node))
        {
            Push(node);
        }
    }

    public void WalkOut(LuaSyntaxNode node)
    {
        if (IsScopeOwner(node))
        {
            Pop();
        }
    }

    private static bool IsScopeOwner(LuaSyntaxNode node)
        => node is LuaBlockSyntax or LuaFuncStatSyntax or LuaRepeatStatSyntax or LuaForRangeStatSyntax
            or LuaForStatSyntax;
}
