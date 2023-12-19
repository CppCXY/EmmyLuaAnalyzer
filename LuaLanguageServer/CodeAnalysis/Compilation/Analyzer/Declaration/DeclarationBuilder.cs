using LuaLanguageServer.CodeAnalysis.Compilation.Type;
using LuaLanguageServer.CodeAnalysis.Compilation.TypeOperator;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;
using LuaLanguageServer.CodeAnalysis.Syntax.Tree;
using LuaLanguageServer.CodeAnalysis.Syntax.Walker;
using LuaLanguageServer.CodeAnalysis.Workspace;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Declaration;

public class DeclarationBuilder : ILuaElementWalker
{
    private DeclarationScope? _topScope = null;

    private DeclarationScope? _curScope = null;

    private Stack<DeclarationScope> _scopes = new();

    private Dictionary<LuaSyntaxElement, DeclarationScope> _scopeOwners = new();

    private DeclarationTree _tree;

    private LuaSyntaxTree _syntaxTree;

    private DeclarationAnalyzer Analyzer { get; }

    private Dictionary<string, Declaration> _typeDeclarations = new();

    private Dictionary<LuaSyntaxElement, List<Declaration>> _bindDocDeclarations = new();
    private DocumentId DocumentId { get; }

    public DeclarationTree Build()
    {
        _syntaxTree.SyntaxRoot.Accept(this);
        return _tree;
    }

    public DeclarationBuilder(DocumentId documentId, LuaSyntaxTree tree, DeclarationAnalyzer analyzer)
    {
        _syntaxTree = tree;
        _tree = new DeclarationTree(tree, _scopeOwners);
        Analyzer = analyzer;
        DocumentId = documentId;
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
                ParamListDeclarationAnalysis(paramListSyntax);
                break;
            }
            case LuaForRangeStatSyntax forRangeStatSyntax:
            {
                ForRangeStatDeclarationAnalysis(forRangeStatSyntax);
                break;
            }
            case LuaForStatSyntax forStatSyntax:
            {
                ForStatDeclarationAnalysis(forStatSyntax);
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
                ClassTagDeclarationAnalysis(tagClassSyntax);
                break;
            }
            case LuaDocTagAliasSyntax tagAliasSyntax:
            {
                AliasTagDeclarationAnalysis(tagAliasSyntax);
                break;
            }
            case LuaDocTagEnumSyntax tagEnumSyntax:
            {
                EnumTagDeclarationAnalysis(tagEnumSyntax);
                break;
            }
            case LuaDocTagInterfaceSyntax tagInterfaceSyntax:
            {
                InterfaceTagDeclarationAnalysis(tagInterfaceSyntax);
                break;
            }
            case LuaDocTagParamSyntax tagParamSyntax:
            {
                ParamTagDeclarationAnalysis(tagParamSyntax);
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
        var types = FindLocalOrAssignTypes(localStatSyntax);
        var nameList = localStatSyntax.NameList.ToList();
        var count = nameList.Count;
        for (var i = 0; i < count; i++)
        {
            var localName = nameList[i];
            var luaType = types.ElementAtOrDefault(i);
            if (localName is { Name: { } name })
            {
                var declaration = CreateDeclaration(name.RepresentText, localName, DeclarationFlag.Local, luaType);
                if (i == 0)
                {
                    var typeDeclaration = FindLocalOrAssignTagDeclaration(localStatSyntax);
                    declaration.PrevDeclaration = typeDeclaration;
                }

                _curScope?.Add(declaration);
            }
        }
    }

    private void ParamListDeclarationAnalysis(LuaParamListSyntax paramListSyntax)
    {
        var dic = FindParamListDeclaration(paramListSyntax);
        foreach (var param in paramListSyntax.Params)
        {
            if (param.Name is { } name)
            {
                var declaration = CreateDeclaration(name.RepresentText, param, DeclarationFlag.Local, null);
                if (dic.TryGetValue(name.RepresentText, out var prevDeclaration))
                {
                    declaration.PrevDeclaration = prevDeclaration;
                }

                _curScope?.Add(declaration);
            }
        }
    }

    private void ForRangeStatDeclarationAnalysis(LuaForRangeStatSyntax forRangeStatSyntax)
    {
        var dic = FindParamListDeclaration(forRangeStatSyntax);
        foreach (var param in forRangeStatSyntax.IteratorNames)
        {
            if (param.Name is { } name)
            {
                var declaration = CreateDeclaration(name.RepresentText, param, DeclarationFlag.Local, null);
                if (dic.TryGetValue(name.RepresentText, out var prevDeclaration))
                {
                    declaration.PrevDeclaration = prevDeclaration;
                }

                _curScope?.Add(declaration);
            }
        }
    }

