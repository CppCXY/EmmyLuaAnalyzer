using EmmyLua.CodeAnalysis.Compilation.Index;
using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Syntax.Tree;
using EmmyLua.CodeAnalysis.Syntax.Walker;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.DeclarationAnalyzer;

public class DeclarationBuilder : ILuaElementWalker
{
    private SymbolScope? _topScope = null;

    private SymbolScope? _curScope = null;

    private Stack<SymbolScope> _scopeStack = new();

    private Dictionary<LuaSyntaxElement, SymbolScope> _scopeOwners = new();

    private SymbolTree _tree;

    private LuaSyntaxTree _syntaxTree;

    private DeclarationAnalyzer Analyzer { get; }

    private LuaCompilation Compilation => Analyzer.Compilation;

    private ProjectIndex ProjectIndex => Compilation.ProjectIndex;

    private AnalyzeContext AnalyzeContext { get; }

    private Dictionary<string, Declaration> _typeDeclarations = new();

    private DocumentId DocumentId { get; }

    public SymbolTree Build()
    {
        _syntaxTree.SyntaxRoot.Accept(this);
        _tree.RootScope = _topScope;
        return _tree;
    }

    public DeclarationBuilder(DocumentId documentId, LuaSyntaxTree tree, DeclarationAnalyzer analyzer,
        AnalyzeContext analyzeContext)
    {
        _syntaxTree = tree;
        _tree = new SymbolTree(tree, _scopeOwners);
        Analyzer = analyzer;
        DocumentId = documentId;
        AnalyzeContext = analyzeContext;
    }

    private Declaration? FindDeclaration(LuaNameExprSyntax nameExpr)
    {
        return FindScope(nameExpr)?.FindNameDeclaration(nameExpr);
    }

    private SymbolScope? FindScope(LuaSyntaxNode element)
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

    private static int GetPosition(LuaSyntaxElement element) => element.Range.StartOffset;

    private string GetUniqueId(LuaSyntaxElement element)
    {
        return $"{DocumentId.Id}|{GetPosition(element)}";
    }

    private void AddSymbol(Symbol.Symbol symbol)
    {
        _curScope?.Add(symbol);
    }

    private void AddUnResolvedSymbol(UnResolveDeclaration declaration)
    {
        AnalyzeContext.UnResolveDeclarations.Add(declaration);
    }

    private void PushScope(LuaSyntaxElement element)
    {
        if (_scopeOwners.TryGetValue(element, out var scope))
        {
            _scopeStack.Push(scope);
            _curScope = scope;
            return;
        }

        var position = GetPosition(element);
        switch (element)
        {
            case LuaLocalStatSyntax:
            {
                SetScope(new LocalStatSymbolScope(_tree, position), element);
                break;
            }
            case LuaRepeatStatSyntax:
            {
                SetScope(new RepeatStatSymbolScope(_tree, position), element);
                break;
            }
            case LuaForRangeStatSyntax:
            {
                SetScope(new ForRangeStatSymbolScope(_tree, position), element);
                break;
            }
            case LuaFuncStatSyntax funcStat:
            {
                PushMethod(position, funcStat);
                break;
            }
            default:
            {
                SetScope(new SymbolScope(_tree, position), element);
                break;
            }
        }
    }

    private void PushMethod(int position, LuaFuncStatSyntax funcStat)
    {
        ParameterDeclaration? self = null;
        // if (funcStat.IndexExpr is { PrefixExpr: { } prefixExpr })
        // {
        //     self = ParameterDeclaration.SelfParameter(new LuaExprRef(prefixExpr));
        // }

        SetScope(new MethodStatSymbolScope(_tree, position, self), funcStat);
    }

    private void SetScope(SymbolScope scope, LuaSyntaxElement element)
    {
        _scopeStack.Push(scope);
        _topScope ??= scope;
        _scopeOwners.Add(element, scope);
        _curScope?.Add(scope);
        _curScope = scope;
    }

    private void PopScope()
    {
        if (_scopeStack.Count != 0)
        {
            _scopeStack.Pop();
        }

        _curScope = _scopeStack.Count != 0 ? _scopeStack.Peek() : _topScope;
    }

