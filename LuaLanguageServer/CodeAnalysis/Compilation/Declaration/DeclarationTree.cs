using LuaLanguageServer.CodeAnalysis.Syntax.Node;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;
using LuaLanguageServer.CodeAnalysis.Syntax.Tree;
using LuaLanguageServer.CodeAnalysis.Syntax.Walker;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Declaration;

public class DeclarationTree : ILuaNodeWalker
{
    public LuaSyntaxTree LuaSyntaxTree { get; }

    private Stack<DeclarationScope> _scopes = new();

    private Dictionary<LuaSyntaxElement, DeclarationScope> _scopeOwners = new();

    private DeclarationScope? _topScope = null;

    private DeclarationScope? _curScope = null;

    public static DeclarationTree From(LuaSyntaxTree tree)
    {
        var declarationTree = new DeclarationTree(tree);
        declarationTree.Build();
        return declarationTree;
    }

    private DeclarationTree(LuaSyntaxTree tree)
    {
        LuaSyntaxTree = tree;
    }

    public int GetPosition(LuaSyntaxElement element) => element.Green.Range.StartOffset;

    private Declaration CreateDeclaration(string name, LuaSyntaxElement element, DeclarationFlag flag)
    {
        var first = element switch
        {
            LuaExprSyntax exprSyntax => Find(exprSyntax),
            _ => null
        };
        return new Declaration(name, GetPosition(element), element, flag, _curScope, first);
    }

    public Declaration? Find(LuaExprSyntax exprSyntax)
    {
        if (exprSyntax is LuaIndexExprSyntax or LuaNameExprSyntax)
        {
            var scope = FindScope(exprSyntax);
            if (scope != null)
            {
                return scope.Find(exprSyntax)?.FirstDeclaration;
            }
        }

        return null;
    }

    private void Build()
    {
        _scopes.Clear();
        LuaSyntaxTree.SyntaxRoot.Accept(this);
    }

    private DeclarationScope Push(LuaSyntaxElement element)
    {
        var position = GetPosition(element);
        return element switch
        {
            LuaLocalStatSyntax => Push(new LocalStatDeclarationScope(this, position, _curScope),
                element),
            LuaRepeatStatSyntax => Push(new RepeatStatDeclarationScope(this, position, _curScope),
                element),
            LuaForRangeStatSyntax => Push(
                new ForRangeStatDeclarationScope(this, position, _curScope), element),
            _ => Push(new DeclarationScope(this, position, _curScope), element)
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
            _curScope = _scopes.Peek();
        }
        else
        {
            _curScope = _topScope;
        }
    }

    public DeclarationScope? FindScope(LuaSyntaxElement element)
    {
        var cur = element;
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

    public void WalkUp(LuaSyntaxElement element, Func<Declaration, bool> process)
    {
        var scope = FindScope(element);
        scope?.WalkUp(GetPosition(element), 0, process);
    }

    public void WalkUpLocal(LuaSyntaxElement element, Func<Declaration, bool> process)
    {
        WalkUp(element, declaration =>
        {
            if (declaration.IsLocal)
            {
                return process(declaration);
            }

            return true;
        });
    }

    private static bool IsScopeOwner(LuaSyntaxNode node)
        => node is LuaBlockSyntax or LuaFuncStatSyntax or LuaRepeatStatSyntax or LuaForRangeStatSyntax
            or LuaForStatSyntax;

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
                if (funcStatSyntax is {IsLocal: true, Name: { } name})
                {
                    var declaration = CreateDeclaration(name.RepresentText, funcStatSyntax,
                        DeclarationFlag.Local | DeclarationFlag.Function);
                    _curScope?.Add(declaration);
                }
                else if (funcStatSyntax is {IsMethod: true, MethodName: { } methodName, Name: { } name2})
                {
                    var declaration = CreateDeclaration(name2.RepresentText, funcStatSyntax,
                        DeclarationFlag.ClassMember | DeclarationFlag.Function);
                    if (methodName.PrefixExpr is { } parentExpr)
                    {
                        Find(parentExpr)?.AddField(declaration);
                    }
                }

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
                                var flags = Find(varExpr)?.Flags ?? DeclarationFlag.Global;
                                var declaration = CreateDeclaration(name.RepresentText, nameExpr,
                                    flags);
                                _curScope?.Add(declaration);
                            }

                            break;
                        }
                        case LuaIndexExprSyntax indexExpr:
                        {
                            if (indexExpr.Name is { } name)
                            {
                                var declaration = _curScope?.Find(indexExpr.PrefixExpr);
                                declaration?.AddField(
                                    CreateDeclaration(name.RepresentText, indexExpr, DeclarationFlag.ClassMember)
                                );
                            }

                            break;
                        }
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
}
