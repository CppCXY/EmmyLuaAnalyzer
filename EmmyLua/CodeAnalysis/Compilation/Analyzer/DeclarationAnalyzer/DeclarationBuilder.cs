using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Index;
using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Syntax.Tree;
using EmmyLua.CodeAnalysis.Syntax.Walker;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.DeclarationAnalyzer;

public class DeclarationBuilder : ILuaElementWalker
{
    private DeclarationScope? _topScope = null;

    private DeclarationScope? _curScope = null;

    private Stack<DeclarationScope> _scopeStack = new();

    private Dictionary<LuaSyntaxElement, DeclarationScope> _scopeOwners = new();

    private LuaDeclarationTree _tree;

    private LuaSyntaxTree _syntaxTree;

    private DeclarationAnalyzer Analyzer { get; }

    private LuaCompilation Compilation => Analyzer.Compilation;

    private ProjectIndex ProjectIndex => Compilation.ProjectIndex;

    private AnalyzeContext AnalyzeContext { get; }

    private SearchContext Context => Analyzer.Context;

    private LuaDocumentId DocumentId { get; }

    public LuaDeclarationTree Build()
    {
        _syntaxTree.SyntaxRoot.Accept(this);
        _tree.RootScope = _topScope;
        return _tree;
    }

    public DeclarationBuilder(
        LuaDocumentId documentId,
        LuaSyntaxTree tree,
        DeclarationAnalyzer analyzer,
        AnalyzeContext analyzeContext)
    {
        _syntaxTree = tree;
        _tree = new LuaDeclarationTree(tree, _scopeOwners);
        Analyzer = analyzer;
        DocumentId = documentId;
        AnalyzeContext = analyzeContext;
    }