    private void ForStatDeclarationAnalysis(LuaForStatSyntax forStatSyntax)
    {
        if (forStatSyntax.IteratorName is { Name: { } name })
        {
            var declaration = CreateDeclaration(name.RepresentText, name, DeclarationFlag.Local,
                Analyzer.Compilation.Builtin.Integer);
            _curScope?.Add(declaration);
        }
    }

    private Declaration? FindLocalOrAssignTagDeclaration(LuaStatSyntax stat)
    {
        var docList = stat.Comments.FirstOrDefault()?.DocList;
        if (docList is null)
        {
            return null;
        }

        foreach (var tag in docList)
        {
            switch (tag)
            {
                case LuaDocTagClassSyntax tagClassSyntax:
                {
                    if (tagClassSyntax is { Name: { } name })
                    {
                        if (_typeDeclarations.TryGetValue(name.RepresentText, out var declaration))
                        {
                            return declaration;
                        }
                    }

                    break;
                }
                case LuaDocTagInterfaceSyntax tagInterfaceSyntax:
                {
                    if (tagInterfaceSyntax is { Name: { } name })
                    {
                        if (_typeDeclarations.TryGetValue(name.RepresentText, out var declaration))
                        {
                            return declaration;
                        }
                    }

                    break;
                }
                case LuaDocTagAliasSyntax tagAliasSyntax:
                {
                    if (tagAliasSyntax is { Name: { } name })
                    {
                        if (_typeDeclarations.TryGetValue(name.RepresentText, out var declaration))
                        {
                            return declaration;
                        }
                    }

                    break;
                }
                case LuaDocTagEnumSyntax tagEnumSyntax:
                {
                    if (tagEnumSyntax is { Name: { } name })
                    {
                        if (_typeDeclarations.TryGetValue(name.RepresentText, out var declaration))
                        {
                            return declaration;
                        }
                    }

                    break;
                }
            }
        }

        return null;
    }

    private List<ILuaType> FindLocalOrAssignTypes(LuaStatSyntax stat) =>
    (
        from comment in stat.Comments
        from tagType in comment.DocList.OfType<LuaDocTagTypeSyntax>()
        from type in tagType.TypeList
        select new LuaTypeRef(type)
    ).Cast<ILuaType>().ToList();

    private Dictionary<string, Declaration> FindParamListDeclaration(LuaSyntaxElement element)
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

        var dic = new Dictionary<string, Declaration>();

        // ReSharper disable once InvertIf
        if (_bindDocDeclarations.TryGetValue(stat, out var declarations))
        {
            foreach (var declaration in declarations)
            {
                if (declaration is { Name: { } name, IsParam: true })
                {
                    dic.Add(name, declaration);
                }
            }
        }

