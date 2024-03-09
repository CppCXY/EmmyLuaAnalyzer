using EmmyLua.CodeAnalysis.Compilation.Index;
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

    private void AddDeclaration(Declaration declaration)
    {
        _curScope?.Add(declaration);
    }

    private void AddUnResolved(UnResolved declaration)
    {
        AnalyzeContext.UnResolves.Add(declaration);
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
            default:
            {
                SetScope(new SymbolScope(_tree, position), element);
                break;
            }
        }
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
                AnalyzeClosureExpr(closureExprSyntax);
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
                AnalyzeSource(sourceSyntax);
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
        => element is LuaBlockSyntax or LuaRepeatStatSyntax or LuaForRangeStatSyntax or LuaForStatSyntax or LuaClosureExprSyntax;

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
                AddDeclaration(declaration);
                var unResolveDeclaration =
                    new UnResolvedDeclaration(declaration, relatedExpr, ResolveState.UnResolvedType);
                AddUnResolved(unResolveDeclaration);
                if (i == 0)
                {
                    var ty = FindFirstLocalOrAssignType(localStatSyntax);
                    if (ty is not null)
                    {
                        declaration.DeclarationType = ty;
                        unResolveDeclaration.IsTypeDeclaration = true;
                    }
                }
            }
        }
    }

    private List<ParameterDeclaration> AnalyzeParamListDeclaration(LuaParamListSyntax paramListSyntax)
    {
        var parameters = new List<ParameterDeclaration>();
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

                parameters.Add(declaration);
                AddDeclaration(declaration);
            }
            else if (param.IsVarArgs)
            {
                var declaration = new ParameterDeclaration("...", GetPosition(param), param, null);
                if (dic.TryGetValue("...", out var prevDeclaration))
                {
                    declaration.DeclarationType = prevDeclaration.DeclarationType;
                }

                parameters.Add(declaration);
                AddDeclaration(declaration);
            }
        }

        return parameters;
    }

    private LuaMultiReturnType GetRetType(IEnumerable<LuaDocTagSyntax> docList)
    {
        var retTag = docList.OfType<LuaDocTagReturnSyntax>().ToList();
        return new LuaMultiReturnType(
            retTag.SelectMany(tag => tag.TypeList).Select(Compilation.SearchContext.Infer).ToList());
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

                AddDeclaration(declaration);
            }
        }
    }

    private void AnalyzeForStatDeclaration(LuaForStatSyntax forStatSyntax)
    {
        if (forStatSyntax is { IteratorName.Name: { } name })
        {
            AddDeclaration(new ParameterDeclaration(name.RepresentText, GetPosition(name), forStatSyntax.IteratorName,
                Builtin.Integer));
        }
    }

    private LuaType? FindFirstLocalOrAssignType(LuaStatSyntax stat)
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
                        return new LuaNamedType(name.RepresentText);
                    }

                    break;
                }
                case LuaDocTagInterfaceSyntax tagInterfaceSyntax:
                {
                    if (tagInterfaceSyntax is { Name: { } name })
                    {
                        return new LuaNamedType(name.RepresentText);
                    }

                    break;
                }
                case LuaDocTagAliasSyntax tagAliasSyntax:
                {
                    if (tagAliasSyntax is { Name: { } name })
                    {
                        return new LuaNamedType(name.RepresentText);
                    }

                    break;
                }
                case LuaDocTagEnumSyntax tagEnumSyntax:
                {
                    if (tagEnumSyntax is { Name: { } name })
                    {
                        return new LuaNamedType(name.RepresentText);
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
                    ty = ty.Union(Builtin.Nil);
                }

                var declaration = new DocParameterDeclaration(name.RepresentText, GetPosition(name), name, ty);
                dic.Add(name.RepresentText, declaration);
                AddDeclaration(declaration);
            }
            else if (tagParamSyntax is { VarArgs: { } varArgs, Type: { } type2 })
            {
                var ty = Compilation.SearchContext.Infer(type2);
                var declaration = new DocParameterDeclaration(varArgs.RepresentText, GetPosition(varArgs), varArgs, ty);
                dic.Add("...", declaration);
                AddDeclaration(declaration);
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
                            ProjectIndex.AddGlobal(DocumentId, name.RepresentText, declaration);
                            var unResolveDeclaration =
                                new UnResolvedDeclaration(declaration, relatedExpr, ResolveState.UnResolvedType);
                            AddUnResolved(unResolveDeclaration);

                            if (i == 0)
                            {
                                declaration.DeclarationType = FindFirstLocalOrAssignType(luaAssignStat);
                                unResolveDeclaration.IsTypeDeclaration = true;
                            }

                            AddDeclaration(declaration);
                        }
                    }

                    break;
                }
                case LuaIndexExprSyntax indexExpr:
                {
                    var declaration = new IndexDeclaration(indexExpr.Name, GetPosition(indexExpr), indexExpr, luaType);
                    if (i == 0)
                    {
                        declaration.DeclarationType = FindFirstLocalOrAssignType(luaAssignStat);
                    }

                    AddDeclaration(declaration);
                    var unResolveDeclaration = new UnResolvedDeclaration(declaration, relatedExpr,
                        ResolveState.UnResolvedType | ResolveState.UnResolvedIndex);
                    AddUnResolved(unResolveDeclaration);
                    break;
                }
            }
        }
    }

    private void AnalyzeMethodDeclaration(LuaFuncStatSyntax luaFuncStat)
    {
        switch (luaFuncStat)
        {
            case { IsLocal: true, LocalName.Name: { } name, ClosureExpr: { } closureExpr }:
            {
                var declaration = new MethodDeclaration(name.RepresentText,
                    GetPosition(luaFuncStat.LocalName), luaFuncStat.LocalName, null, closureExpr)
                {
                    Feature = SymbolFeature.Local
                };
                AddDeclaration(declaration);
                var unResolved = new UnResolvedDeclaration(declaration, new LuaExprRef(closureExpr), ResolveState.UnResolvedType);
                AddUnResolved(unResolved);
                break;
            }
            case { IsLocal: false, NameExpr.Name: { } name2, ClosureExpr: { } closureExpr }:
            {
                var prevDeclaration = FindDeclaration(luaFuncStat.NameExpr);
                if (prevDeclaration is null)
                {
                    var declaration = new MethodDeclaration(name2.RepresentText,
                        GetPosition(luaFuncStat.NameExpr),
                        luaFuncStat.NameExpr, null, closureExpr)
                    {
                        Feature = SymbolFeature.Global
                    };
                    ProjectIndex.AddGlobal(DocumentId, name2.RepresentText, declaration);
                    AddDeclaration(declaration);
                    var unResolved = new UnResolvedDeclaration(declaration, new LuaExprRef(closureExpr), ResolveState.UnResolvedType);
                    AddUnResolved(unResolved);
                }

                break;
            }
            case { IsMethod: true, IndexExpr: { } indexExpr, ClosureExpr: { } closureExpr }:
            {
                if (indexExpr is { Name: { } name })
                {
                    var declaration = new MethodDeclaration(name, GetPosition(indexExpr), indexExpr, null,
                        closureExpr);
                    AddDeclaration(declaration);
                    var unResolved = new UnResolvedDeclaration(declaration, new LuaExprRef(closureExpr), ResolveState.UnResolvedIndex | ResolveState.UnResolvedType);
                    AddUnResolved(unResolved);
                }

                break;
            }
        }
    }

    private void AnalyzeClosureExpr(LuaClosureExprSyntax closureExprSyntax)
    {
        var comment = closureExprSyntax.AncestorsAndSelf.OfType<LuaStatSyntax>().FirstOrDefault()?.Comments
            .FirstOrDefault();
        if (comment is null)
        {
            return;
        }

        var docList = comment.DocList.ToList();
        var generic = docList.OfType<LuaDocTagGenericDeclareListSyntax>().FirstOrDefault();
        if (generic is not null)
        {
            foreach (var param in generic.Params)
            {
                if (param is { Name: { } name })
                {
                    var declaration =
                        new GenericParameterDeclaration(name.RepresentText, GetPosition(param), param, null);
                    AddDeclaration(declaration);
                }
            }
        }

        var overloads = docList
            .OfType<LuaDocTagOverloadSyntax>()
            .Select(it => Compilation.SearchContext.Infer(it.TypeFunc))
            .Cast<LuaMethodType>()
            .Select(it=>it.MainSignature).ToList();

        var parameters = new List<ParameterDeclaration>();
        if (closureExprSyntax.ParamList is { } paramList)
        {
            PushScope(closureExprSyntax);
            parameters = AnalyzeParamListDeclaration(paramList);
            PopScope();
        }

        var mainRetType = GetRetType(docList);
        var isColon = false;
        var method = new LuaMethodType(new LuaSignature(mainRetType, parameters), overloads, isColon);
        if (closureExprSyntax.Block is { } block)
        {
            var unResolved = new UnResolvedMethod(method, block, ResolveState.UnResolveReturn);
            AddUnResolved(unResolved);
        }

        ProjectIndex.AddMethod(DocumentId, closureExprSyntax.UniqueId, method);
    }

    private void AnalyzeClassTagDeclaration(LuaDocTagClassSyntax tagClassSyntax)
    {
        if (tagClassSyntax is { Name: { } name })
        {
            var luaClass = new LuaNamedType(name.RepresentText);
            var declaration = new NamedTypeDeclaration(name.RepresentText, GetPosition(name), name, luaClass);
            AddDeclaration(declaration);
            ProjectIndex.AddType(DocumentId, name.RepresentText, declaration, TypeFeature.Class);

            AnalyzeTypeFields(luaClass, tagClassSyntax);
            AnalyzeTypeOperator(luaClass, tagClassSyntax);

            if (tagClassSyntax is { Body: { } body })
            {
                AnalyzeTypeBody(luaClass, body);
            }

            if (tagClassSyntax is { ExtendTypeList: { } extendTypeList })
            {
                AnalyzeTypeSupers(extendTypeList, luaClass);
            }

            if (tagClassSyntax is { GenericDeclareList: { } genericDeclareList })
            {
                AnalyzeTypeGenericParam(genericDeclareList, luaClass);
            }
        }
    }

    private void AnalyzeAliasTagDeclaration(LuaDocTagAliasSyntax tagAliasSyntax)
    {
        if (tagAliasSyntax is { Name: { } name, Type: { } type })
        {
            var luaAlias = new LuaNamedType(name.RepresentText);
            var baseTy = Compilation.SearchContext.Infer(type);
            var declaration = new NamedTypeDeclaration(name.RepresentText, GetPosition(name), name, luaAlias);
            AddDeclaration(declaration);
            ProjectIndex.AddAlias(DocumentId, name.RepresentText, baseTy, declaration);
        }
    }

    private void AnalyzeEnumTagDeclaration(LuaDocTagEnumSyntax tagEnumSyntax)
    {
        if (tagEnumSyntax is { Name: { } name })
        {
            var baseType = tagEnumSyntax.BaseType is { } type
                ? Compilation.SearchContext.Infer(type)
                : Builtin.Integer;
            var luaEnum = new LuaNamedType(name.RepresentText);
            var declaration = new NamedTypeDeclaration(name.RepresentText, GetPosition(name), name, luaEnum);
            AddDeclaration(declaration);
            ProjectIndex.AddType(DocumentId, name.RepresentText, declaration, TypeFeature.Enum);
            foreach (var field in tagEnumSyntax.FieldList)
            {
                if (field is { Name: { } fieldName })
                {
                    var fieldDeclaration = new EnumFieldDeclaration(fieldName.RepresentText, GetPosition(fieldName),
                        field, baseType);
                    ProjectIndex.AddMember(DocumentId, name.RepresentText, fieldDeclaration);
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
            AddDeclaration(declaration);

            ProjectIndex.AddType(DocumentId, name.RepresentText, declaration, TypeFeature.Interface);
            AnalyzeTypeFields(luaInterface, tagInterfaceSyntax);
            AnalyzeTypeOperator(luaInterface, tagInterfaceSyntax);
            if (tagInterfaceSyntax is { Body: { } body })
            {
                AnalyzeTypeBody(luaInterface, body);
            }

            if (tagInterfaceSyntax is { ExtendTypeList: { } extendTypeList })
            {
                AnalyzeTypeSupers(extendTypeList, luaInterface);
            }

            if (tagInterfaceSyntax is { GenericDeclareList: { } genericDeclareList })
            {
                AnalyzeTypeGenericParam(genericDeclareList, luaInterface);
            }
        }
    }

    private void AnalyzeTypeOperator(LuaNamedType namedType, LuaDocTagSyntax typeTag)
    {
        foreach (var operatorSyntax in typeTag.NextOfType<LuaDocTagOperatorSyntax>())
        {
            switch (operatorSyntax.Operator?.RepresentText)
            {
                case "add":
                {
                    var type = Compilation.SearchContext.Infer(operatorSyntax.Types.FirstOrDefault());
                    var retType = Compilation.SearchContext.Infer(operatorSyntax.ReturnType);
                    var op = new BinaryOperator(TypeOperatorKind.Add, namedType, type, retType);
                    ProjectIndex.TypeIndex.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "sub":
                {
                    var type = Compilation.SearchContext.Infer(operatorSyntax.Types.FirstOrDefault());
                    var retType = Compilation.SearchContext.Infer(operatorSyntax.ReturnType);
                    var op = new BinaryOperator(TypeOperatorKind.Sub, namedType, type, retType);
                    ProjectIndex.TypeIndex.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "mul":
                {
                    var type = Compilation.SearchContext.Infer(operatorSyntax.Types.FirstOrDefault());
                    var retType = Compilation.SearchContext.Infer(operatorSyntax.ReturnType);
                    var op = new BinaryOperator(TypeOperatorKind.Mul, namedType, type, retType);
                    ProjectIndex.TypeIndex.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "div":
                {
                    var type = Compilation.SearchContext.Infer(operatorSyntax.Types.FirstOrDefault());
                    var retType = Compilation.SearchContext.Infer(operatorSyntax.ReturnType);
                    var op = new BinaryOperator(TypeOperatorKind.Div, namedType, type, retType);
                    ProjectIndex.TypeIndex.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "mod":
                {
                    var type = Compilation.SearchContext.Infer(operatorSyntax.Types.FirstOrDefault());
                    var retType = Compilation.SearchContext.Infer(operatorSyntax.ReturnType);
                    var op = new BinaryOperator(TypeOperatorKind.Mod, namedType, type, retType);
                    ProjectIndex.TypeIndex.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "pow":
                {
                    var type = Compilation.SearchContext.Infer(operatorSyntax.Types.FirstOrDefault());
                    var retType = Compilation.SearchContext.Infer(operatorSyntax.ReturnType);
                    var op = new BinaryOperator(TypeOperatorKind.Pow, namedType, type, retType);
                    ProjectIndex.TypeIndex.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "unm":
                {
                    var retType = Compilation.SearchContext.Infer(operatorSyntax.ReturnType);
                    var op = new UnaryOperator(TypeOperatorKind.Unm, namedType, retType);
                    ProjectIndex.TypeIndex.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "idiv":
                {
                    var type = Compilation.SearchContext.Infer(operatorSyntax.Types.FirstOrDefault());
                    var retType = Compilation.SearchContext.Infer(operatorSyntax.ReturnType);
                    var op = new BinaryOperator(TypeOperatorKind.Idiv, namedType, type, retType);
                    ProjectIndex.TypeIndex.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "band":
                {
                    var type = Compilation.SearchContext.Infer(operatorSyntax.Types.FirstOrDefault());
                    var retType = Compilation.SearchContext.Infer(operatorSyntax.ReturnType);
                    var op = new BinaryOperator(TypeOperatorKind.Band, namedType, type, retType);
                    ProjectIndex.TypeIndex.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "bor":
                {
                    var type = Compilation.SearchContext.Infer(operatorSyntax.Types.FirstOrDefault());
                    var retType = Compilation.SearchContext.Infer(operatorSyntax.ReturnType);
                    var op = new BinaryOperator(TypeOperatorKind.Bor, namedType, type, retType);
                    ProjectIndex.TypeIndex.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "bxor":
                {
                    var type = Compilation.SearchContext.Infer(operatorSyntax.Types.FirstOrDefault());
                    var retType = Compilation.SearchContext.Infer(operatorSyntax.ReturnType);
                    var op = new BinaryOperator(TypeOperatorKind.Bxor, namedType, type, retType);
                    ProjectIndex.TypeIndex.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "bnot":
                {
                    var retType = Compilation.SearchContext.Infer(operatorSyntax.ReturnType);
                    var op = new UnaryOperator(TypeOperatorKind.Bnot, namedType, retType);
                    ProjectIndex.TypeIndex.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "shl":
                {
                    var type = Compilation.SearchContext.Infer(operatorSyntax.Types.FirstOrDefault());
                    var retType = Compilation.SearchContext.Infer(operatorSyntax.ReturnType);
                    var op = new BinaryOperator(TypeOperatorKind.Shl, namedType, type, retType);
                    ProjectIndex.TypeIndex.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "shr":
                {
                    var type = Compilation.SearchContext.Infer(operatorSyntax.Types.FirstOrDefault());
                    var retType = Compilation.SearchContext.Infer(operatorSyntax.ReturnType);
                    var op = new BinaryOperator(TypeOperatorKind.Shr, namedType, type, retType);
                    ProjectIndex.TypeIndex.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "concat":
                {
                    var type = Compilation.SearchContext.Infer(operatorSyntax.Types.FirstOrDefault());
                    var retType = Compilation.SearchContext.Infer(operatorSyntax.ReturnType);
                    var op = new BinaryOperator(TypeOperatorKind.Concat, namedType, type, retType);
                    ProjectIndex.TypeIndex.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "len":
                {
                    var retType = Compilation.SearchContext.Infer(operatorSyntax.ReturnType);
                    var op = new UnaryOperator(TypeOperatorKind.Len, namedType, retType);
                    ProjectIndex.TypeIndex.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "eq":
                {
                    var type = Compilation.SearchContext.Infer(operatorSyntax.Types.FirstOrDefault());
                    var op = new BinaryOperator(TypeOperatorKind.Eq, namedType, type, Builtin.Boolean);
                    ProjectIndex.TypeIndex.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "lt":
                {
                    var type = Compilation.SearchContext.Infer(operatorSyntax.Types.FirstOrDefault());
                    var op = new BinaryOperator(TypeOperatorKind.Lt, namedType, type, Builtin.Boolean);
                    ProjectIndex.TypeIndex.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "le":
                {
                    var type = Compilation.SearchContext.Infer(operatorSyntax.Types.FirstOrDefault());
                    var op = new BinaryOperator(TypeOperatorKind.Le, namedType, type, Builtin.Boolean);
                    ProjectIndex.TypeIndex.AddTypeOperator(DocumentId, op);
                    break;
                }
            }
        }
    }

    private void AnalyzeTypeFields(LuaNamedType namedType, LuaDocTagSyntax typeTag)
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
                    AddDeclaration(declaration);
                    ProjectIndex.AddMember(DocumentId, namedType.Name, declaration);
                    break;
                }
                case { IntegerField: { } integerField, Type: { } type2 }:
                {
                    var type = Compilation.SearchContext.Infer(type2);
                    var declaration = new DocFieldDeclaration($"[{integerField.Value}]", GetPosition(integerField),
                        tagField, type);
                    AddDeclaration(declaration);
                    ProjectIndex.AddMember(DocumentId, namedType.Name, declaration);
                    break;
                }
                case { StringField: { } stringField, Type: { } type3 }:
                {
                    var type = Compilation.SearchContext.Infer(type3);
                    var declaration = new DocFieldDeclaration(stringField.Value, GetPosition(stringField),
                        tagField, type);
                    AddDeclaration(declaration);
                    ProjectIndex.AddMember(DocumentId, namedType.Name, declaration);
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

    private void AnalyzeTypeBody(LuaNamedType namedType, LuaDocTagBodySyntax docBody)
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
                    AddDeclaration(declaration);
                    ProjectIndex.AddMember(DocumentId, namedType.Name, declaration);
                    break;
                }
                case { IntegerField: { } integerField, Type: { } type2 }:
                {
                    var type = Compilation.SearchContext.Infer(type2);
                    var declaration = new DocFieldDeclaration($"[{integerField.Value}]", GetPosition(integerField),
                        field, type);
                    AddDeclaration(declaration);
                    ProjectIndex.AddMember(DocumentId, namedType.Name, declaration);
                    break;
                }
                case { StringField: { } stringField, Type: { } type3 }:
                {
                    var type = Compilation.SearchContext.Infer(type3);
                    var declaration = new DocFieldDeclaration(stringField.Value, GetPosition(stringField),
                        field, type);
                    AddDeclaration(declaration);
                    ProjectIndex.AddMember(DocumentId, namedType.Name, declaration);
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

    private void AnalyzeTypeSupers(IEnumerable<LuaDocTypeSyntax> extendList, LuaNamedType namedType)
    {
        foreach (var extend in extendList)
        {
            var type = Compilation.SearchContext.Infer(extend);
            ProjectIndex.AddSuper(DocumentId, namedType.Name, type);
        }
    }

    private void AnalyzeTypeGenericParam(LuaDocTagGenericDeclareListSyntax genericDeclareList,
        LuaNamedType namedType)
    {
        foreach (var param in genericDeclareList.Params)
        {
            if (param is { Name: { } name })
            {
                var type = Compilation.SearchContext.Infer(param.Type);
                var declaration = new GenericParameterDeclaration(name.RepresentText, GetPosition(name), param, type);
                ProjectIndex.AddGenericParam(DocumentId, namedType.Name, declaration);
            }
        }
    }

    private void AnalyzeTableFieldDeclaration(LuaTableFieldSyntax tableFieldSyntax)
    {
        if (tableFieldSyntax is { Name: { } fieldName, ParentTable: { } table, Value: { } value })
        {
            var parentId = table.UniqueId;
            // TODO get type from ---@field ---@type
            var declaration =
                new TableFieldDeclaration(fieldName, GetPosition(tableFieldSyntax), tableFieldSyntax, null);
            AddDeclaration(declaration);
            ProjectIndex.AddMember(DocumentId, parentId, declaration);
            var unResolveDeclaration =
                new UnResolvedDeclaration(declaration, new LuaExprRef(value), ResolveState.UnResolvedType);
            AddUnResolved(unResolveDeclaration);
        }
    }

    private void AnalyzeLuaTableType(LuaDocTableTypeSyntax luaDocTableTypeSyntax)
    {
        var className = luaDocTableTypeSyntax.UniqueId;
        var tableType = new LuaTableLiteralType(className);
        foreach (var fieldSyntax in luaDocTableTypeSyntax.FieldList)
        {
            if (fieldSyntax is { NameField: { } nameToken, Type: { } type1 })
            {
                var type = Compilation.SearchContext.Infer(type1);
                var declaration = new DocFieldDeclaration(nameToken.RepresentText, GetPosition(fieldSyntax),
                    fieldSyntax, type);
                AddDeclaration(declaration);
                ProjectIndex.AddMember(DocumentId, className, declaration);
            }
            else if (fieldSyntax is { IntegerField: { } integerField, Type: { } type2 })
            {
                var type = Compilation.SearchContext.Infer(type2);
                var declaration = new DocFieldDeclaration($"[{integerField.Value}]", GetPosition(fieldSyntax),
                    fieldSyntax, type);
                AddDeclaration(declaration);
                ProjectIndex.AddMember(DocumentId, className, declaration);
            }
            else if (fieldSyntax is { StringField: { } stringField, Type: { } type3 })
            {
                var type = Compilation.SearchContext.Infer(type3);
                var declaration = new DocFieldDeclaration(stringField.Value, GetPosition(fieldSyntax), fieldSyntax,
                    type);
                AddDeclaration(declaration);
                ProjectIndex.AddMember(DocumentId, className, declaration);
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

    private void AnalyzeLuaLabel(LuaLabelStatSyntax labelStatSyntax)
    {
        if (labelStatSyntax is { Name: { } name })
        {
            var labelDeclaration =
                new LabelDeclaration(name.RepresentText, GetPosition(labelStatSyntax), labelStatSyntax);
            AddDeclaration(labelDeclaration);
        }
    }

    private void AnalyzeSource(LuaSourceSyntax sourceSyntax)
    {
        if (sourceSyntax.Block is { } block)
        {
            AddUnResolved(new UnResolvedSource(DocumentId, block, ResolveState.UnResolveReturn));
        }
    }
}