    private LuaDeclaration? FindDeclaration(LuaNameExprSyntax nameExpr)
    {
        return FindScope(nameExpr)?.FindNameDeclaration(nameExpr);
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

    // TODO: replace property
    private static int GetPosition(LuaSyntaxElement element) => element.Position;

    private void AddDeclaration(LuaDeclaration luaDeclaration)
    {
        _curScope?.Add(luaDeclaration);
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
                SetScope(new LocalStatDeclarationScope(_tree, position), element);
                break;
            }
            case LuaRepeatStatSyntax:
            {
                SetScope(new RepeatStatDeclarationScope(_tree, position), element);
                break;
            }
            case LuaForRangeStatSyntax:
            {
                SetScope(new ForRangeStatDeclarationScope(_tree, position), element);
                break;
            }
            default:
            {
                SetScope(new DeclarationScope(_tree, position), element);
                break;
            }
        }
    }

    private void SetScope(DeclarationScope scope, LuaSyntaxElement element)
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
            case LuaTableExprSyntax tableSyntax:
            {
                AnalyzeTableExprDeclaration(tableSyntax);
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
            case LuaNameExprSyntax nameExpr:
            {
                IndexNameExpr(nameExpr);
                break;
            }
            case LuaIndexExprSyntax indexExpr:
            {
                IndexIndexExpr(indexExpr);
                break;
            }
            case LuaDocNameTypeSyntax docNameType:
            {
                IndexDocNameType(docNameType);
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
            or LuaClosureExprSyntax;

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
                var declaration =
                    new LocalLuaDeclaration(name.RepresentText, new(localName), luaType);
                AddDeclaration(declaration);
                var unResolveDeclaration =
                    new UnResolvedDeclaration(declaration, relatedExpr, ResolveState.UnResolvedType);
                AddUnResolved(unResolveDeclaration);
                if (i == 0)
                {
                    var definedType = FindFirstLocalOrAssignType(localStatSyntax);
                    if (definedType is not null)
                    {
                        declaration.DeclarationType = definedType;
                        declaration.IsTypeDefine = true;
                        unResolveDeclaration.IsTypeDeclaration = true;
                    }
                }
            }
        }
    }

    private List<ParameterLuaDeclaration> AnalyzeParamListDeclaration(LuaParamListSyntax paramListSyntax)
    {
        var parameters = new List<ParameterLuaDeclaration>();
        var dic = FindParamTypeDict(paramListSyntax);
        foreach (var param in paramListSyntax.Params)
        {
            if (param.Name is { } name)
            {
                var declaration = new ParameterLuaDeclaration(name.RepresentText, new(param), null);
                if (dic.TryGetValue(name.RepresentText, out var ty))
                {
                    declaration.DeclarationType = ty;
                }

                parameters.Add(declaration);
                AddDeclaration(declaration);
            }
            else if (param.IsVarArgs)
            {
                var declaration = new ParameterLuaDeclaration("...", new(param), null);
                if (dic.TryGetValue("...", out var ty))
                {
                    declaration.DeclarationType = ty;
                }

                parameters.Add(declaration);
                AddDeclaration(declaration);
            }
        }

        return parameters;
    }

    private LuaType GetRetType(IEnumerable<LuaDocTagSyntax>? docList)
    {
        var returnTypes = docList?.OfType<LuaDocTagReturnSyntax>()
            .SelectMany(tag => tag.TypeList).Select(Context.Infer).ToList();
        LuaType returnType = Builtin.Unknown;
        if (returnTypes is null)
        {
            return returnType;
        }

        if (returnTypes.Count == 1)
        {
            returnType = returnTypes[0];
        }
        else if (returnTypes.Count > 1)
        {
            returnType = new LuaMultiReturnType(returnTypes);
        }

        return returnType;
    }

    private void AnalyzeForRangeStatDeclaration(LuaForRangeStatSyntax forRangeStatSyntax)
    {
        var dic = FindParamTypeDict(forRangeStatSyntax);
        var parameters = new List<ParameterLuaDeclaration>();
        foreach (var param in forRangeStatSyntax.IteratorNames)
        {
            if (param.Name is { } name)
            {
                var declaration = new ParameterLuaDeclaration(name.RepresentText, new(param), null);
                if (dic.TryGetValue(name.RepresentText, out var ty))
                {
                    declaration.DeclarationType = ty;
                }

                AddDeclaration(declaration);
                parameters.Add(declaration);
            }
        }

        var unResolved = new UnResolvedForRangeParameter(parameters, forRangeStatSyntax.ExprList.ToList());
        AddUnResolved(unResolved);
    }

    private void AnalyzeForStatDeclaration(LuaForStatSyntax forStatSyntax)
    {
        if (forStatSyntax is { IteratorName.Name: { } name })
        {
            AddDeclaration(new ParameterLuaDeclaration(name.RepresentText,
                new(forStatSyntax.IteratorName),
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

        var tagNameTypeSyntax = docList.OfType<LuaDocTagNamedTypeSyntax>().FirstOrDefault();
        if (tagNameTypeSyntax is { Name.RepresentText: { } name })
        {
            return new LuaNamedType(name);
        }

        return null;
    }

    private List<LuaType> FindLocalOrAssignTypes(LuaStatSyntax stat) =>
    (
        from comment in stat.Comments
        from tagType in comment.DocList.OfType<LuaDocTagTypeSyntax>()
        from type in tagType.TypeList
        select Context.Infer(type)
    ).ToList();

    private Dictionary<string, LuaType> FindParamTypeDict(LuaSyntaxElement element)
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

        var dic = new Dictionary<string, LuaType>();

        foreach (var tagParamSyntax in docList.OfType<LuaDocTagParamSyntax>())
        {
            if (tagParamSyntax is { Name: { } name, Type: { } type, Nullable: { } nullable })
            {
                var ty = Context.Infer(type);
                if (nullable)
                {
                    ty = ty.Union(Builtin.Nil);
                }

                dic.TryAdd(name.RepresentText, ty);
            }
            else if (tagParamSyntax is { VarArgs: { } varArgs, Type: { } type2 })
            {
                var ty = Context.Infer(type2);
                dic.TryAdd("...", ty);
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
                            var declaration = new GlobalLuaDeclaration(name.RepresentText,
                                new(nameExpr),
                                luaType);
                            ProjectIndex.AddGlobal(DocumentId, name.RepresentText, declaration);
                            var unResolveDeclaration =
                                new UnResolvedDeclaration(declaration, relatedExpr, ResolveState.UnResolvedType);
                            AddUnResolved(unResolveDeclaration);

                            if (i == 0)
                            {
                                var definedType = FindFirstLocalOrAssignType(luaAssignStat);
                                if (definedType is not null)
                                {
                                    declaration.DeclarationType = definedType;
                                    declaration.IsTypeDefine = true;
                                    unResolveDeclaration.IsTypeDeclaration = true;
                                }
                            }

                            AddDeclaration(declaration);
                        }
                    }

                    break;
                }
                case LuaIndexExprSyntax indexExpr:
                {
                    if (indexExpr.Name is null)
                    {
                        break;
                    }

                    var declaration =
                        new IndexLuaDeclaration(indexExpr.Name, new(indexExpr), luaType);
                    if (i == 0)
                    {
                        var declarationType = FindFirstLocalOrAssignType(luaAssignStat);
                        if (declarationType is not null)
                        {
                            declaration.DeclarationType = declarationType;
                        }
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
                var declaration = new MethodLuaDeclaration(
                    name.RepresentText,
                    new(luaFuncStat.LocalName),
                    null,
                    new(luaFuncStat))
                {
                    Feature = DeclarationFeature.Local
                };
                AddDeclaration(declaration);
                var unResolved = new UnResolvedDeclaration(declaration, new LuaExprRef(closureExpr),
                    ResolveState.UnResolvedType);
                AddUnResolved(unResolved);
                break;
            }
            case { IsLocal: false, NameExpr.Name: { } name2, ClosureExpr: { } closureExpr }:
            {
                var prevDeclaration = FindDeclaration(luaFuncStat.NameExpr);
                if (prevDeclaration is null)
                {
                    var declaration = new MethodLuaDeclaration(
                        name2.RepresentText,
                        new(luaFuncStat.NameExpr),
                        null,
                        new(luaFuncStat))
                    {
                        Feature = DeclarationFeature.Global
                    };
                    ProjectIndex.AddGlobal(DocumentId, name2.RepresentText, declaration);
                    AddDeclaration(declaration);
                    var unResolved = new UnResolvedDeclaration(declaration, new LuaExprRef(closureExpr),
                        ResolveState.UnResolvedType);
                    AddUnResolved(unResolved);
                }

                break;
            }
            case { IsMethod: true, IndexExpr: { } indexExpr, ClosureExpr: { } closureExpr }:
            {
                if (indexExpr is { Name: { } name })
                {
                    var declaration = new MethodLuaDeclaration(
                        name,
                        new(indexExpr),
                        null,
                        new(luaFuncStat));
                    AddDeclaration(declaration);
                    var unResolved = new UnResolvedDeclaration(declaration, new LuaExprRef(closureExpr),
                        ResolveState.UnResolvedIndex | ResolveState.UnResolvedType);
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

        var genericParameters = new List<GenericParameterLuaDeclaration>();
        var docList = comment?.DocList.ToList();
        var generic = docList?.OfType<LuaDocTagGenericSyntax>().FirstOrDefault();
        if (generic is not null)
        {
            foreach (var param in generic.Params)
            {
                if (param is { Name: { } name })
                {
                    var declaration =
                        new GenericParameterLuaDeclaration(
                            name.RepresentText,
                            new(param),
                            null);
                    // AddDeclaration(declaration);
                    genericParameters.Add(declaration);
                }
            }
        }

        var overloads = docList?
            .OfType<LuaDocTagOverloadSyntax>()
            .Select(it => Context.Infer(it.TypeFunc))
            .Cast<LuaMethodType>()
            .Select(it => it.MainSignature).ToList();

        var parameters = new List<ParameterLuaDeclaration>();
        if (closureExprSyntax.ParamList is { } paramList)
        {
            PushScope(closureExprSyntax);
            parameters = AnalyzeParamListDeclaration(paramList);
            PopScope();
        }

        var mainRetType = GetRetType(docList);
        var isColonDefine = closureExprSyntax.Parent is LuaFuncStatSyntax { IsColonFunc: true };
        LuaMethodType method =
            genericParameters.Count != 0
                ? new LuaGenericMethodType(
                    genericParameters,
                    new LuaSignature(mainRetType, parameters), overloads, isColonDefine)
                : new LuaMethodType(new LuaSignature(mainRetType, parameters), overloads, isColonDefine);
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
            var declaration = new NamedTypeLuaDeclaration(
                name.RepresentText,
                new(tagClassSyntax),
                luaClass,
                NamedTypeKind.Class
            );
            AddDeclaration(declaration);
            ProjectIndex.AddType(DocumentId, name.RepresentText, declaration, NamedTypeKind.Class);

            AnalyzeTypeFields(luaClass, tagClassSyntax);
            AnalyzeTypeOperator(luaClass, tagClassSyntax);

            if (tagClassSyntax is { Body: { } body })
            {
                AnalyzeDocBody(luaClass, body);
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
            var baseTy = Context.Infer(type);
            var declaration = new NamedTypeLuaDeclaration(
                name.RepresentText,
                new(tagAliasSyntax),
                luaAlias,
                NamedTypeKind.Alias);
            AddDeclaration(declaration);
            ProjectIndex.AddAlias(DocumentId, name.RepresentText, baseTy, declaration);
        }
    }

    private void AnalyzeEnumTagDeclaration(LuaDocTagEnumSyntax tagEnumSyntax)
    {
        if (tagEnumSyntax is { Name: { } name })
        {
            var baseType = tagEnumSyntax.BaseType is { } type
                ? Context.Infer(type)
                : Builtin.Integer;
            var luaEnum = new LuaNamedType(name.RepresentText);
            var declaration = new NamedTypeLuaDeclaration(
                name.RepresentText,
                new(tagEnumSyntax),
                luaEnum,
                NamedTypeKind.Enum);
            AddDeclaration(declaration);
            ProjectIndex.AddEnum(DocumentId, name.RepresentText, baseType, declaration);
            foreach (var field in tagEnumSyntax.FieldList)
            {
                if (field is { Name: { } fieldName })
                {
                    var fieldDeclaration = new EnumFieldLuaDeclaration(
                        fieldName.RepresentText,
                        new(field),
                        baseType);
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
                new NamedTypeLuaDeclaration(
                    name.RepresentText,
                    new(tagInterfaceSyntax),
                    luaInterface,
                    NamedTypeKind.Interface
                );
            AddDeclaration(declaration);

            ProjectIndex.AddType(DocumentId, name.RepresentText, declaration, NamedTypeKind.Interface);
            AnalyzeTypeFields(luaInterface, tagInterfaceSyntax);
            AnalyzeTypeOperator(luaInterface, tagInterfaceSyntax);
            if (tagInterfaceSyntax is { Body: { } body })
            {
                AnalyzeDocBody(luaInterface, body);
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
                    var type = Context.Infer(operatorSyntax.Types.FirstOrDefault());
                    var retType = Context.Infer(operatorSyntax.ReturnType);
                    var opDeclaration = new TypeOpDeclaration(retType, new(operatorSyntax));
                    var op = new BinaryOperator(TypeOperatorKind.Add, namedType, type, retType, opDeclaration);
                    ProjectIndex.TypeOperatorStorage.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "sub":
                {
                    var type = Context.Infer(operatorSyntax.Types.FirstOrDefault());
                    var retType = Context.Infer(operatorSyntax.ReturnType);
                    var opDeclaration = new TypeOpDeclaration(retType, new(operatorSyntax));
                    var op = new BinaryOperator(TypeOperatorKind.Sub, namedType, type, retType, opDeclaration);
                    ProjectIndex.TypeOperatorStorage.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "mul":
                {
                    var type = Context.Infer(operatorSyntax.Types.FirstOrDefault());
                    var retType = Context.Infer(operatorSyntax.ReturnType);
                    var opDeclaration = new TypeOpDeclaration(retType, new(operatorSyntax));
                    var op = new BinaryOperator(TypeOperatorKind.Mul, namedType, type, retType, opDeclaration);
                    ProjectIndex.TypeOperatorStorage.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "div":
                {
                    var type = Context.Infer(operatorSyntax.Types.FirstOrDefault());
                    var retType = Context.Infer(operatorSyntax.ReturnType);
                    var opDeclaration = new TypeOpDeclaration(retType, new(operatorSyntax));
                    var op = new BinaryOperator(TypeOperatorKind.Div, namedType, type, retType, opDeclaration);
                    ProjectIndex.TypeOperatorStorage.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "mod":
                {
                    var type = Context.Infer(operatorSyntax.Types.FirstOrDefault());
                    var retType = Context.Infer(operatorSyntax.ReturnType);
                    var opDeclaration = new TypeOpDeclaration(retType, new(operatorSyntax));
                    var op = new BinaryOperator(TypeOperatorKind.Mod, namedType, type, retType, opDeclaration);
                    ProjectIndex.TypeOperatorStorage.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "pow":
                {
                    var type = Context.Infer(operatorSyntax.Types.FirstOrDefault());
                    var retType = Context.Infer(operatorSyntax.ReturnType);
                    var opDeclaration = new TypeOpDeclaration(retType, new(operatorSyntax));
                    var op = new BinaryOperator(TypeOperatorKind.Pow, namedType, type, retType, opDeclaration);
                    ProjectIndex.TypeOperatorStorage.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "unm":
                {
                    var retType = Context.Infer(operatorSyntax.ReturnType);
                    var opDeclaration = new TypeOpDeclaration(retType, new(operatorSyntax));
                    var op = new UnaryOperator(TypeOperatorKind.Unm, namedType, retType, opDeclaration);
                    ProjectIndex.TypeOperatorStorage.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "idiv":
                {
                    var type = Context.Infer(operatorSyntax.Types.FirstOrDefault());
                    var retType = Context.Infer(operatorSyntax.ReturnType);
                    var opDeclaration = new TypeOpDeclaration(retType, new(operatorSyntax));
                    var op = new BinaryOperator(TypeOperatorKind.Idiv, namedType, type, retType, opDeclaration);
                    ProjectIndex.TypeOperatorStorage.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "band":
                {
                    var type = Context.Infer(operatorSyntax.Types.FirstOrDefault());
                    var retType = Context.Infer(operatorSyntax.ReturnType);
                    var opDeclaration = new TypeOpDeclaration(retType, new(operatorSyntax));
                    var op = new BinaryOperator(TypeOperatorKind.Band, namedType, type, retType, opDeclaration);
                    ProjectIndex.TypeOperatorStorage.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "bor":
                {
                    var type = Context.Infer(operatorSyntax.Types.FirstOrDefault());
                    var retType = Context.Infer(operatorSyntax.ReturnType);
                    var opDeclaration = new TypeOpDeclaration(retType, new(operatorSyntax));
                    var op = new BinaryOperator(TypeOperatorKind.Bor, namedType, type, retType, opDeclaration);
                    ProjectIndex.TypeOperatorStorage.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "bxor":
                {
                    var type = Context.Infer(operatorSyntax.Types.FirstOrDefault());
                    var retType = Context.Infer(operatorSyntax.ReturnType);
                    var opDeclaration = new TypeOpDeclaration(retType, new(operatorSyntax));
                    var op = new BinaryOperator(TypeOperatorKind.Bxor, namedType, type, retType, opDeclaration);
                    ProjectIndex.TypeOperatorStorage.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "bnot":
                {
                    var retType = Context.Infer(operatorSyntax.ReturnType);
                    var opDeclaration = new TypeOpDeclaration(retType, new(operatorSyntax));
                    var op = new UnaryOperator(TypeOperatorKind.Bnot, namedType, retType, opDeclaration);
                    ProjectIndex.TypeOperatorStorage.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "shl":
                {
                    var type = Context.Infer(operatorSyntax.Types.FirstOrDefault());
                    var retType = Context.Infer(operatorSyntax.ReturnType);
                    var opDeclaration = new TypeOpDeclaration(retType, new(operatorSyntax));
                    var op = new BinaryOperator(TypeOperatorKind.Shl, namedType, type, retType, opDeclaration);
                    ProjectIndex.TypeOperatorStorage.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "shr":
                {
                    var type = Context.Infer(operatorSyntax.Types.FirstOrDefault());
                    var retType = Context.Infer(operatorSyntax.ReturnType);
                    var opDeclaration = new TypeOpDeclaration(retType, new(operatorSyntax));
                    var op = new BinaryOperator(TypeOperatorKind.Shr, namedType, type, retType, opDeclaration);
                    ProjectIndex.TypeOperatorStorage.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "concat":
                {
                    var type = Context.Infer(operatorSyntax.Types.FirstOrDefault());
                    var retType = Context.Infer(operatorSyntax.ReturnType);
                    var opDeclaration = new TypeOpDeclaration(retType, new(operatorSyntax));
                    var op = new BinaryOperator(TypeOperatorKind.Concat, namedType, type, retType, opDeclaration);
                    ProjectIndex.TypeOperatorStorage.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "len":
                {
                    var retType = Context.Infer(operatorSyntax.ReturnType);
                    var opDeclaration = new TypeOpDeclaration(retType, new(operatorSyntax));
                    var op = new UnaryOperator(TypeOperatorKind.Len, namedType, retType, opDeclaration);
                    ProjectIndex.TypeOperatorStorage.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "eq":
                {
                    var type = Context.Infer(operatorSyntax.Types.FirstOrDefault());
                    var opDeclaration = new TypeOpDeclaration(type, new(operatorSyntax));
                    var op = new BinaryOperator(TypeOperatorKind.Eq, namedType, type, Builtin.Boolean, opDeclaration);
                    ProjectIndex.TypeOperatorStorage.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "lt":
                {
                    var type = Context.Infer(operatorSyntax.Types.FirstOrDefault());
                    var opDeclaration = new TypeOpDeclaration(type, new(operatorSyntax));
                    var op = new BinaryOperator(TypeOperatorKind.Lt, namedType, type, Builtin.Boolean, opDeclaration);
                    ProjectIndex.TypeOperatorStorage.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "le":
                {
                    var type = Context.Infer(operatorSyntax.Types.FirstOrDefault());
                    var opDeclaration = new TypeOpDeclaration(type, new(operatorSyntax));
                    var op = new BinaryOperator(TypeOperatorKind.Le, namedType, type, Builtin.Boolean, opDeclaration);
                    ProjectIndex.TypeOperatorStorage.AddTypeOperator(DocumentId, op);
                    break;
                }
            }
        }
    }

    private void AnalyzeDocDetailField(LuaNamedType namedType, LuaDocFieldSyntax field)
    {
        switch (field)
        {
            case { NameField: { } nameField, Type: { } type1 }:
            {
                var type = Context.Infer(type1);
                var declaration = new DocFieldLuaDeclaration(
                    nameField.RepresentText,
                    new(field),
                    type);
                ProjectIndex.AddMember(DocumentId, namedType.Name, declaration);
                break;
            }
            case { IntegerField: { } integerField, Type: { } type2 }:
            {
                var type = Context.Infer(type2);
                var declaration = new DocFieldLuaDeclaration(
                    $"[{integerField.Value}]",
                    new(field),
                    type);
                ProjectIndex.AddMember(DocumentId, namedType.Name, declaration);
                break;
            }
            case { StringField: { } stringField, Type: { } type3 }:
            {
                var type = Context.Infer(type3);
                var declaration = new DocFieldLuaDeclaration(
                    stringField.Value,
                    new(field),
                    type);
                ProjectIndex.AddMember(DocumentId, namedType.Name, declaration);
                break;
            }
            case { TypeField: { } typeField, Type: { } type4 }:
            {
                var keyType = Context.Infer(typeField);
                var valueType = Context.Infer(type4);
                var docIndexDeclaration = new TypeIndexDeclaration(keyType, valueType, new(field));
                var indexOperator = new IndexOperator(namedType, keyType, valueType, docIndexDeclaration);
                ProjectIndex.TypeOperatorStorage.AddTypeOperator(DocumentId, indexOperator);
                break;
            }
        }
    }

    private void AnalyzeTypeFields(LuaNamedType namedType, LuaDocTagSyntax typeTag)
    {
        foreach (var tagField in typeTag.NextOfType<LuaDocTagFieldSyntax>())
        {
            if (tagField.Field is not null)
            {
                AnalyzeDocDetailField(namedType, tagField.Field);
            }
        }
    }

    private void AnalyzeDocBody(LuaNamedType namedType, LuaDocBodySyntax docBody)
    {
        foreach (var field in docBody.FieldList)
        {
            AnalyzeDocDetailField(namedType, field);
        }
    }

    private void AnalyzeTypeSupers(IEnumerable<LuaDocTypeSyntax> extendList, LuaNamedType namedType)
    {
        foreach (var extend in extendList)
        {
            var type = Context.Infer(extend);
            ProjectIndex.AddSuper(DocumentId, namedType.Name, type);
        }
    }

    private void AnalyzeTypeGenericParam(LuaDocGenericDeclareListSyntax generic,
        LuaNamedType namedType)
    {
        foreach (var param in generic.Params)
        {
            if (param is { Name: { } name })
            {
                var type = param.Type is not null ? Context.Infer(param.Type) : null;
                var declaration =
                    new GenericParameterLuaDeclaration(name.RepresentText, new(param), type);
                ProjectIndex.AddGenericParam(DocumentId, namedType.Name, declaration);
            }
        }
    }

    private void AnalyzeTableExprDeclaration(LuaTableExprSyntax tableExprSyntax)
    {
        var tableUniqueId = tableExprSyntax.UniqueId;
        foreach (var fieldSyntax in tableExprSyntax.FieldList)
        {
            if (fieldSyntax is { Name: { } fieldName, Value: { } value })
            {
                // TODO get type from ---@field ---@type
                var declaration =
                    new TableFieldLuaDeclaration(fieldName, new(fieldSyntax), null);
                ProjectIndex.AddMember(DocumentId, tableUniqueId, declaration);
                var unResolveDeclaration =
                    new UnResolvedDeclaration(declaration, new LuaExprRef(value), ResolveState.UnResolvedType);
                AddUnResolved(unResolveDeclaration);
            }
        }
    }

    private void AnalyzeLuaTableType(LuaDocTableTypeSyntax luaDocTableTypeSyntax)
    {
        var className = luaDocTableTypeSyntax.UniqueId;
        var tableType = new LuaNamedType(className);
        if (luaDocTableTypeSyntax.Body is not null)
        {
            AnalyzeDocBody(tableType, luaDocTableTypeSyntax.Body);
        }
    }

    private void AnalyzeLuaLabel(LuaLabelStatSyntax labelStatSyntax)
    {
        if (labelStatSyntax is { Name: { } name })
        {
            var labelDeclaration =
                new LabelLuaDeclaration(name.RepresentText, new(labelStatSyntax));
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

    private void IndexNameExpr(LuaNameExprSyntax nameExpr)
    {
        ProjectIndex.AddNameExpr(DocumentId, nameExpr);
    }

    private void IndexIndexExpr(LuaIndexExprSyntax indexExpr)
    {
        ProjectIndex.AddIndexExpr(DocumentId, indexExpr);
    }

    private void IndexDocNameType(LuaDocNameTypeSyntax docNameType)
    {
        ProjectIndex.AddNameType(DocumentId, docNameType);
    }
}