        return dic;
    }

    private void AssignStatDeclarationAnalysis(LuaAssignStatSyntax luaAssignStat)
    {
        var types = FindLocalOrAssignTypes(luaAssignStat);
        var varList = luaAssignStat.VarList.ToList();
        var count = varList.Count;
        for (var i = 0; i < count; i++)
        {
            var varExpr = varList[i];
            var luaType = types.ElementAtOrDefault(i);
            switch (varExpr)
            {
                case LuaNameExprSyntax nameExpr:
                {
                    if (nameExpr.Name is { } name)
                    {
                        var prevDeclaration = FindNameExpr(nameExpr);
                        var flags = prevDeclaration?.Flags ?? DeclarationFlag.Global;
                        var declaration = CreateDeclaration(name.RepresentText, nameExpr, flags, luaType);
                        if (prevDeclaration is not null)
                        {
                            declaration.PrevDeclaration = prevDeclaration;
                        }
                        else
                        {
                            if (i == 0)
                            {
                                var typeDeclaration = FindLocalOrAssignTagDeclaration(luaAssignStat);
                                declaration.PrevDeclaration = typeDeclaration;
                            }

                            Analyzer.Compilation.StubIndexImpl.GlobalDeclaration.AddStub(DocumentId, name.RepresentText,
                                declaration);
                        }

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
            var declaration = CreateDeclaration(name.RepresentText, name,
                DeclarationFlag.Method | DeclarationFlag.Local, null);
            _curScope?.Add(declaration);
        }
        else if (luaFuncStat is { IsLocal: false, NameExpr.Name: { } name2 })
        {
            var declaration = CreateDeclaration(name2.RepresentText, name2,
                DeclarationFlag.Method | DeclarationFlag.ClassMember, null);
            _curScope?.Add(declaration);
        }
    }

    private void ClassTagDeclarationAnalysis(LuaDocTagClassSyntax tagClassSyntax)
    {
        if (tagClassSyntax is { Name: { } name })
        {
            var luaClass = new LuaClass(name.RepresentText);
            var declaration = CreateDeclaration(name.RepresentText, name, DeclarationFlag.TypeDeclaration, luaClass);
            _typeDeclarations.Add(name.RepresentText, declaration);
            Analyzer.Compilation.StubIndexImpl.GlobalDeclaration.AddStub(DocumentId, name.RepresentText, declaration);
            Analyzer.Compilation.StubIndexImpl.NamedTypeIndex.AddStub(DocumentId, name.RepresentText, luaClass);
            ClassOrInterfaceFieldsTagAnalysis(luaClass, tagClassSyntax);
            if (tagClassSyntax is { Body: { } body })
            {
                ClassOrInterfaceBodyAnalysis(luaClass, body);
            }
        }
    }

    private void AliasTagDeclarationAnalysis(LuaDocTagAliasSyntax tagAliasSyntax)
    {
        if (tagAliasSyntax is { Name: { } name, Type: { } type })
        {
            var luaAlias = new LuaAlias(name.RepresentText, new LuaTypeRef(type));
            var declaration = CreateDeclaration(name.RepresentText, name, DeclarationFlag.TypeDeclaration, luaAlias);
            _typeDeclarations.Add(name.RepresentText, declaration);
            Analyzer.Compilation.StubIndexImpl.GlobalDeclaration.AddStub(DocumentId, name.RepresentText, declaration);
            Analyzer.Compilation.StubIndexImpl.NamedTypeIndex.AddStub(DocumentId, name.RepresentText, luaAlias);
        }
    }

    private void EnumTagDeclarationAnalysis(LuaDocTagEnumSyntax tagEnumSyntax)
    {
        if (tagEnumSyntax is { Name: { } name })
        {
            ILuaType baseType = tagEnumSyntax.BaseType is { } type
                ? new LuaTypeRef(type)
                : Analyzer.Compilation.Builtin.Integer;
            var luaEnum = new LuaEnum(name.RepresentText, baseType);
            var declaration = CreateDeclaration(name.RepresentText, name, DeclarationFlag.TypeDeclaration, luaEnum);
            _typeDeclarations.Add(name.RepresentText, declaration);
            Analyzer.Compilation.StubIndexImpl.GlobalDeclaration.AddStub(DocumentId, name.RepresentText, declaration);
            Analyzer.Compilation.StubIndexImpl.NamedTypeIndex.AddStub(DocumentId, name.RepresentText, luaEnum);
            foreach (var field in tagEnumSyntax.FieldList)
            {
                if (field is { Name: { } fieldName })
                {
                    var fieldDeclaration = CreateDeclaration(fieldName.RepresentText, fieldName,
                        DeclarationFlag.EnumMember, baseType);
                    Analyzer.Compilation.StubIndexImpl.Members.AddStub(DocumentId, name.RepresentText,
                        fieldDeclaration);
                }
            }
        }
    }

    private void InterfaceTagDeclarationAnalysis(LuaDocTagInterfaceSyntax tagInterfaceSyntax)
    {
        if (tagInterfaceSyntax is { Name: { } name })
        {
            var luaInterface = new LuaInterface(name.RepresentText);
            var declaration =
                CreateDeclaration(name.RepresentText, name, DeclarationFlag.TypeDeclaration, luaInterface);
            _typeDeclarations.Add(name.RepresentText, declaration);
            Analyzer.Compilation.StubIndexImpl.GlobalDeclaration.AddStub(DocumentId, name.RepresentText, declaration);
            Analyzer.Compilation.StubIndexImpl.NamedTypeIndex.AddStub(DocumentId, name.RepresentText, luaInterface);
            ClassOrInterfaceFieldsTagAnalysis(luaInterface, tagInterfaceSyntax);
            if (tagInterfaceSyntax is { Body: { } body })
            {
                ClassOrInterfaceBodyAnalysis(luaInterface, body);
            }
        }
    }

    private void AddTagDocDeclaration(LuaDocTagSyntax element, Declaration declaration)
    {
        var stat = element.Parent?.Parent;
        if (stat is LuaStatSyntax)
        {
            if (!_bindDocDeclarations.TryGetValue(stat, out var declarations))
            {
                declarations = new List<Declaration>();
                _bindDocDeclarations.Add(stat, declarations);
            }

            declarations.Add(declaration);
        }
    }

    private void ParamTagDeclarationAnalysis(LuaDocTagParamSyntax tagParamSyntax)
    {
        if (tagParamSyntax is { Name: { } name, Type: { } type })
        {
            var declaration = CreateDeclaration(name.RepresentText, name,
                DeclarationFlag.Local | DeclarationFlag.Parameter, new LuaTypeRef(type));
            AddTagDocDeclaration(tagParamSyntax, declaration);
        }
    }

    private void ClassOrInterfaceFieldsTagAnalysis(ILuaNamedType namedType, LuaDocTagSyntax typeTag)
    {
        foreach (var tagField in typeTag.NextOfType<LuaDocTagFieldSyntax>())
        {
            switch (tagField)
            {
                case { NameField: { } nameField, Type: { } type1 }:
                {
                    var declaration = CreateDeclaration(nameField.RepresentText, nameField,
                        DeclarationFlag.ClassMember | DeclarationFlag.DocField, new LuaTypeRef(type1));
                    Analyzer.Compilation.StubIndexImpl.Members.AddStub(DocumentId, namedType.Name, declaration);
                    break;
                }
                case { IntegerField: { } integerField, Type: { } type2 }:
                {
                    var declaration = CreateDeclaration($"[{integerField.Value}]", integerField,
                        DeclarationFlag.ClassMember | DeclarationFlag.DocField, new LuaTypeRef(type2));
                    Analyzer.Compilation.StubIndexImpl.Members.AddStub(DocumentId, namedType.Name, declaration);
                    break;
                }
                case { StringField: { } stringField, Type: { } type3 }:
                {
                    var declaration = CreateDeclaration(stringField.Value, stringField,
                        DeclarationFlag.ClassMember | DeclarationFlag.DocField, new LuaTypeRef(type3));
                    Analyzer.Compilation.StubIndexImpl.Members.AddStub(DocumentId, namedType.Name, declaration);
                    break;
                }
                case { TypeField: { } typeField, Type: { } type4 }:
                {
                    var indexOperator = new IndexOperator(new LuaTypeRef(typeField), new LuaTypeRef(type4));
                    Analyzer.Compilation.StubIndexImpl.TypeOperators.AddStub(DocumentId, namedType.Name, indexOperator);
                    break;
                }
            }
        }
    }

    private void ClassOrInterfaceBodyAnalysis(ILuaNamedType namedType, LuaDocTagBodySyntax docBody)
    {
        foreach (var field in docBody.FieldList)
        {
            switch (field)
            {
                case { NameField: { } nameField, Type: { } type1 }:
                {
                    var declaration = CreateDeclaration(nameField.RepresentText, nameField,
                        DeclarationFlag.ClassMember | DeclarationFlag.DocField, new LuaTypeRef(type1));
                    Analyzer.Compilation.StubIndexImpl.Members.AddStub(DocumentId, namedType.Name, declaration);
                    break;
                }
                case { IntegerField: { } integerField, Type: { } type2 }:
                {
                    var declaration = CreateDeclaration($"[{integerField.Value}]", integerField,
                        DeclarationFlag.ClassMember | DeclarationFlag.DocField, new LuaTypeRef(type2));
                    Analyzer.Compilation.StubIndexImpl.Members.AddStub(DocumentId, namedType.Name, declaration);
                    break;
                }
                case { StringField: { } stringField, Type: { } type3 }:
                {
                    var declaration = CreateDeclaration(stringField.Value, stringField,
                        DeclarationFlag.ClassMember | DeclarationFlag.DocField, new LuaTypeRef(type3));
                    Analyzer.Compilation.StubIndexImpl.Members.AddStub(DocumentId, namedType.Name, declaration);
                    break;
                }
                case { TypeField: { } typeField, Type: { } type4 }:
                {
                    var indexOperator = new IndexOperator(new LuaTypeRef(typeField), new LuaTypeRef(type4));
                    Analyzer.Compilation.StubIndexImpl.TypeOperators.AddStub(DocumentId, namedType.Name, indexOperator);
                    break;
                }
            }
        }
    }
}
