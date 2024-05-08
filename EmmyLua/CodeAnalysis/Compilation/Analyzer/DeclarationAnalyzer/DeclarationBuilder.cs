using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Index;
using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Document.Version;
using EmmyLua.CodeAnalysis.Kind;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Syntax.Tree;
using EmmyLua.CodeAnalysis.Syntax.Walker;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.DeclarationAnalyzer;

public class DeclarationBuilder : ILuaElementWalker
{
    private DeclarationScope? _topScope;

    private DeclarationScope? _curScope;

    private Stack<DeclarationScope> _scopeStack = new();

    private Dictionary<LuaSyntaxElement, DeclarationScope> _scopeOwners = new();

    private LuaDeclarationTree _tree;

    private LuaSyntaxTree _syntaxTree;

    private DeclarationAnalyzer Analyzer { get; }

    private LuaCompilation Compilation => Analyzer.Compilation;

    private DbManager DbManager => Compilation.DbManager;

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

        var position = element.Position;
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
            case LuaDocTagMetaSyntax:
            {
                Compilation.Diagnostics.AddMeta(DocumentId);
                break;
            }
            case LuaDocTagDiagnosticSyntax diagnosticSyntax:
            {
                AnalyzeDiagnostic(diagnosticSyntax);
                break;
            }
            case LuaDocTagModuleSyntax moduleSyntax:
            {
                AnalyzeModule(moduleSyntax);
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
                var declaration = new LuaDeclaration(
                    name.RepresentText,
                    localName.Position,
                    new LocalInfo(
                        new(localName),
                        luaType,
                        false,
                        localName.Attribute?.IsConst ?? false,
                        localName.Attribute?.IsClose ?? false
                    ),
                    DeclarationFeature.Local
                );
                AddDeclaration(declaration);
                var unResolveDeclaration =
                    new UnResolvedDeclaration(declaration, relatedExpr, ResolveState.UnResolvedType);
                AddUnResolved(unResolveDeclaration);
                if (i == 0)
                {
                    var definedType = FindFirstLocalOrAssignType(localStatSyntax);
                    if (definedType is not null && declaration.Info is LocalInfo info)
                    {
                        declaration.Info = info with
                        {
                            DeclarationType = definedType,
                            IsTypeDefine = true
                        };
                        unResolveDeclaration.IsTypeDeclaration = true;
                    }
                }
            }
        }
    }

    private List<LuaDeclaration> AnalyzeParamListDeclaration(LuaParamListSyntax paramListSyntax)
    {
        var parameters = new List<LuaDeclaration>();
        var paramDict = FindParamDict(paramListSyntax);
        foreach (var param in paramListSyntax.Params)
        {
            if (param.Name is { } name)
            {
                LuaType? luaType = null;
                var nullable = false;
                if (paramDict.TryGetValue(name.RepresentText, out var paramInfo))
                {
                    luaType = paramInfo.Item1;
                    nullable = paramInfo.Item2;
                }

                var declaration = new LuaDeclaration(
                    name.RepresentText,
                    param.Position,
                    new ParamInfo(
                        new(param),
                        luaType,
                        false,
                        nullable
                    ),
                    DeclarationFeature.Local
                );

                parameters.Add(declaration);
                AddDeclaration(declaration);
            }
            else if (param.IsVarArgs)
            {
                LuaType? luaType = null;
                var nullable = false;
                if (paramDict.TryGetValue("...", out var paramInfo))
                {
                    luaType = paramInfo.Item1;
                    nullable = paramInfo.Item2;
                }

                var declaration = new LuaDeclaration(
                    "...",
                    param.Position,
                    new ParamInfo(
                        new(param),
                        luaType,
                        false,
                        nullable
                    ),
                    DeclarationFeature.Local
                );

                parameters.Add(declaration);
                AddDeclaration(declaration);
            }
        }

        return parameters;
    }

    private LuaType GetRetType(IEnumerable<LuaDocTagSyntax>? docList)
    {
        var returnTypes = docList?
            .OfType<LuaDocTagReturnSyntax>()
            .SelectMany(tag => tag.TypeList)
            .Select(Context.Infer)
            .ToList();
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
        var dic = FindParamDict(forRangeStatSyntax);
        var parameters = new List<LuaDeclaration>();
        foreach (var param in forRangeStatSyntax.IteratorNames)
        {
            if (param.Name is { } name)
            {
                var declaration = new LuaDeclaration(
                    name.RepresentText,
                    param.Position,
                    new ParamInfo(
                        new(param),
                        null,
                        false
                    ),
                    DeclarationFeature.Local);
                if (dic.TryGetValue(name.RepresentText, out var paramInfo))
                {
                    declaration.Info = declaration.Info with { DeclarationType = paramInfo.Item1 };
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
            AddDeclaration(new LuaDeclaration(
                name.RepresentText,
                forStatSyntax.IteratorName.Position,
                new ParamInfo(
                    new(forStatSyntax.IteratorName),
                    Builtin.Integer,
                    false
                ),
                DeclarationFeature.Local));
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

    private Dictionary<string, (LuaType, bool /* nullable*/)> FindParamDict(LuaSyntaxElement element)
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

        var dic = new Dictionary<string, (LuaType, bool /* nullable*/)>();

        foreach (var tagParamSyntax in docList.OfType<LuaDocTagParamSyntax>())
        {
            if (tagParamSyntax is { Name: { } name, Type: { } type, Nullable: { } nullable })
            {
                var ty = Context.Infer(type);
                if (nullable)
                {
                    ty = ty.Union(Builtin.Nil);
                }

                dic.TryAdd(name.RepresentText, (ty, nullable));
            }
            else if (tagParamSyntax is { VarArgs: { } _, Type: { } type2, Nullable: { } nullable2 })
            {
                var ty = Context.Infer(type2);
                dic.TryAdd("...", (ty, nullable2));
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
                            var declaration = new LuaDeclaration(
                                name.RepresentText,
                                nameExpr.Position,
                                new GlobalInfo(
                                    new(nameExpr),
                                    luaType,
                                    false
                                ),
                                DeclarationFeature.Global
                            );
                            AnalyzeDeclarationDoc(declaration, luaAssignStat);
                            DbManager.AddGlobal(DocumentId, name.RepresentText, declaration);
                            var unResolveDeclaration =
                                new UnResolvedDeclaration(declaration, relatedExpr, ResolveState.UnResolvedType);
                            AddUnResolved(unResolveDeclaration);

                            if (i == 0)
                            {
                                var definedType = FindFirstLocalOrAssignType(luaAssignStat);
                                if (definedType is not null && declaration.Info is GlobalInfo info)
                                {
                                    declaration.Info = info with
                                    {
                                        DeclarationType = definedType,
                                        IsTypeDefine = true
                                    };
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

                    var valueExprPtr = relatedExpr?.RetId == 0
                        ? new(relatedExpr.Expr)
                        : LuaElementPtr<LuaExprSyntax>.Empty;
                    var declaration = new LuaDeclaration(
                        indexExpr.Name,
                        indexExpr.Position,
                        new IndexInfo(
                            new(indexExpr),
                            valueExprPtr,
                            luaType
                        )
                    );
                    AnalyzeDeclarationDoc(declaration, luaAssignStat);
                    if (i == 0)
                    {
                        var declarationType = FindFirstLocalOrAssignType(luaAssignStat);
                        if (declarationType is not null)
                        {
                            declaration.Info = declaration.Info with { DeclarationType = declarationType };
                        }
                    }

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
                var declaration = new LuaDeclaration(
                    name.RepresentText,
                    luaFuncStat.LocalName.Position,
                    new MethodInfo(
                        new(luaFuncStat.LocalName),
                        null,
                        new(luaFuncStat)
                    ),
                    DeclarationFeature.Local
                );
                AnalyzeDeclarationDoc(declaration, luaFuncStat);
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
                    var declaration = new LuaDeclaration(
                        name2.RepresentText,
                        luaFuncStat.NameExpr.Position,
                        new MethodInfo(
                            new(luaFuncStat.NameExpr),
                            null,
                            new(luaFuncStat)
                        ),
                        DeclarationFeature.Global
                    );
                    AnalyzeDeclarationDoc(declaration, luaFuncStat);
                    DbManager.AddGlobal(DocumentId, name2.RepresentText, declaration);
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
                    var declaration = new LuaDeclaration(
                        name,
                        indexExpr.Position,
                        new MethodInfo(
                            new(indexExpr),
                            null,
                            new(luaFuncStat)
                        )
                    );
                    AnalyzeDeclarationDoc(declaration, luaFuncStat);
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

        var genericParameters = new List<LuaDeclaration>();
        var docList = comment?.DocList.ToList();
        var generic = docList?.OfType<LuaDocTagGenericSyntax>().FirstOrDefault();
        if (generic is not null)
        {
            var genericParams = generic.Params.ToList();
            for (var i = 0; i < genericParams.Count; i++)
            {
                var variadic = i == genericParams.Count - 1 && generic.Variadic;
                var param = genericParams[i];
                if (param is { Name: { } name })
                {
                    var declaration = new LuaDeclaration(
                        name.RepresentText,
                        param.Position,
                        new GenericParamInfo(
                            new(param),
                            Context.Infer(param.Type),
                            variadic
                        )
                    );
                    genericParameters.Add(declaration);
                }
            }
        }

        var overloads = docList?
            .OfType<LuaDocTagOverloadSyntax>()
            .Where(it => it.TypeFunc is not null)
            .Select(it => Context.Infer(it.TypeFunc))
            .Cast<LuaMethodType>()
            .Select(it => it.MainSignature).ToList();

        var parameters = new List<LuaDeclaration>();
        if (closureExprSyntax.ParamList is { } paramList)
        {
            PushScope(closureExprSyntax);
            parameters = AnalyzeParamListDeclaration(paramList);
            PopScope();
        }

        if (closureExprSyntax.Parent is LuaCallArgListSyntax { Parent: LuaCallExprSyntax callExprSyntax } callArgList)
        {
            var index = callArgList.ArgList.ToList().IndexOf(closureExprSyntax);
            var unResolved = new UnResolvedClosureParameters(parameters, callExprSyntax, index);
            AddUnResolved(unResolved);
        }

        var mainRetType = GetRetType(docList);
        var isColonDefine = closureExprSyntax.Parent is LuaFuncStatSyntax { IsColonFunc: true };
        var method = genericParameters.Count != 0
            ? new LuaGenericMethodType(
                genericParameters,
                new LuaSignature(mainRetType, parameters), overloads, isColonDefine)
            : new LuaMethodType(new LuaSignature(mainRetType, parameters), overloads, isColonDefine);
        if (closureExprSyntax.Block is { } block)
        {
            var unResolved = new UnResolvedMethod(method, block, ResolveState.UnResolveReturn);
            AddUnResolved(unResolved);
        }

        DbManager.AddIdRelatedType(DocumentId, closureExprSyntax.UniqueId, method);
    }

    private void AnalyzeClassTagDeclaration(LuaDocTagClassSyntax tagClassSyntax)
    {
        if (tagClassSyntax is { Name: { } name })
        {
            var luaClass = new LuaNamedType(name.RepresentText);
            var declaration = new LuaDeclaration(
                name.RepresentText,
                tagClassSyntax.Position,
                new NamedTypeInfo(
                    new(tagClassSyntax),
                    luaClass,
                    NamedTypeKind.Class
                )
            );

            DbManager.AddType(DocumentId, name.RepresentText, declaration);

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
            var declaration = new LuaDeclaration(
                name.RepresentText,
                tagAliasSyntax.Position,
                new NamedTypeInfo(
                    new(tagAliasSyntax),
                    luaAlias,
                    NamedTypeKind.Alias
                ));
            DbManager.AddAlias(DocumentId, name.RepresentText, baseTy, declaration);
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
            var declaration = new LuaDeclaration(
                name.RepresentText,
                tagEnumSyntax.Position,
                new NamedTypeInfo(
                    new(tagEnumSyntax),
                    luaEnum,
                    NamedTypeKind.Enum
                ));

            DbManager.AddEnum(DocumentId, name.RepresentText, baseType, declaration);
            foreach (var field in tagEnumSyntax.FieldList)
            {
                if (field is { Name: { } fieldName })
                {
                    var fieldDeclaration = new LuaDeclaration(
                        fieldName.RepresentText,
                        field.Position,
                        new EnumFieldInfo(
                            new(field),
                            baseType
                        ));
                    DbManager.AddMember(DocumentId, name.RepresentText, fieldDeclaration);
                }
            }
        }
    }

    private void AnalyzeInterfaceTagDeclaration(LuaDocTagInterfaceSyntax tagInterfaceSyntax)
    {
        if (tagInterfaceSyntax is { Name: { } name })
        {
            var luaInterface = new LuaNamedType(name.RepresentText);
            var declaration = new LuaDeclaration(
                name.RepresentText,
                tagInterfaceSyntax.Position,
                new NamedTypeInfo(
                    new(tagInterfaceSyntax),
                    luaInterface,
                    NamedTypeKind.Interface
                ));


            DbManager.AddType(DocumentId, name.RepresentText, declaration);
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
                    var opDeclaration = new LuaDeclaration(
                        string.Empty,
                        operatorSyntax.Position,
                        new TypeOpInfo(
                            new(operatorSyntax),
                            retType
                        ));
                    var op = new BinaryOperator(TypeOperatorKind.Add, namedType, type, retType, opDeclaration);
                    DbManager.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "sub":
                {
                    var type = Context.Infer(operatorSyntax.Types.FirstOrDefault());
                    var retType = Context.Infer(operatorSyntax.ReturnType);
                    var opDeclaration = new LuaDeclaration(
                        string.Empty,
                        operatorSyntax.Position,
                        new TypeOpInfo(
                            new(operatorSyntax),
                            retType
                        ));
                    var op = new BinaryOperator(TypeOperatorKind.Sub, namedType, type, retType, opDeclaration);
                    DbManager.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "mul":
                {
                    var type = Context.Infer(operatorSyntax.Types.FirstOrDefault());
                    var retType = Context.Infer(operatorSyntax.ReturnType);
                    var opDeclaration = new LuaDeclaration(
                        string.Empty,
                        operatorSyntax.Position,
                        new TypeOpInfo(
                            new(operatorSyntax),
                            retType
                        ));
                    var op = new BinaryOperator(TypeOperatorKind.Mul, namedType, type, retType, opDeclaration);
                    DbManager.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "div":
                {
                    var type = Context.Infer(operatorSyntax.Types.FirstOrDefault());
                    var retType = Context.Infer(operatorSyntax.ReturnType);
                    var opDeclaration = new LuaDeclaration(
                        string.Empty,
                        operatorSyntax.Position,
                        new TypeOpInfo(
                            new(operatorSyntax),
                            retType
                        ));
                    var op = new BinaryOperator(TypeOperatorKind.Div, namedType, type, retType, opDeclaration);
                    DbManager.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "mod":
                {
                    var type = Context.Infer(operatorSyntax.Types.FirstOrDefault());
                    var retType = Context.Infer(operatorSyntax.ReturnType);
                    var opDeclaration = new LuaDeclaration(
                        string.Empty,
                        operatorSyntax.Position,
                        new TypeOpInfo(
                            new(operatorSyntax),
                            retType
                        ));
                    var op = new BinaryOperator(TypeOperatorKind.Mod, namedType, type, retType, opDeclaration);
                    DbManager.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "pow":
                {
                    var type = Context.Infer(operatorSyntax.Types.FirstOrDefault());
                    var retType = Context.Infer(operatorSyntax.ReturnType);
                    var opDeclaration = new LuaDeclaration(
                        string.Empty,
                        operatorSyntax.Position,
                        new TypeOpInfo(
                            new(operatorSyntax),
                            retType
                        ));
                    var op = new BinaryOperator(TypeOperatorKind.Pow, namedType, type, retType, opDeclaration);
                    DbManager.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "unm":
                {
                    var retType = Context.Infer(operatorSyntax.ReturnType);
                    var opDeclaration = new LuaDeclaration(
                        string.Empty,
                        operatorSyntax.Position,
                        new TypeOpInfo(
                            new(operatorSyntax),
                            retType
                        ));
                    var op = new UnaryOperator(TypeOperatorKind.Unm, namedType, retType, opDeclaration);
                    DbManager.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "idiv":
                {
                    var type = Context.Infer(operatorSyntax.Types.FirstOrDefault());
                    var retType = Context.Infer(operatorSyntax.ReturnType);
                    var opDeclaration = new LuaDeclaration(
                        string.Empty,
                        operatorSyntax.Position,
                        new TypeOpInfo(
                            new(operatorSyntax),
                            retType
                        ));
                    var op = new BinaryOperator(TypeOperatorKind.Idiv, namedType, type, retType, opDeclaration);
                    DbManager.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "band":
                {
                    var type = Context.Infer(operatorSyntax.Types.FirstOrDefault());
                    var retType = Context.Infer(operatorSyntax.ReturnType);
                    var opDeclaration = new LuaDeclaration(
                        string.Empty,
                        operatorSyntax.Position,
                        new TypeOpInfo(
                            new(operatorSyntax),
                            retType
                        ));
                    var op = new BinaryOperator(TypeOperatorKind.Band, namedType, type, retType, opDeclaration);
                    DbManager.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "bor":
                {
                    var type = Context.Infer(operatorSyntax.Types.FirstOrDefault());
                    var retType = Context.Infer(operatorSyntax.ReturnType);
                    var opDeclaration = new LuaDeclaration(
                        string.Empty,
                        operatorSyntax.Position,
                        new TypeOpInfo(
                            new(operatorSyntax),
                            retType
                        ));
                    var op = new BinaryOperator(TypeOperatorKind.Bor, namedType, type, retType, opDeclaration);
                    DbManager.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "bxor":
                {
                    var type = Context.Infer(operatorSyntax.Types.FirstOrDefault());
                    var retType = Context.Infer(operatorSyntax.ReturnType);
                    var opDeclaration = new LuaDeclaration(
                        string.Empty,
                        operatorSyntax.Position,
                        new TypeOpInfo(
                            new(operatorSyntax),
                            retType
                        ));
                    var op = new BinaryOperator(TypeOperatorKind.Bxor, namedType, type, retType, opDeclaration);
                    DbManager.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "bnot":
                {
                    var retType = Context.Infer(operatorSyntax.ReturnType);
                    var opDeclaration = new LuaDeclaration(
                        string.Empty,
                        operatorSyntax.Position,
                        new TypeOpInfo(
                            new(operatorSyntax),
                            retType
                        ));
                    var op = new UnaryOperator(TypeOperatorKind.Bnot, namedType, retType, opDeclaration);
                    DbManager.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "shl":
                {
                    var type = Context.Infer(operatorSyntax.Types.FirstOrDefault());
                    var retType = Context.Infer(operatorSyntax.ReturnType);
                    var opDeclaration = new LuaDeclaration(
                        string.Empty,
                        operatorSyntax.Position,
                        new TypeOpInfo(
                            new(operatorSyntax),
                            retType
                        ));
                    var op = new BinaryOperator(TypeOperatorKind.Shl, namedType, type, retType, opDeclaration);
                    DbManager.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "shr":
                {
                    var type = Context.Infer(operatorSyntax.Types.FirstOrDefault());
                    var retType = Context.Infer(operatorSyntax.ReturnType);
                    var opDeclaration = new LuaDeclaration(
                        string.Empty,
                        operatorSyntax.Position,
                        new TypeOpInfo(
                            new(operatorSyntax),
                            retType
                        ));
                    var op = new BinaryOperator(TypeOperatorKind.Shr, namedType, type, retType, opDeclaration);
                    DbManager.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "concat":
                {
                    var type = Context.Infer(operatorSyntax.Types.FirstOrDefault());
                    var retType = Context.Infer(operatorSyntax.ReturnType);
                    var opDeclaration = new LuaDeclaration(
                        string.Empty,
                        operatorSyntax.Position,
                        new TypeOpInfo(
                            new(operatorSyntax),
                            retType
                        ));
                    var op = new BinaryOperator(TypeOperatorKind.Concat, namedType, type, retType, opDeclaration);
                    DbManager.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "len":
                {
                    var retType = Context.Infer(operatorSyntax.ReturnType);
                    var opDeclaration = new LuaDeclaration(
                        string.Empty,
                        operatorSyntax.Position,
                        new TypeOpInfo(
                            new(operatorSyntax),
                            retType
                        ));
                    var op = new UnaryOperator(TypeOperatorKind.Len, namedType, retType, opDeclaration);
                    DbManager.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "eq":
                {
                    var type = Context.Infer(operatorSyntax.Types.FirstOrDefault());
                    var opDeclaration = new LuaDeclaration(
                        string.Empty,
                        operatorSyntax.Position,
                        new TypeOpInfo(
                            new(operatorSyntax),
                            type
                        ));
                    var op = new BinaryOperator(TypeOperatorKind.Eq, namedType, type, Builtin.Boolean, opDeclaration);
                    DbManager.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "lt":
                {
                    var type = Context.Infer(operatorSyntax.Types.FirstOrDefault());
                    var opDeclaration = new LuaDeclaration(
                        string.Empty,
                        operatorSyntax.Position,
                        new TypeOpInfo(
                            new(operatorSyntax),
                            type
                        ));
                    var op = new BinaryOperator(TypeOperatorKind.Lt, namedType, type, Builtin.Boolean, opDeclaration);
                    DbManager.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "le":
                {
                    var type = Context.Infer(operatorSyntax.Types.FirstOrDefault());
                    var opDeclaration = new LuaDeclaration(
                        string.Empty,
                        operatorSyntax.Position,
                        new TypeOpInfo(
                            new(operatorSyntax),
                            type
                        ));
                    var op = new BinaryOperator(TypeOperatorKind.Le, namedType, type, Builtin.Boolean, opDeclaration);
                    DbManager.AddTypeOperator(DocumentId, op);
                    break;
                }
            }
        }
    }

    private DeclarationVisibility GetVisibility(VisibilityKind visibility)
    {
        return visibility switch
        {
            VisibilityKind.Public => DeclarationVisibility.Public,
            VisibilityKind.Protected => DeclarationVisibility.Protected,
            VisibilityKind.Private => DeclarationVisibility.Private,
            _ => DeclarationVisibility.Public
        };
    }

    private void AnalyzeDocDetailField(LuaNamedType namedType, LuaDocFieldSyntax field)
    {
        var visibility = field.Visibility;
        switch (field)
        {
            case { NameField: { } nameField, Type: { } type1 }:
            {
                var type = Context.Infer(type1);
                var declaration = new LuaDeclaration(
                    nameField.RepresentText,
                    field.Position,
                    new DocFieldInfo(
                        new(field),
                        type),
                    DeclarationFeature.None,
                    GetVisibility(visibility)
                );
                DbManager.AddMember(DocumentId, namedType.Name, declaration);
                break;
            }
            case { IntegerField: { } integerField, Type: { } type2 }:
            {
                var type = Context.Infer(type2);
                var declaration = new LuaDeclaration(
                    $"[{integerField.Value}]",
                    field.Position,
                    new DocFieldInfo(
                        new(field),
                        type
                    ),
                    DeclarationFeature.None,
                    GetVisibility(visibility)
                );
                DbManager.AddMember(DocumentId, namedType.Name, declaration);
                break;
            }
            case { StringField: { } stringField, Type: { } type3 }:
            {
                var type = Context.Infer(type3);
                var declaration = new LuaDeclaration(
                    stringField.Value,
                    field.Position,
                    new DocFieldInfo(
                        new(field),
                        type),
                    DeclarationFeature.None,
                    GetVisibility(visibility)
                );
                DbManager.AddMember(DocumentId, namedType.Name, declaration);
                break;
            }
            case { TypeField: { } typeField, Type: { } type4 }:
            {
                var keyType = Context.Infer(typeField);
                var valueType = Context.Infer(type4);
                var docIndexDeclaration = new LuaDeclaration(
                    string.Empty,
                    field.Position,
                    new TypeIndexInfo(
                        keyType,
                        valueType,
                        new(field)
                    ));
                var indexOperator = new IndexOperator(namedType, keyType, valueType, docIndexDeclaration);
                DbManager.AddTypeOperator(DocumentId, indexOperator);
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
            DbManager.AddSuper(DocumentId, namedType.Name, type);
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
                var declaration = new LuaDeclaration(
                    name.RepresentText,
                    param.Position,
                    new GenericParamInfo(
                        new(param),
                        type
                    ));
                DbManager.AddGenericParam(DocumentId, namedType.Name, declaration);
            }
        }
    }

    private void AnalyzeTableExprDeclaration(LuaTableExprSyntax tableExprSyntax)
    {
        var tableClass = tableExprSyntax.UniqueString;
        foreach (var fieldSyntax in tableExprSyntax.FieldList)
        {
            if (fieldSyntax is { Name: { } fieldName, Value: { } value })
            {
                // TODO get type from ---@field ---@type
                var declaration = new LuaDeclaration(
                    fieldName,
                    fieldSyntax.Position,
                    new TableFieldInfo(
                        new(fieldSyntax),
                        null
                    ));
                DbManager.AddMember(DocumentId, tableClass, declaration);
                var unResolveDeclaration =
                    new UnResolvedDeclaration(declaration, new LuaExprRef(value), ResolveState.UnResolvedType);
                AddUnResolved(unResolveDeclaration);
            }
        }
    }

    private void AnalyzeLuaTableType(LuaDocTableTypeSyntax luaDocTableTypeSyntax)
    {
        var tableType = new LuaDocTableType(luaDocTableTypeSyntax);
        if (luaDocTableTypeSyntax.Body is not null)
        {
            AnalyzeDocBody(tableType, luaDocTableTypeSyntax.Body);
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
        DbManager.AddNameExpr(DocumentId, nameExpr);
    }

    private void IndexIndexExpr(LuaIndexExprSyntax indexExpr)
    {
        DbManager.AddIndexExpr(DocumentId, indexExpr);
    }

    private void IndexDocNameType(LuaDocNameTypeSyntax docNameType)
    {
        DbManager.AddNameType(DocumentId, docNameType);
    }

    private void AnalyzeDiagnostic(LuaDocTagDiagnosticSyntax diagnosticSyntax)
    {
        if (diagnosticSyntax is
            {
                Action: { RepresentText: { } actionName },
                Diagnostics: { DiagnosticNames: { } diagnosticNames }
            })
        {
            switch (actionName)
            {
                case "disable-next-line":
                {
                    if (diagnosticSyntax.Parent is LuaCommentSyntax { Owner.Range: { } range })
                    {
                        foreach (var diagnosticName in diagnosticNames)
                        {
                            if (diagnosticName is { RepresentText: { } name })
                            {
                                Compilation.Diagnostics.AddDiagnosticDisableNextLine(DocumentId, range, name);
                            }
                        }
                    }

                    break;
                }
                case "disable":
                {
                    foreach (var diagnosticName in diagnosticNames)
                    {
                        if (diagnosticName is { RepresentText: { } name })
                        {
                            Compilation.Diagnostics.AddDiagnosticDisable(DocumentId, name);
                        }
                    }

                    break;
                }
                case "enable":
                {
                    foreach (var diagnosticName in diagnosticNames)
                    {
                        if (diagnosticName is { RepresentText: { } name })
                        {
                            Compilation.Diagnostics.AddDiagnosticEnable(DocumentId, name);
                        }
                    }

                    break;
                }
            }
        }
    }

    private void AnalyzeModule(LuaDocTagModuleSyntax moduleSyntax)
    {
        if (moduleSyntax.Module is { Value: { } moduleName })
        {
            Compilation.Workspace.ModuleGraph.AddVirtualModule(DocumentId, moduleName);
        }
    }

    private void AnalyzeDeclarationDoc(LuaDeclaration declaration, LuaStatSyntax statSyntax)
    {
        var comment = statSyntax.Comments.FirstOrDefault();
        if (comment?.DocList is { } docList)
        {
            foreach (var tagSyntax in docList)
            {
                switch (tagSyntax)
                {
                    case LuaDocTagDeprecatedSyntax:
                    {
                        declaration.Feature |= DeclarationFeature.Deprecated;
                        break;
                    }
                    case LuaDocTagVisibilitySyntax visibilitySyntax:
                    {
                        declaration.Visibility = GetVisibility(visibilitySyntax.Visibility);
                        break;
                    }
                    case LuaDocTagVersionSyntax versionSyntax:
                    {
                        var requiredVersions = new List<RequiredVersion>();
                        foreach (var version in versionSyntax.Versions)
                        {
                            var action = version.Action;
                            var framework = version.Version?.RepresentText ?? string.Empty;
                            var versionNumber = version.VersionNumber?.Version ?? new VersionNumber(0, 0, 0, 0);
                            requiredVersions.Add(new RequiredVersion(action, framework, versionNumber));
                        }
                        declaration.RequiredVersions = requiredVersions;
                        break;
                    }
                    case LuaDocTagNodiscardSyntax:
                    {
                        declaration.Feature |= DeclarationFeature.NoDiscard;
                        break;
                    }
                }
            }
        }
    }
}