    public void WalkIn(LuaSyntaxElement node)
    {
        if (IsScopeOwner(node))
        {
            PushScope(node);
        }

        switch (node)
        {
            case LuaLocalStatSyntax localStatSyntax:
            {
                AnalyzeLocalStatDeclaration(localStatSyntax);
                break;
            }
            case LuaForRangeStatSyntax forRangeStatSyntax:
            {
                AnalyzeForRangeStatDeclaration(forRangeStatSyntax);
                break;
            }
            case LuaForStatSyntax forStatSyntax:
            {
                AnalyzeForStatDeclaration(forStatSyntax);
                break;
            }
            case LuaFuncStatSyntax funcStatSyntax:
            {
                AnalyzeMethodDeclaration(funcStatSyntax);
                break;
            }
            case LuaClosureExprSyntax closureExprSyntax:
            {
                AnalyzeClosureExprDeclaration(closureExprSyntax);
                break;
            }
            case LuaAssignStatSyntax assignStatSyntax:
            {
                AnalyzeAssignStatDeclaration(assignStatSyntax);
                break;
            }
            case LuaDocTagClassSyntax tagClassSyntax:
            {
                AnalyzeClassTagDeclaration(tagClassSyntax);
                break;
            }
            case LuaDocTagAliasSyntax tagAliasSyntax:
            {
                AnalyzeAliasTagDeclaration(tagAliasSyntax);
                break;
            }
            case LuaDocTagEnumSyntax tagEnumSyntax:
            {
                AnalyzeEnumTagDeclaration(tagEnumSyntax);
                break;
            }
            case LuaDocTagInterfaceSyntax tagInterfaceSyntax:
            {
                AnalyzeInterfaceTagDeclaration(tagInterfaceSyntax);
                break;
            }
            case LuaTableFieldSyntax tableFieldSyntax:
            {
                AnalyzeTableFieldDeclaration(tableFieldSyntax);
                break;
            }
            case LuaDocTableTypeSyntax tableTypeSyntax:
            {
                AnalyzeLuaTableType(tableTypeSyntax);
                break;
            }
            case LuaSourceSyntax sourceSyntax:
            {
                if (sourceSyntax.Block is not null)
                {
                    AnalyzeBlockReturn(sourceSyntax.Block);
                }

                break;
            }
            case LuaLabelStatSyntax labelStatSyntax:
            {
                AnalyzeLuaLabel(labelStatSyntax);
                break;
            }
        }
    }

    public void WalkOut(LuaSyntaxElement node)
    {
        if (IsScopeOwner(node))
        {
            PopScope();
        }
    }

    private static bool IsScopeOwner(LuaSyntaxElement element)
        => element is LuaBlockSyntax or LuaRepeatStatSyntax or LuaForRangeStatSyntax or LuaForStatSyntax
            or LuaFuncStatSyntax or LuaFuncBodySyntax;

    private void AnalyzeLocalStatDeclaration(LuaLocalStatSyntax localStatSyntax)
    {
        var types = FindLocalOrAssignTypes(localStatSyntax);
        var nameList = localStatSyntax.NameList.ToList();
        var exprList = localStatSyntax.ExprList.ToList();
        LuaExprSyntax? lastValidExpr = null;
        var count = nameList.Count;
        var retId = 0;
        for (var i = 0; i < count; i++)
        {
            var localName = nameList[i];
            var luaType = types.ElementAtOrDefault(i);
            var expr = exprList.ElementAtOrDefault(i);
            if (expr is not null)
            {
                lastValidExpr = expr;
                retId = 0;
            }
            else
            {
                retId++;
            }

            LuaExprRef? relatedExpr = null;
            if (lastValidExpr is not null)
            {
                relatedExpr = new LuaExprRef(lastValidExpr, retId);
            }

            if (localName is { Name: { } name })
            {
                var declaration = new LocalDeclaration(name.RepresentText, GetPosition(localName), localName, luaType);
                AddSymbol(declaration);
                var unResolveDeclaration = new UnResolveDeclaration(declaration, relatedExpr);
                AddUnResolvedSymbol(unResolveDeclaration);
                if (i == 0)
                {
                    var typeDeclaration = FindLocalOrAssignTypeDeclaration(localStatSyntax);
                    declaration.DeclarationType = typeDeclaration?.DeclarationType;
                }
            }
        }
    }

