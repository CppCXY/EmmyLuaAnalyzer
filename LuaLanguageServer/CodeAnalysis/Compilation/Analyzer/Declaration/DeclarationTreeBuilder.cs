using LuaLanguageServer.CodeAnalysis.Compilation.Type;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;
using LuaLanguageServer.CodeAnalysis.Syntax.Tree;
using LuaLanguageServer.CodeAnalysis.Syntax.Walker;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Declaration;

public class DeclarationTreeBuilder : ILuaElementWalker
{
    private DeclarationScope? _topScope = null;

    private DeclarationScope? _curScope = null;

    private Stack<DeclarationScope> _scopes = new();

    private Dictionary<LuaSyntaxElement, DeclarationScope> _scopeOwners = new();

    private DeclarationTree _tree;

    private Dictionary<string, ILuaType> _typeDeclarations = new();

    private LuaAnalyzer Analyzer { get; }

    public static DeclarationTree Build(LuaSyntaxTree tree, LuaAnalyzer analyzer)
    {
        var builder = new DeclarationTreeBuilder(tree, analyzer);
        tree.SyntaxRoot.Accept(builder);
        return builder._tree;
    }

    private DeclarationTreeBuilder(LuaSyntaxTree tree, LuaAnalyzer analyzer)
    {
        _tree = new DeclarationTree(tree, _scopeOwners);
        Analyzer = analyzer;
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

    private Declaration CreateDeclaration(string name, LuaSyntaxElement element, DeclarationFlag flag,
        ILuaType? luaType)
    {
        var first = element switch
        {
            LuaNameExprSyntax nameExpr => FindNameExpr(nameExpr),
            _ => null
        };
        return new Declaration(name, GetPosition(element), element, flag, _curScope, first, luaType);
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

    public void WalkIn(LuaSyntaxElement node)
    {
        if (IsScopeOwner(node))
        {
            Push(node);
        }

        switch (node)
        {
            case LuaLocalStatSyntax localStatSyntax:
            {
                LocalStatDeclarationAnalysis(localStatSyntax);
                break;
            }
            case LuaParamListSyntax paramListSyntax:
            {
                var dic = FindParamListDeclaration(paramListSyntax);
                foreach (var param in paramListSyntax.Params)
                {
                    if (param.Name is { } name)
                    {
                        var declaration = CreateDeclaration(name.RepresentText, param, DeclarationFlag.Local,
                            dic.GetValueOrDefault(name.RepresentText));
                        _curScope?.Add(declaration);
                    }
                }

                break;
            }
            case LuaForRangeStatSyntax forRangeStatSyntax:
            {
                var dic = FindParamListDeclaration(forRangeStatSyntax);
                foreach (var param in forRangeStatSyntax.IteratorNames)
                {
                    if (param.Name is { } name)
                    {
                        var declaration = CreateDeclaration(name.RepresentText, param, DeclarationFlag.Local,
                            dic.GetValueOrDefault(name.RepresentText));
                        _curScope?.Add(declaration);
                    }
                }

                break;
            }
            case LuaForStatSyntax forStatSyntax:
            {
                if (forStatSyntax.IteratorName is { Name: { } name })
                {
                    var declaration = CreateDeclaration(name.RepresentText, name, DeclarationFlag.Local,
                        Analyzer.Compilation.Builtin.Integer);
                    _curScope?.Add(declaration);
                }

                break;
            }
            case LuaFuncStatSyntax funcStatSyntax:
            {
                MethodDeclarationAnalysis(funcStatSyntax);
                break;
            }
            case LuaAssignStatSyntax assignStatSyntax:
            {
                AssignStatDeclarationAnalysis(assignStatSyntax);
                break;
            }
            case LuaDocTagClassSyntax tagClassSyntax:
            {
                if (tagClassSyntax is { Name: { } name })
                {
                    var declaration = CreateDeclaration(name.RepresentText, tagClassSyntax,
                        DeclarationFlag.TypeDeclaration, null);
                    _curScope?.Add(declaration);
                    _typeDeclarations.Add(name.RepresentText, new LuaTypeRef(tagClassSyntax));
                }

                break;
            }
            case LuaDocTagAliasSyntax tagAliasSyntax:
            {
                if (tagAliasSyntax is { Name: { } name })
                {
                    var declaration = CreateDeclaration(name.RepresentText, tagAliasSyntax,
                        DeclarationFlag.TypeDeclaration, null);
                    _curScope?.Add(declaration);
                    _typeDeclarations.Add(name.RepresentText, new LuaTypeRef(tagAliasSyntax));
                }

                break;
            }
        }
    }

    public void WalkOut(LuaSyntaxElement node)
    {
        if (IsScopeOwner(node))
        {
            Pop();
        }
    }

    private static bool IsScopeOwner(LuaSyntaxElement node)
        => node is LuaBlockSyntax or LuaFuncBodySyntax or LuaRepeatStatSyntax or LuaForRangeStatSyntax
            or LuaForStatSyntax;

    private void LocalStatDeclarationAnalysis(LuaLocalStatSyntax localStatSyntax)
    {
        var typeDeclarations = FindLocalOrAssignTagDeclaration(localStatSyntax);
        var nameList = localStatSyntax.NameList.ToList();
        var count = nameList.Count;
        for (var i = 0; i < count; i++)
        {
            var localName = nameList[i];

            var luaType = typeDeclarations.ElementAtOrDefault(i);
            if (localName is { Name: { } name })
            {
                var declaration = CreateDeclaration(name.RepresentText, localName, DeclarationFlag.Local, luaType);
                _curScope?.Add(declaration);
            }
        }
    }

    private List<ILuaType> FindLocalOrAssignTagDeclaration(LuaStatSyntax stat)
    {
        var docList = stat.Comments.FirstOrDefault()?.DocList;
        if (docList is null)
        {
            return [];
        }

        foreach (var tag in docList)
        {
            switch (tag)
            {
                case LuaDocTagClassSyntax tagClassSyntax:
                {
                    if (tagClassSyntax is { Name: { } name })
                    {
                        if (_typeDeclarations.TryGetValue(name.RepresentText, out var type))
                        {
                            return [type];
                        }
                    }

                    break;
                }
                case LuaDocTagInterfaceSyntax tagInterfaceSyntax:
                {
                    if (tagInterfaceSyntax is { Name: { } name })
                    {
                        if (_typeDeclarations.TryGetValue(name.RepresentText, out var type))
                        {
                            return [type];
                        }
                    }

                    break;
                }
                case LuaDocTagAliasSyntax tagAliasSyntax:
                {
                    if (tagAliasSyntax is { Name: { } name })
                    {
                        if (_typeDeclarations.TryGetValue(name.RepresentText, out var type))
                        {
                            return [type];
                        }
                    }

                    break;
                }
                case LuaDocTagEnumSyntax tagEnumSyntax:
                {
                    if (tagEnumSyntax is { Name: { } name })
                    {
                        if (_typeDeclarations.TryGetValue(name.RepresentText, out var type))
                        {
                            return [type];
                        }
                    }

                    break;
                }
                case LuaDocTagTypeSyntax tagTypeSyntax:
                {
                    return tagTypeSyntax.TypeList.Select(type => new LuaTypeRef(type)).Cast<ILuaType>().ToList();
                }
            }
        }

        return [];
    }

    private Dictionary<string, ILuaType> FindParamListDeclaration(LuaSyntaxElement element)
    {
        var stat = element.AncestorsAndSelf.OfType<LuaStatSyntax>().FirstOrDefault();
        if (stat is null)
        {
            return [];
        }

        var docList = stat.Comments.FirstOrDefault()?.DocList;
        if (docList is null)
        {
            return [];
        }

        var dic = new Dictionary<string, ILuaType>();
        foreach (var tagParam in docList.OfType<LuaDocTagParamSyntax>())
        {
            if (tagParam is { Name: { } name, Type: { } type })
            {
                dic.Add(name.RepresentText, new LuaTypeRef(type));
            }
        }

        return dic;
    }

    private void AssignStatDeclarationAnalysis(LuaAssignStatSyntax luaAssignStat)
    {
        var typeDeclarations = FindLocalOrAssignTagDeclaration(luaAssignStat);
        var varList = luaAssignStat.VarList.ToList();
        var count = varList.Count;
        for (var i = 0; i < count; i++)
        {
            var varExpr = varList[i];
            var luaType = typeDeclarations.ElementAtOrDefault(i);

            switch (varExpr)
            {
                case LuaNameExprSyntax nameExpr:
                {
                    if (nameExpr.Name is { } name)
                    {
                        var flags = FindNameExpr(nameExpr)?.Flags ?? DeclarationFlag.Global;
                        var declaration = CreateDeclaration(name.RepresentText, nameExpr, flags, luaType);
                        _curScope?.Add(declaration);
                    }

                    break;
                }
            }
        }
    }

    private void MethodDeclarationAnalysis(LuaFuncStatSyntax luaFuncStat)
    {
        if (luaFuncStat is { IsLocal: true, LocalName.Name: { } name })
        {
            var declaration = CreateDeclaration(name.RepresentText, luaFuncStat,
                DeclarationFlag.Function | DeclarationFlag.Local, null);
            _curScope?.Add(declaration);
        }
        else if (luaFuncStat is { IsLocal: false, NameExpr: { } nameExpr })
        {
            // var declaration = CreateDeclaration(name.RepresentText, luaFuncStat,
            //     DeclarationFlag.Function | DeclarationFlag.Global, null);
            // _curScope?.Add(declaration);
        }
    }
}
