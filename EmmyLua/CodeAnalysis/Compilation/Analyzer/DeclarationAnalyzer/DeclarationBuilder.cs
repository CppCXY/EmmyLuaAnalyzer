using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Compilation.TypeOperator;
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

    private Stack<SymbolScope> _scopes = new();

    private Dictionary<LuaSyntaxElement, SymbolScope> _scopeOwners = new();

    private SymbolTree _tree;

    private LuaSyntaxTree _syntaxTree;

    private DeclarationAnalyzer Analyzer { get; }

    private LuaCompilation Compilation => Analyzer.Compilation;

    private Compilation.Stub.Stub Stub => Compilation.Stub;

    private Dictionary<string, Declaration> _typeDeclarations = new();

    private DocumentId DocumentId { get; }

    public SymbolTree Build()
    {
        _syntaxTree.SyntaxRoot.Accept(this);
        _tree.RootScope = _topScope;
        return _tree;
    }

    public DeclarationBuilder(DocumentId documentId, LuaSyntaxTree tree, DeclarationAnalyzer analyzer)
    {
        _syntaxTree = tree;
        _tree = new SymbolTree(tree, _scopeOwners);
        Analyzer = analyzer;
        DocumentId = documentId;
    }

    private Symbol.Symbol? FindDeclaration(LuaNameExprSyntax nameExpr)
    {
        return FindScope(nameExpr)?.FindNameDeclaration(nameExpr)?.FirstSymbol;
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
        return $"{DocumentId.Guid}:{GetPosition(element)}";
    }

    private void AddSymbol(Symbol.Symbol symbol)
    {
        _curScope?.Add(symbol);
    }

    private SymbolScope Push(LuaSyntaxElement element)
    {
        var position = GetPosition(element);
        return element switch
        {
            LuaLocalStatSyntax => Push(new LocalStatSymbolScope(_tree, position, _curScope),
                element),
            LuaRepeatStatSyntax => Push(new RepeatStatSymbolScope(_tree, position, _curScope),
                element),
            LuaForRangeStatSyntax => Push(new ForRangeStatSymbolScope(_tree, position, _curScope), element),
            LuaFuncStatSyntax funcStat => PushMethod(position, funcStat),
            _ => Push(new SymbolScope(_tree, position, _curScope), element)
        };
    }

    private SymbolScope PushMethod(int position, LuaFuncStatSyntax funcStat)
    {
        ParameterDeclaration? self = null;
        if (funcStat.IndexExpr is { PrefixExpr: { } prefixExpr })
        {
            self = ParameterDeclaration.SelfParameter(new LuaExprRef(prefixExpr));
        }

        return Push(new MethodStatSymbolScope(_tree, position, _curScope, self), funcStat);
    }

    private SymbolScope Push(SymbolScope scope, LuaSyntaxElement element)
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
            case LuaClosureExprSyntax closureExprSyntax:
            {
                ClosureExprDeclarationAnalysis(closureExprSyntax);
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
            case LuaTableFieldSyntax tableFieldSyntax:
            {
                TableFieldDeclarationAnalysis(tableFieldSyntax);
                break;
            }
            case LuaDocTableTypeSyntax tableTypeSyntax:
            {
                LuaTableTypeAnalysis(tableTypeSyntax);
                break;
            }
            case LuaSourceSyntax sourceSyntax:
            {
                if (sourceSyntax.Block is not null)
                {
                    BlockReturnAnalysis(sourceSyntax.Block);
                }

                break;
            }
            case LuaLabelStatSyntax labelStatSyntax:
            {
                LuaLabelAnalysis(labelStatSyntax);
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
            or LuaForStatSyntax or LuaFuncStatSyntax;

    private void LocalStatDeclarationAnalysis(LuaLocalStatSyntax localStatSyntax)
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
                var symbol = new LocalDeclaration(
                    name.RepresentText, GetPosition(localName), localName, luaType, relatedExpr);
                AddSymbol(symbol);
                if (i == 0)
                {
                    var typeDeclaration = FindLocalOrAssignTypeDeclaration(localStatSyntax);
                    symbol.PrevSymbol = typeDeclaration;
                }
            }
        }
    }

    private List<ParameterDeclaration> GetParamListDeclaration(LuaParamListSyntax paramListSyntax)
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
                    declaration.PrevSymbol = prevDeclaration;
                }

                declarations.Add(declaration);
                AddSymbol(declaration);
            }
            else if (param.IsVarArgs)
            {
                var declaration = new ParameterDeclaration("...", GetPosition(param), param, null);
                if (dic.TryGetValue("...", out var prevDeclaration))
                {
                    declaration.PrevSymbol = prevDeclaration;
                }

                declarations.Add(declaration);
                AddSymbol(declaration);
            }
        }

        return declarations;
    }

    private ILuaType? GetRetType(IEnumerable<LuaDocTagSyntax> docList)
    {
        var retTag = docList.OfType<LuaDocTagReturnSyntax>().ToList();
        switch (retTag.Count)
        {
            case 0:
                return null;
            case 1:
            {
                var typeList = retTag[0].TypeList.ToList();
                switch (typeList.Count)
                {
                    case 0:
                        return null;
                    case 1:
                        return new LuaTypeRef(typeList[0]);
                    default:
                    {
                        var rets = typeList.Select(type => new LuaTypeRef(type)).Cast<ILuaType>().ToList();
                        return new LuaMultiRetType(rets);
                    }
                }
            }
            default:
            {
                var rets = retTag.SelectMany(tag => tag.TypeList).Select(type => new LuaTypeRef(type)).Cast<ILuaType>()
                    .ToList();
                return new LuaMultiRetType(rets);
            }
        }
    }

    private void ForRangeStatDeclarationAnalysis(LuaForRangeStatSyntax forRangeStatSyntax)
    {
        var dic = FindParamDeclarations(forRangeStatSyntax);
        foreach (var param in forRangeStatSyntax.IteratorNames)
        {
            if (param.Name is { } name)
            {
                var declaration = new ParameterDeclaration(name.RepresentText, GetPosition(param), param, null);
                if (dic.TryGetValue(name.RepresentText, out var prevDeclaration))
                {
                    declaration.PrevSymbol = prevDeclaration;
                }

                AddSymbol(declaration);
            }
        }
    }

    private void ForStatDeclarationAnalysis(LuaForStatSyntax forStatSyntax)
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

    private List<ILuaType> FindLocalOrAssignTypes(LuaStatSyntax stat) =>
    (
        from comment in stat.Comments
        from tagType in comment.DocList.OfType<LuaDocTagTypeSyntax>()
        from type in tagType.TypeList
        select new LuaTypeRef(type)
    ).Cast<ILuaType>().ToList();

    private Dictionary<string, Symbol.Symbol> FindParamDeclarations(LuaSyntaxElement element)
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

        var dic = new Dictionary<string, Symbol.Symbol>();

        foreach (var tagParamSyntax in docList.OfType<LuaDocTagParamSyntax>())
        {
            if (tagParamSyntax is { Name: { } name, Type: { } type, Nullable: { } nullable })
            {
                ILuaType ty = new LuaTypeRef(type);
                if (nullable)
                {
                    ty = LuaUnion.UnionType(ty, Compilation.Builtin.Nil);
                }

                var declaration = new DocParameterDeclaration(name.RepresentText, GetPosition(name), name, ty);
                dic.Add(name.RepresentText, declaration);
                AddSymbol(declaration);
            }
            else if (tagParamSyntax is { VarArgs: { } varArgs, Type: { } type2 })
            {
                var declaration = new DocParameterDeclaration(varArgs.RepresentText, GetPosition(varArgs), varArgs,
                    new LuaTypeRef(type2));
                dic.Add("...", declaration);
                AddSymbol(declaration);
            }
        }

        return dic;
    }

    private void AssignStatDeclarationAnalysis(LuaAssignStatSyntax luaAssignStat)
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
                        Symbol.Symbol declarationOrSymbol = null!;
                        if (prevDeclaration is not null)
                        {
                            AddSymbol(new AssignSymbol(name.RepresentText, GetPosition(nameExpr), prevDeclaration));
                        }
                        else
                        {
                            var declaration = new GlobalDeclaration(name.RepresentText, GetPosition(nameExpr), nameExpr,
                                luaType, relatedExpr);
                            Stub.GlobalDeclaration.AddStub(DocumentId, name.RepresentText, declaration);

                            if (i == 0)
                            {
                                var typeDeclaration = FindLocalOrAssignTypeDeclaration(luaAssignStat);
                                declarationOrSymbol.PrevSymbol = typeDeclaration;
                            }

                            AddSymbol(declaration);
                        }
                    }

                    break;
                }
                case LuaIndexExprSyntax indexExpr:
                {
                    var declaration = new IndexDeclaration(indexExpr.Name, GetPosition(indexExpr), indexExpr, luaType,
                        relatedExpr);
                    if (i == 0)
                    {
                        var typeDeclaration = FindLocalOrAssignTypeDeclaration(luaAssignStat);
                        declaration.PrevSymbol = typeDeclaration;
                    }

                    AddSymbol(declaration);
                    break;
                }
            }
        }
    }

    private void MethodDeclarationAnalysis(LuaFuncStatSyntax luaFuncStat)
    {
        switch (luaFuncStat)
        {
            case { IsLocal: true, LocalName.Name: { } name }:
            {
                var luaMethod = FuncBodyMethodType(luaFuncStat.FuncBody, false);
                if (luaMethod is not null)
                {
                    var methodDeclaration =
                        new MethodDeclaration(name.RepresentText, GetPosition(luaFuncStat.LocalName),
                            luaFuncStat.LocalName, luaMethod, luaFuncStat.FuncBody!);
                    AddSymbol(methodDeclaration);
                }

                break;
            }
            case { IsLocal: false, NameExpr.Name: { } name2 }:
            {
                var luaMethod = FuncBodyMethodType(luaFuncStat.FuncBody, false);
                if (luaMethod is not null)
                {
                    var prevDeclaration = FindDeclaration(luaFuncStat.NameExpr);
                    if (prevDeclaration is not null)
                    {
                        AddSymbol(new AssignSymbol(name2.RepresentText, GetPosition(luaFuncStat.NameExpr), prevDeclaration));
                    }
                    else
                    {
                        var declaration = new MethodDeclaration(name2.RepresentText,
                            GetPosition(luaFuncStat.NameExpr),
                            luaFuncStat.NameExpr, luaMethod, luaFuncStat.FuncBody!)
                        {
                            Feature = SymbolFeature.Global
                        };
                        Stub.GlobalDeclaration.AddStub(DocumentId, name2.RepresentText, declaration);
                        AddSymbol(declaration);
                    }
                }

                break;
            }
            case { IsMethod: true, IndexExpr: { } indexExpr }:
            {
                var luaMethod = FuncBodyMethodType(luaFuncStat.FuncBody, indexExpr.IsColonIndex);
                if (luaMethod is not null && indexExpr is { Name: { } name })
                {
                    var declaration =
                        new MethodDeclaration(name, GetPosition(indexExpr), indexExpr, luaMethod,
                            luaFuncStat.FuncBody!);
                    AddSymbol(declaration);
                }

                break;
            }
        }
    }

    private void ClosureExprDeclarationAnalysis(LuaClosureExprSyntax closureExprSyntax)
    {
        var funcBody = closureExprSyntax.FuncBody;
        FuncBodyMethodType(funcBody, false);
    }

    private void ClassTagDeclarationAnalysis(LuaDocTagClassSyntax tagClassSyntax)
    {
        if (tagClassSyntax is { Name: { } name })
        {
            var luaClass = new LuaClass(name.RepresentText);
            var declaration = new ClassDeclaration(name.RepresentText, GetPosition(name), tagClassSyntax, luaClass);
            AddSymbol(declaration);
            _typeDeclarations.Add(name.RepresentText, declaration);
            Stub.NamedTypeIndex.AddStub(DocumentId, name.RepresentText, luaClass);
            ClassOrInterfaceFieldsTagAnalysis(luaClass, tagClassSyntax);
            if (tagClassSyntax is { Body: { } body })
            {
                ClassOrInterfaceBodyAnalysis(luaClass, body);
            }

            if (tagClassSyntax is { ExtendTypeList: { } extendTypeList })
            {
                ClassOrInterfaceSupersAnalysis(extendTypeList, luaClass);
            }

            if (tagClassSyntax is { GenericDeclareList: { } genericDeclareList })
            {
                ClassOrInterfaceGenericParamAnalysis(genericDeclareList, luaClass);
            }
        }
    }

    private void AliasTagDeclarationAnalysis(LuaDocTagAliasSyntax tagAliasSyntax)
    {
        if (tagAliasSyntax is { Name: { } name, Type: { } type })
        {
            var luaAlias = new LuaAlias(name.RepresentText, new LuaTypeRef(type));
            var declaration = new AliasDeclaration(name.RepresentText, GetPosition(name), tagAliasSyntax, luaAlias);
            AddSymbol(declaration);
            _typeDeclarations.Add(name.RepresentText, declaration);
            Stub.NamedTypeIndex.AddStub(DocumentId, name.RepresentText, luaAlias);
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
            var declaration = new EnumDeclaration(name.RepresentText, GetPosition(name), tagEnumSyntax, luaEnum);
            AddSymbol(declaration);
            _typeDeclarations.Add(name.RepresentText, declaration);
            Stub.NamedTypeIndex.AddStub(DocumentId, name.RepresentText, luaEnum);
            foreach (var field in tagEnumSyntax.FieldList)
            {
                if (field is { Name: { } fieldName })
                {
                    var fieldDeclaration = new EnumFieldDeclaration(fieldName.RepresentText, GetPosition(fieldName),
                        field, baseType);
                    Stub.Members.AddStub(DocumentId, name.RepresentText, fieldDeclaration);
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
                new InterfaceDeclaration(name.RepresentText, GetPosition(name), tagInterfaceSyntax, luaInterface);
            AddSymbol(declaration);
            _typeDeclarations.Add(name.RepresentText, declaration);
            Stub.NamedTypeIndex.AddStub(DocumentId, name.RepresentText, luaInterface);
            ClassOrInterfaceFieldsTagAnalysis(luaInterface, tagInterfaceSyntax);
            if (tagInterfaceSyntax is { Body: { } body })
            {
                ClassOrInterfaceBodyAnalysis(luaInterface, body);
            }

            if (tagInterfaceSyntax is { ExtendTypeList: { } extendTypeList })
            {
                ClassOrInterfaceSupersAnalysis(extendTypeList, luaInterface);
            }

            if (tagInterfaceSyntax is { GenericDeclareList: { } genericDeclareList })
            {
                ClassOrInterfaceGenericParamAnalysis(genericDeclareList, luaInterface);
            }
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
                    var declaration = new DocFieldDeclaration(nameField.RepresentText, GetPosition(nameField),
                        tagField, new LuaTypeRef(type1));
                    AddSymbol(declaration);
                    Stub.Members.AddStub(DocumentId, namedType.Name, declaration);
                    break;
                }
                case { IntegerField: { } integerField, Type: { } type2 }:
                {
                    var declaration = new DocFieldDeclaration($"[{integerField.Value}]", GetPosition(integerField),
                        tagField, new LuaTypeRef(type2));
                    AddSymbol(declaration);
                    Stub.Members.AddStub(DocumentId, namedType.Name, declaration);
                    break;
                }
                case { StringField: { } stringField, Type: { } type3 }:
                {
                    var declaration = new DocFieldDeclaration(stringField.Value, GetPosition(stringField),
                        tagField, new LuaTypeRef(type3));
                    AddSymbol(declaration);
                    Stub.Members.AddStub(DocumentId, namedType.Name, declaration);
                    break;
                }
                case { TypeField: { } typeField, Type: { } type4 }:
                {
                    var indexOperator = new IndexOperator(new LuaTypeRef(typeField), new LuaTypeRef(type4));
                    Stub.TypeOperators.AddStub(DocumentId, namedType.Name, indexOperator);
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
                    var declaration = new DocFieldDeclaration(nameField.RepresentText, GetPosition(nameField),
                        field, new LuaTypeRef(type1));
                    AddSymbol(declaration);
                    Stub.Members.AddStub(DocumentId, namedType.Name, declaration);
                    break;
                }
                case { IntegerField: { } integerField, Type: { } type2 }:
                {
                    var declaration = new DocFieldDeclaration($"[{integerField.Value}]", GetPosition(integerField),
                        field, new LuaTypeRef(type2));
                    AddSymbol(declaration);
                    Stub.Members.AddStub(DocumentId, namedType.Name, declaration);
                    break;
                }
                case { StringField: { } stringField, Type: { } type3 }:
                {
                    var declaration = new DocFieldDeclaration(stringField.Value, GetPosition(stringField),
                        field, new LuaTypeRef(type3));
                    AddSymbol(declaration);
                    Stub.Members.AddStub(DocumentId, namedType.Name, declaration);
                    break;
                }
                case { TypeField: { } typeField, Type: { } type4 }:
                {
                    var indexOperator = new IndexOperator(new LuaTypeRef(typeField), new LuaTypeRef(type4));
                    Stub.TypeOperators.AddStub(DocumentId, namedType.Name, indexOperator);
                    break;
                }
            }
        }
    }

    private void ClassOrInterfaceSupersAnalysis(IEnumerable<LuaDocTypeSyntax> extendList, ILuaNamedType namedType)
    {
        foreach (var extend in extendList)
        {
            var type = new LuaTypeRef(extend);
            Stub.Supers.AddStub(DocumentId, namedType.Name, type);
        }
    }

    private void ClassOrInterfaceGenericParamAnalysis(LuaDocTagGenericDeclareListSyntax genericDeclareList,
        ILuaNamedType namedType)
    {
        foreach (var param in genericDeclareList.Params)
        {
            if (param is { Name: { } name })
            {
                var declaration = new GenericParameterDeclaration(name.RepresentText, GetPosition(name), param,
                    param.Type != null ? new LuaTypeRef(param.Type) : null);
                Stub.NamedTypeGenericParams.AddStub(DocumentId, namedType.Name, declaration);
            }
        }
    }

    private void TableFieldDeclarationAnalysis(LuaTableFieldSyntax tableFieldSyntax)
    {
        if (tableFieldSyntax is { Name: { } fieldName, ParentTable: { } table, Value: { } value })
        {
            var parentId = GetUniqueId(table);
            // TODO get type from ---@field ---@type
            var declaration =
                new TableFieldDeclaration(fieldName, GetPosition(tableFieldSyntax), tableFieldSyntax, null);
            AddSymbol(declaration);
            Stub.Members.AddStub(DocumentId, parentId, declaration);
        }
    }

    private LuaMethod? FuncBodyMethodType(LuaFuncBodySyntax? funcBody, bool colon)
    {
        var stat = funcBody?.AncestorsAndSelf.OfType<LuaStatSyntax>().FirstOrDefault();
        if (stat is null)
        {
            return null;
        }

        var comment = stat.Comments.FirstOrDefault();

        ILuaType? retType = null;
        List<Signature>? overloads = null;
        List<GenericParameterDeclaration>? genericParams = null;
        if (comment?.DocList is { } docList)
        {
            var list = docList.ToList();
            retType = GetRetType(list);
            overloads = list
                .OfType<LuaDocTagOverloadSyntax>()
                .Select(it => it.TypeFunc)
                .Where(it => it is not null)
                .Select(it => TypeInfer.InferFuncType(it!, Compilation.SearchContext))
                .OfType<LuaMethod>()
                .Select(it => it.MainSignature)
                .ToList();

            var generic = list.OfType<LuaDocTagGenericDeclareListSyntax>().FirstOrDefault();
            if (generic is not null)
            {
                genericParams = generic.Params
                    .Select(it =>
                        new GenericParameterDeclaration(
                            it.Name?.RepresentText ?? string.Empty,
                            GetPosition(it),
                            it,
                            it.Type is not null ? new LuaTypeRef(it.Type) : null)
                    )
                    .ToList();
            }
        }

        var parameters = new List<ParameterDeclaration>();
        if (funcBody?.ParamList is { } paramList)
        {
            parameters = GetParamListDeclaration(paramList);
        }

        if (funcBody?.Block is not null)
        {
            BlockReturnAnalysis(funcBody.Block);
        }

        var method = new LuaMethod(new Signature(colon, parameters, retType), overloads, genericParams);

        if (funcBody is not null)
        {
            Stub.Methods.AddStub(DocumentId, funcBody, method);
        }

        return method;
    }

    private void LuaTableTypeAnalysis(LuaDocTableTypeSyntax luaDocTableTypeSyntax)
    {
        var className = GetUniqueId(luaDocTableTypeSyntax);
        foreach (var fieldSyntax in luaDocTableTypeSyntax.FieldList)
        {
            if (fieldSyntax is { NameField: { } nameToken, Type: { } type })
            {
                var declaration = new DocFieldDeclaration(nameToken.RepresentText, GetPosition(fieldSyntax),
                    fieldSyntax, new LuaTypeRef(type));
                AddSymbol(declaration);
                Stub.Members.AddStub(DocumentId, className, declaration);
            }
            else if (fieldSyntax is { IntegerField: { } integerField, Type: { } type2 })
            {
                var declaration = new DocFieldDeclaration($"[{integerField.Value}]", GetPosition(fieldSyntax),
                    fieldSyntax, new LuaTypeRef(type2));
                AddSymbol(declaration);
                Stub.Members.AddStub(DocumentId, className, declaration);
            }
            else if (fieldSyntax is { StringField: { } stringField, Type: { } type3 })
            {
                var declaration = new DocFieldDeclaration(stringField.Value, GetPosition(fieldSyntax), fieldSyntax,
                    new LuaTypeRef(type3));
                AddSymbol(declaration);
                Stub.Members.AddStub(DocumentId, className, declaration);
            }
            else if (fieldSyntax is { TypeField: { } typeField, Type: { } type4 })
            {
                var indexOperator = new IndexOperator(new LuaTypeRef(typeField), new LuaTypeRef(type4));
                Stub.TypeOperators.AddStub(DocumentId, className, indexOperator);
            }
        }

        Stub.NamedTypeIndex.AddStub(DocumentId, className, new LuaClass(className));
    }

    private void BlockReturnAnalysis(LuaBlockSyntax mainBlock)
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
                        Stub.MainBlockReturns.AddStub(DocumentId, block, returnStatSyntax.ExprList.ToList());
                        break;
                    }
                }
            }
        }
    }

    private void LuaLabelAnalysis(LuaLabelStatSyntax labelStatSyntax)
    {
        if (labelStatSyntax is { Name: { } name })
        {
            var labelDeclaration =
                new LabelDeclaration(name.RepresentText, GetPosition(labelStatSyntax), labelStatSyntax);
            AddSymbol(labelDeclaration);
        }
    }
}