    private List<ParameterDeclaration> AnalyzeParamListDeclaration(LuaParamListSyntax paramListSyntax)
    {
        var declarations = new List<ParameterDeclaration>();
        var dic = FindParamDeclarations(paramListSyntax);
        foreach (var param in paramListSyntax.Params)
        {
            if (param.Name is { } name)
            {
                var declaration = new ParameterDeclaration(name.RepresentText, GetPosition(param), param, null);
                if (dic.TryGetValue(name.RepresentText, out var prevDeclaration))
                {
                    declaration.DeclarationType = prevDeclaration.DeclarationType;
                }

                declarations.Add(declaration);
                AddSymbol(declaration);
            }
            else if (param.IsVarArgs)
            {
                var declaration = new ParameterDeclaration("...", GetPosition(param), param, null);
                if (dic.TryGetValue("...", out var prevDeclaration))
                {
                    declaration.DeclarationType = prevDeclaration.DeclarationType;
                }

                declarations.Add(declaration);
                AddSymbol(declaration);
            }
        }

        return declarations;
    }

    private List<LuaType> GetRetType(IEnumerable<LuaDocTagSyntax> docList)
    {
        var retTag = docList.OfType<LuaDocTagReturnSyntax>().ToList();
        return retTag.SelectMany(tag => tag.TypeList).Select(Compilation.SearchContext.Infer).ToList();
    }

    private void AnalyzeForRangeStatDeclaration(LuaForRangeStatSyntax forRangeStatSyntax)
    {
        var dic = FindParamDeclarations(forRangeStatSyntax);
        foreach (var param in forRangeStatSyntax.IteratorNames)
        {
            if (param.Name is { } name)
            {
                var declaration = new ParameterDeclaration(name.RepresentText, GetPosition(param), param, null);
                if (dic.TryGetValue(name.RepresentText, out var prevDeclaration))
                {
                    declaration.DeclarationType = prevDeclaration.DeclarationType;
                }

                AddSymbol(declaration);
            }
        }
    }

    private void AnalyzeForStatDeclaration(LuaForStatSyntax forStatSyntax)
    {
        if (forStatSyntax is { IteratorName.Name: { } name })
        {
            AddSymbol(new ParameterDeclaration(name.RepresentText, GetPosition(name), forStatSyntax.IteratorName,
                Compilation.Builtin.Integer));
        }
    }

    private Symbol.Symbol? FindLocalOrAssignTypeDeclaration(LuaStatSyntax stat)
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

    private List<LuaType> FindLocalOrAssignTypes(LuaStatSyntax stat) =>
    (
        from comment in stat.Comments
        from tagType in comment.DocList.OfType<LuaDocTagTypeSyntax>()
        from type in tagType.TypeList
        select Compilation.SearchContext.Infer(type)
    ).ToList();

    private Dictionary<string, Declaration> FindParamDeclarations(LuaSyntaxElement element)
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

        foreach (var tagParamSyntax in docList.OfType<LuaDocTagParamSyntax>())
        {
            if (tagParamSyntax is { Name: { } name, Type: { } type, Nullable: { } nullable })
            {
                var ty = Compilation.SearchContext.Infer(type);
                if (nullable)
                {
                    ty = ty.Union(Compilation.Builtin.Nil);
                }

                var declaration = new DocParameterDeclaration(name.RepresentText, GetPosition(name), name, ty);
                dic.Add(name.RepresentText, declaration);
                AddSymbol(declaration);
            }
            else if (tagParamSyntax is { VarArgs: { } varArgs, Type: { } type2 })
            {
                var ty = Compilation.SearchContext.Infer(type2);
                var declaration = new DocParameterDeclaration(varArgs.RepresentText, GetPosition(varArgs), varArgs, ty);
                dic.Add("...", declaration);
                AddSymbol(declaration);
            }
        }

        return dic;
    }

    private void AnalyzeAssignStatDeclaration(LuaAssignStatSyntax luaAssignStat)
    {
        var types = FindLocalOrAssignTypes(luaAssignStat);
        var varList = luaAssignStat.VarList.ToList();
        var exprList = luaAssignStat.ExprList.ToList();
        LuaExprSyntax? lastValidExpr = null;
        var retId = 0;
        var count = varList.Count;
        for (var i = 0; i < count; i++)
        {
            var varExpr = varList[i];
            var luaType = types.ElementAtOrDefault(i);
            var expr = exprList.ElementAtOrDefault(i);
            if (expr is not null)
            {
                lastValidExpr = expr;
                retId = 0;
            }
            else
            {
                retId++;
            }

            LuaExprRef? relatedExpr = null;
            if (lastValidExpr is not null)
            {
                relatedExpr = new LuaExprRef(lastValidExpr, retId);
            }

            switch (varExpr)
            {
                case LuaNameExprSyntax nameExpr:
                {
                    if (nameExpr.Name is { } name)
                    {
                        var prevDeclaration = FindDeclaration(nameExpr);
                        if (prevDeclaration is null)
                        {
                            var declaration = new GlobalDeclaration(name.RepresentText, GetPosition(nameExpr), nameExpr,
                                luaType);
                            ProjectIndex.GlobalDeclaration.AddStub(DocumentId, name.RepresentText, declaration);

                            if (i == 0)
                            {
                                var typeDeclaration = FindLocalOrAssignTypeDeclaration(luaAssignStat);
                                declaration.DeclarationType = typeDeclaration?.DeclarationType;
                            }

                            AddSymbol(declaration);
                            var unResolveDeclaration = new UnResolveDeclaration(declaration, relatedExpr);
                            AddUnResolvedSymbol(unResolveDeclaration);
                        }
                    }

                    break;
                }
                case LuaIndexExprSyntax indexExpr:
                {
                    var declaration = new IndexDeclaration(indexExpr.Name, GetPosition(indexExpr), indexExpr, luaType);
                    if (i == 0)
                    {
                        var typeDeclaration = FindLocalOrAssignTypeDeclaration(luaAssignStat);
                        declaration.DeclarationType = typeDeclaration?.DeclarationType;
                    }

                    AddSymbol(declaration);
                    var unResolveDeclaration = new UnResolveDeclaration(declaration, relatedExpr);
                    AddUnResolvedSymbol(unResolveDeclaration);
                    break;
                }
            }
        }
    }

    private void AnalyzeMethodDeclaration(LuaFuncStatSyntax luaFuncStat)
    {
        switch (luaFuncStat)
        {
            case { IsLocal: true, LocalName.Name: { } name }:
            {
                var luaMethods = AnalyzeFuncBody(luaFuncStat.FuncBody, false);
                var methodDeclaration = new MethodDeclaration(name.RepresentText,
                    GetPosition(luaFuncStat.LocalName), luaFuncStat.LocalName, luaMethods, luaFuncStat.FuncBody!)
                {
                    Feature = SymbolFeature.Local
                };
                AddSymbol(methodDeclaration);

                break;
            }
            case { IsLocal: false, NameExpr.Name: { } name2 }:
            {
                var luaMethods = AnalyzeFuncBody(luaFuncStat.FuncBody, false);
                var prevDeclaration = FindDeclaration(luaFuncStat.NameExpr);
                if (prevDeclaration is null)
                {
                    var declaration = new MethodDeclaration(name2.RepresentText,
                        GetPosition(luaFuncStat.NameExpr),
                        luaFuncStat.NameExpr, luaMethods, luaFuncStat.FuncBody!)
                    {
                        Feature = SymbolFeature.Global
                    };
                    ProjectIndex.GlobalDeclaration.AddStub(DocumentId, name2.RepresentText, declaration);
                    AddSymbol(declaration);
                }

                break;
            }
            case { IsMethod: true, IndexExpr: { } indexExpr }:
            {
                var luaMethod = AnalyzeFuncBody(luaFuncStat.FuncBody, indexExpr.IsColonIndex);
                if (indexExpr is { Name: { } name })
                {
                    var declaration = new MethodDeclaration(name, GetPosition(indexExpr), indexExpr, luaMethod,
                        luaFuncStat.FuncBody!);
                    AddSymbol(declaration);
                    var unResolveDeclaration = new UnResolveDeclaration(declaration, null);
                    AddUnResolvedSymbol(unResolveDeclaration);
                }

                break;
            }
        }
    }

    private void AnalyzeClosureExprDeclaration(LuaClosureExprSyntax closureExprSyntax)
    {
        var funcBody = closureExprSyntax.FuncBody;
        AnalyzeFuncBody(funcBody, false);
    }

    private void AnalyzeClassTagDeclaration(LuaDocTagClassSyntax tagClassSyntax)
    {
        if (tagClassSyntax is { Name: { } name })
        {
            var luaClass = new LuaNamedType(name.RepresentText);
            var declaration = new NamedTypeDeclaration(name.RepresentText, GetPosition(name), name, luaClass);
            AddSymbol(declaration);
            _typeDeclarations.Add(name.RepresentText, declaration);
            ProjectIndex.NamedType.AddStub(DocumentId, name.RepresentText, declaration);
            ProjectIndex.TypeIndex.AddFeature(DocumentId, name.RepresentText, TypeFeature.Class);
            TypeFieldsTagAnalysis(luaClass, tagClassSyntax);
            if (tagClassSyntax is { Body: { } body })
            {
                TypeBodyAnalysis(luaClass, body);
            }

            if (tagClassSyntax is { ExtendTypeList: { } extendTypeList })
            {
                TypeSupersAnalysis(extendTypeList, luaClass);
            }

            if (tagClassSyntax is { GenericDeclareList: { } genericDeclareList })
            {
                TypeGenericParamAnalysis(genericDeclareList, luaClass);
            }
        }
    }

    private void AnalyzeAliasTagDeclaration(LuaDocTagAliasSyntax tagAliasSyntax)
    {
        if (tagAliasSyntax is { Name: { } name, Type: { } type })
        {
            var luaAlias = new LuaAliasType(name.RepresentText, Compilation.SearchContext.Infer(type));
            var declaration = new NamedTypeDeclaration(name.RepresentText, GetPosition(name), name, luaAlias);
            AddSymbol(declaration);
            _typeDeclarations.Add(name.RepresentText, declaration);
            ProjectIndex.NamedType.AddStub(DocumentId, name.RepresentText, declaration);
        }
    }

    private void AnalyzeEnumTagDeclaration(LuaDocTagEnumSyntax tagEnumSyntax)
    {
        if (tagEnumSyntax is { Name: { } name })
        {
            var baseType = tagEnumSyntax.BaseType is { } type
                ? Compilation.SearchContext.Infer(type)
                : Analyzer.Compilation.Builtin.Integer;
            var luaEnum = new LuaNamedType(name.RepresentText);
            var declaration = new NamedTypeDeclaration(name.RepresentText, GetPosition(name), name, luaEnum);
            AddSymbol(declaration);
            _typeDeclarations.Add(name.RepresentText, declaration);
            ProjectIndex.NamedType.AddStub(DocumentId, name.RepresentText, declaration);
            ProjectIndex.TypeIndex.AddFeature(DocumentId, name.RepresentText, TypeFeature.Enum);
            foreach (var field in tagEnumSyntax.FieldList)
            {
                if (field is { Name: { } fieldName })
                {
                    var fieldDeclaration = new EnumFieldDeclaration(fieldName.RepresentText, GetPosition(fieldName),
                        field, baseType);
                    ProjectIndex.Members.AddStub(DocumentId, name.RepresentText, fieldDeclaration);
                }
            }
        }
    }

    private void AnalyzeInterfaceTagDeclaration(LuaDocTagInterfaceSyntax tagInterfaceSyntax)
    {
        if (tagInterfaceSyntax is { Name: { } name })
        {
            var luaInterface = new LuaNamedType(name.RepresentText);
            var declaration =
                new NamedTypeDeclaration(name.RepresentText, GetPosition(name), name, luaInterface);
            AddSymbol(declaration);
            _typeDeclarations.Add(name.RepresentText, declaration);
            ProjectIndex.NamedType.AddStub(DocumentId, name.RepresentText, declaration);
            ProjectIndex.TypeIndex.AddFeature(DocumentId, name.RepresentText, TypeFeature.Interface);
            TypeFieldsTagAnalysis(luaInterface, tagInterfaceSyntax);
            if (tagInterfaceSyntax is { Body: { } body })
            {
                TypeBodyAnalysis(luaInterface, body);
            }

            if (tagInterfaceSyntax is { ExtendTypeList: { } extendTypeList })
            {
                TypeSupersAnalysis(extendTypeList, luaInterface);
            }

            if (tagInterfaceSyntax is { GenericDeclareList: { } genericDeclareList })
            {
                TypeGenericParamAnalysis(genericDeclareList, luaInterface);
            }
        }
    }

    private void TypeFieldsTagAnalysis(LuaNamedType namedType, LuaDocTagSyntax typeTag)
    {
        foreach (var tagField in typeTag.NextOfType<LuaDocTagFieldSyntax>())
        {
            switch (tagField)
            {
                case { NameField: { } nameField, Type: { } type1 }:
                {
                    var type = Compilation.SearchContext.Infer(type1);
                    var declaration = new DocFieldDeclaration(nameField.RepresentText, GetPosition(nameField),
                        tagField, type);
                    AddSymbol(declaration);
                    ProjectIndex.Members.AddStub(DocumentId, namedType.Name, declaration);
                    break;
                }
                case { IntegerField: { } integerField, Type: { } type2 }:
                {
                    var type = Compilation.SearchContext.Infer(type2);
                    var declaration = new DocFieldDeclaration($"[{integerField.Value}]", GetPosition(integerField),
                        tagField, type);
                    AddSymbol(declaration);
                    ProjectIndex.Members.AddStub(DocumentId, namedType.Name, declaration);
                    break;
                }
                case { StringField: { } stringField, Type: { } type3 }:
                {
                    var type = Compilation.SearchContext.Infer(type3);
                    var declaration = new DocFieldDeclaration(stringField.Value, GetPosition(stringField),
                        tagField, type);
                    AddSymbol(declaration);
                    ProjectIndex.Members.AddStub(DocumentId, namedType.Name, declaration);
                    break;
                }
                case { TypeField: { } typeField, Type: { } type4 }:
                {
                    var keyType = Compilation.SearchContext.Infer(typeField);
                    var valueType = Compilation.SearchContext.Infer(type4);
                    var indexOperator = new IndexOperator(namedType, keyType, valueType);
                    ProjectIndex.TypeIndex.AddTypeOperator(DocumentId, indexOperator);
                    break;
                }
            }
        }
    }

    private void TypeBodyAnalysis(LuaNamedType namedType, LuaDocTagBodySyntax docBody)
    {
        foreach (var field in docBody.FieldList)
        {
            switch (field)
            {
                case { NameField: { } nameField, Type: { } type1 }:
                {
                    var type = Compilation.SearchContext.Infer(type1);
                    var declaration = new DocFieldDeclaration(nameField.RepresentText, GetPosition(nameField),
                        field, type);
                    AddSymbol(declaration);
                    ProjectIndex.Members.AddStub(DocumentId, namedType.Name, declaration);
                    break;
                }
                case { IntegerField: { } integerField, Type: { } type2 }:
                {
                    var type = Compilation.SearchContext.Infer(type2);
                    var declaration = new DocFieldDeclaration($"[{integerField.Value}]", GetPosition(integerField),
                        field, type);
                    AddSymbol(declaration);
                    ProjectIndex.Members.AddStub(DocumentId, namedType.Name, declaration);
                    break;
                }
                case { StringField: { } stringField, Type: { } type3 }:
                {
                    var type = Compilation.SearchContext.Infer(type3);
                    var declaration = new DocFieldDeclaration(stringField.Value, GetPosition(stringField),
                        field, type);
                    AddSymbol(declaration);
                    ProjectIndex.Members.AddStub(DocumentId, namedType.Name, declaration);
                    break;
                }
                case { TypeField: { } typeField, Type: { } type4 }:
                {
                    var keyType = Compilation.SearchContext.Infer(typeField);
                    var valueType = Compilation.SearchContext.Infer(type4);
                    var indexOperator = new IndexOperator(namedType, keyType, valueType);
                    ProjectIndex.TypeIndex.AddTypeOperator(DocumentId, indexOperator);
                    break;
                }
            }
        }
    }

    private void TypeSupersAnalysis(IEnumerable<LuaDocTypeSyntax> extendList, LuaNamedType namedType)
    {
        foreach (var extend in extendList)
        {
            var type = Compilation.SearchContext.Infer(extend);
            ProjectIndex.Supers.AddStub(DocumentId, namedType.Name, type);
        }
    }

    private void TypeGenericParamAnalysis(LuaDocTagGenericDeclareListSyntax genericDeclareList,
        LuaNamedType namedType)
    {
        foreach (var param in genericDeclareList.Params)
        {
            if (param is { Name: { } name })
            {
                var type = Compilation.SearchContext.Infer(param.Type);
                var declaration = new GenericParameterDeclaration(name.RepresentText, GetPosition(name), param, type);
                ProjectIndex.GenericParam.AddStub(DocumentId, namedType.Name, declaration);
            }
        }
    }

    private void AnalyzeTableFieldDeclaration(LuaTableFieldSyntax tableFieldSyntax)
    {
        if (tableFieldSyntax is { Name: { } fieldName, ParentTable: { } table, Value: { } value })
        {
            var parentId = GetUniqueId(table);
            // TODO get type from ---@field ---@type
            var declaration =
                new TableFieldDeclaration(fieldName, GetPosition(tableFieldSyntax), tableFieldSyntax, null);
            AddSymbol(declaration);
            ProjectIndex.Members.AddStub(DocumentId, parentId, declaration);
            var unResolveDeclaration = new UnResolveDeclaration(declaration, new LuaExprRef(value));
            AddUnResolvedSymbol(unResolveDeclaration);
        }
    }

    private List<LuaMethodType> AnalyzeFuncBody(LuaFuncBodySyntax? funcBody, bool colon)
    {
        var comment = funcBody?.AncestorsAndSelf.OfType<LuaStatSyntax>().FirstOrDefault()?.Comments.FirstOrDefault();
        if (comment is null)
        {
            return [];
        }

        var docList = comment.DocList.ToList();
        var results = new List<LuaMethodType>();

        var generic = docList.OfType<LuaDocTagGenericDeclareListSyntax>().FirstOrDefault();
        if (generic is not null)
        {
            foreach (var param in generic.Params)
            {
                if (param is { Name: { } name })
                {
                    var declaration = new GenericParameterDeclaration(name.RepresentText, GetPosition(param), param, null);
                    AddSymbol(declaration);
                }
            }
        }

        var overloads = docList
            .OfType<LuaDocTagOverloadSyntax>()
            .Select(it => Compilation.SearchContext.Infer(it.TypeFunc))
            .Cast<LuaMethodType>().ToList();

        PushScope(funcBody!);
        var parameters = new List<TypedParameter>();
        if (funcBody!.ParamList is { } paramList)
        {
            parameters = AnalyzeParamListDeclaration(paramList)
                .Select(it => new TypedParameter(it.Name, it.DeclarationType))
                .ToList();
        }

        if (funcBody.Block is not null)
        {
            AnalyzeBlockReturn(funcBody.Block);
        }

        var mainRetType = GetRetType(docList);
        var method = new LuaMethodType(mainRetType, parameters);
        AnalyzeContext.Methods.Add(funcBody, method);
        PopScope();

        results.Add(method);
        results.AddRange(overloads);
        return results;
    }

    private void AnalyzeLuaTableType(LuaDocTableTypeSyntax luaDocTableTypeSyntax)
    {
        var className = GetUniqueId(luaDocTableTypeSyntax);
        var tableType = new LuaNamedType(className);
        foreach (var fieldSyntax in luaDocTableTypeSyntax.FieldList)
        {
            if (fieldSyntax is { NameField: { } nameToken, Type: { } type1 })
            {
                var type = Compilation.SearchContext.Infer(type1);
                var declaration = new DocFieldDeclaration(nameToken.RepresentText, GetPosition(fieldSyntax),
                    fieldSyntax, type);
                AddSymbol(declaration);
                ProjectIndex.Members.AddStub(DocumentId, className, declaration);
            }
            else if (fieldSyntax is { IntegerField: { } integerField, Type: { } type2 })
            {
                var type = Compilation.SearchContext.Infer(type2);
                var declaration = new DocFieldDeclaration($"[{integerField.Value}]", GetPosition(fieldSyntax),
                    fieldSyntax, type);
                AddSymbol(declaration);
                ProjectIndex.Members.AddStub(DocumentId, className, declaration);
            }
            else if (fieldSyntax is { StringField: { } stringField, Type: { } type3 })
            {
                var type = Compilation.SearchContext.Infer(type3);
                var declaration = new DocFieldDeclaration(stringField.Value, GetPosition(fieldSyntax), fieldSyntax,
                    type);
                AddSymbol(declaration);
                ProjectIndex.Members.AddStub(DocumentId, className, declaration);
            }
            else if (fieldSyntax is { TypeField: { } typeField, Type: { } type4 })
            {
                var keyType = Compilation.SearchContext.Infer(typeField);
                var valueType = Compilation.SearchContext.Infer(type4);
                var indexOperator = new IndexOperator(tableType, keyType, valueType);
                ProjectIndex.TypeIndex.AddTypeOperator(DocumentId, indexOperator);
            }
        }
    }

    private void AnalyzeBlockReturn(LuaBlockSyntax mainBlock)
    {
        var queue = new Queue<LuaBlockSyntax>();
        queue.Enqueue(mainBlock);
        while (queue.Count != 0)
        {
            var block = queue.Dequeue();
            foreach (var stat in block.StatList)
            {
                switch (stat)
                {
                    case LuaDoStatSyntax doStat:
                    {
                        if (doStat.Block is not null)
                        {
                            queue.Enqueue(doStat.Block);
                        }

                        break;
                    }
                    case LuaWhileStatSyntax whileStat:
                    {
                        if (whileStat.Block is not null)
                        {
                            queue.Enqueue(whileStat.Block);
                        }

                        break;
                    }
                    case LuaRepeatStatSyntax repeatStat:
                    {
                        if (repeatStat.Block is not null)
                        {
                            queue.Enqueue(repeatStat.Block);
                        }

                        break;
                    }
                    case LuaIfStatSyntax ifStat:
                    {
                        if (ifStat.ThenBlock is not null)
                        {
                            queue.Enqueue(ifStat.ThenBlock);
                        }

                        foreach (var ifClauseStatSyntax in ifStat.IfClauseStatementList)
                        {
                            if (ifClauseStatSyntax.Block is not null)
                            {
                                queue.Enqueue(ifClauseStatSyntax.Block);
                            }
                        }

                        break;
                    }
                    case LuaForStatSyntax forStat:
                    {
                        if (forStat.Block is not null)
                        {
                            queue.Enqueue(forStat.Block);
                        }

                        break;
                    }
                    case LuaForRangeStatSyntax forRangeStat:
                    {
                        if (forRangeStat.Block is not null)
                        {
                            queue.Enqueue(forRangeStat.Block);
                        }

                        break;
                    }
                    case LuaReturnStatSyntax returnStatSyntax:
                    {
                        AnalyzeContext.MainBlockReturns.Add(block, returnStatSyntax.ExprList.ToList());
                        break;
                    }
                }
            }
        }
    }

    private void AnalyzeLuaLabel(LuaLabelStatSyntax labelStatSyntax)
    {
        if (labelStatSyntax is { Name: { } name })
        {
            var labelDeclaration =
                new LabelDeclaration(name.RepresentText, GetPosition(labelStatSyntax), labelStatSyntax);
            AddSymbol(labelDeclaration);
        }
    }
}
