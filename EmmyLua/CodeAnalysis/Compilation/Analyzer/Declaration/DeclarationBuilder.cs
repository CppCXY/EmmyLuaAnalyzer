using EmmyLua.CodeAnalysis.Compilation.Analyzer.Stub;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Compilation.TypeOperator;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Syntax.Tree;
using EmmyLua.CodeAnalysis.Syntax.Walker;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.Declaration;

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

    private StubIndexImpl StubIndexImpl => Compilation.StubIndexImpl;

    private Dictionary<string, Symbol.Symbol> _typeDeclarations = new();

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
        return FindScope(nameExpr)?.FindNameExpr(nameExpr)?.FirstSymbol;
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

    private int GetPosition(LuaSyntaxElement element) => element.Range.StartOffset;

    private string GetUniqueId(LuaSyntaxElement element, DocumentId documentId)
    {
        return $"{documentId.Guid}:{GetPosition(element)}";
    }

    private Symbol.Symbol CreateDeclaration(
        string name,
        LuaSyntaxElement element,
        SymbolFlag flag,
        ILuaType? luaType
    )
    {
        var declaration = new Symbol.Symbol(
            name,
            GetPosition(element),
            element,
            flag,
            _curScope,
            null,
            luaType);
        _curScope?.Add(declaration);
        return declaration;
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
            _ => Push(new SymbolScope(_tree, position, _curScope), element)
        };
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
            case LuaBlockSyntax blockSyntax:
            {
                BlockReturnAnalysis(blockSyntax);
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
            var luaType = types.ElementAtOrDefault(i) ?? null;

            if (localName is { Name: { } name })
            {
                var declaration = CreateDeclaration(name.RepresentText, localName, SymbolFlag.Local, luaType);
                if (i == 0)
                {
                    var typeDeclaration = FindLocalOrAssignTagDeclaration(localStatSyntax);
                    declaration.PrevSymbol = typeDeclaration;
                }
            }
        }
    }

    private List<Symbol.Symbol> GetParamListDeclaration(LuaParamListSyntax paramListSyntax)
    {
        var declarations = new List<Symbol.Symbol>();
        var dic = FindParamDeclarations(paramListSyntax);
        foreach (var param in paramListSyntax.Params)
        {
            if (param.Name is { } name)
            {
                var declaration = CreateDeclaration(name.RepresentText, param,
                    SymbolFlag.Local | SymbolFlag.Parameter, null);
                if (dic.TryGetValue(name.RepresentText, out var prevDeclaration))
                {
                    declaration.PrevSymbol = prevDeclaration;
                }

                declarations.Add(declaration);
            }
            else if (param.IsVarArgs)
            {
                var declaration = CreateDeclaration("...", param,
                    SymbolFlag.Local | SymbolFlag.Parameter, null);
                if (dic.TryGetValue("...", out var prevDeclaration))
                {
                    declaration.PrevSymbol = prevDeclaration;
                }

                declarations.Add(declaration);
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
                var declaration = CreateDeclaration(name.RepresentText, param,
                    SymbolFlag.Local, null);
                if (dic.TryGetValue(name.RepresentText, out var prevDeclaration))
                {
                    declaration.PrevSymbol = prevDeclaration;
                }
            }
        }
    }

    private void ForStatDeclarationAnalysis(LuaForStatSyntax forStatSyntax)
    {
        if (forStatSyntax is { IteratorName.Name: { } name, InitExpr: { } initExpr })
        {
            CreateDeclaration(name.RepresentText, name, SymbolFlag.Local, Compilation.Builtin.Integer);
        }
    }

    private Symbol.Symbol? FindLocalOrAssignTagDeclaration(LuaStatSyntax stat)
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

                var declaration = CreateDeclaration(name.RepresentText, name,
                    SymbolFlag.Parameter, ty);
                dic.Add(name.RepresentText, declaration);
            }
            else if (tagParamSyntax is { VarArgs: { } varArgs, Type: { } type2 })
            {
                var declaration = CreateDeclaration(varArgs.RepresentText, varArgs,
                    SymbolFlag.Parameter, new LuaTypeRef(type2));
                dic.Add("...", declaration);
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

            switch (varExpr)
            {
                case LuaNameExprSyntax nameExpr:
                {
                    if (nameExpr.Name is { } name)
                    {
                        var prevDeclaration = FindDeclaration(nameExpr);
                        var flags = prevDeclaration?.Flags ?? SymbolFlag.Global;
                        var declaration = CreateDeclaration(name.RepresentText, nameExpr, flags, luaType);
                        if (prevDeclaration is not null)
                        {
                            declaration.PrevSymbol = prevDeclaration;
                        }
                        else
                        {
                            if (i == 0)
                            {
                                var typeDeclaration = FindLocalOrAssignTagDeclaration(luaAssignStat);
                                declaration.PrevSymbol = typeDeclaration;
                            }

                            StubIndexImpl.GlobalDeclaration.AddStub(DocumentId, name.RepresentText,
                                declaration);
                        }
                    }

                    break;
                }
                case LuaIndexExprSyntax indexExpr:
                {
                    if (i == 0)
                    {
                        var typeDeclaration = FindLocalOrAssignTagDeclaration(luaAssignStat);
                        Analyzer.DelayAnalyzeNodes.Add(new DelayAnalyzeNode(indexExpr, DocumentId, luaType,
                            typeDeclaration, _curScope, lastValidExpr, retId));
                    }
                    else
                    {
                        Analyzer.DelayAnalyzeNodes.Add(new DelayAnalyzeNode(indexExpr, DocumentId, luaType, null,
                            _curScope, lastValidExpr, retId));
                    }

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
                CreateDeclaration(name.RepresentText, name,
                    SymbolFlag.Method | SymbolFlag.Local, GetMethodDeclaration(luaFuncStat.FuncBody, false));
                break;
            }
            case { IsLocal: false, NameExpr.Name: { } name2 }:
            {
                var prevDeclaration = FindDeclaration(luaFuncStat.NameExpr);
                var flags = prevDeclaration?.Flags ?? (SymbolFlag.Global | SymbolFlag.Method);
                var declaration = CreateDeclaration(name2.RepresentText, name2, flags,
                    GetMethodDeclaration(luaFuncStat.FuncBody, false));
                if (prevDeclaration is not null)
                {
                    declaration.PrevSymbol = prevDeclaration;
                }
                else
                {
                    StubIndexImpl.GlobalDeclaration.AddStub(DocumentId, name2.RepresentText, declaration);
                }

                break;
            }
            case { IsMethod: true, IndexExpr: { } indexExpr }:
            {
                Analyzer.DelayAnalyzeNodes.Add(new DelayAnalyzeNode(indexExpr, DocumentId,
                    GetMethodDeclaration(luaFuncStat.FuncBody, indexExpr.IsColonIndex), null, _curScope, null));
                break;
            }
        }
    }

    private void ClosureExprDeclarationAnalysis(LuaClosureExprSyntax closureExprSyntax)
    {
        var funcBody = closureExprSyntax.FuncBody;
        var luaMethod = GetMethodDeclaration(funcBody, false);
        if (luaMethod is not null)
        {
            StubIndexImpl.Methods.AddStub(DocumentId, closureExprSyntax, luaMethod);
        }
    }

    private void ClassTagDeclarationAnalysis(LuaDocTagClassSyntax tagClassSyntax)
    {
        if (tagClassSyntax is { Name: { } name })
        {
            var luaClass = new LuaClass(name.RepresentText);
            var declaration = CreateDeclaration(name.RepresentText, name, SymbolFlag.TypeDeclaration, luaClass);
            _typeDeclarations.Add(name.RepresentText, declaration);
            StubIndexImpl.GlobalDeclaration.AddStub(DocumentId, name.RepresentText, declaration);
            StubIndexImpl.NamedTypeIndex.AddStub(DocumentId, name.RepresentText, luaClass);
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
            var declaration = CreateDeclaration(name.RepresentText, name, SymbolFlag.TypeDeclaration, luaAlias);
            _typeDeclarations.Add(name.RepresentText, declaration);
            StubIndexImpl.GlobalDeclaration.AddStub(DocumentId, name.RepresentText, declaration);
            StubIndexImpl.NamedTypeIndex.AddStub(DocumentId, name.RepresentText, luaAlias);
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
            var declaration = CreateDeclaration(name.RepresentText, name, SymbolFlag.TypeDeclaration, luaEnum);
            _typeDeclarations.Add(name.RepresentText, declaration);
            StubIndexImpl.GlobalDeclaration.AddStub(DocumentId, name.RepresentText, declaration);
            StubIndexImpl.NamedTypeIndex.AddStub(DocumentId, name.RepresentText, luaEnum);
            foreach (var field in tagEnumSyntax.FieldList)
            {
                if (field is { Name: { } fieldName })
                {
                    var fieldDeclaration = CreateDeclaration(fieldName.RepresentText, fieldName,
                        SymbolFlag.EnumMember, baseType);
                    StubIndexImpl.Members.AddStub(DocumentId, name.RepresentText,
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
                CreateDeclaration(name.RepresentText, name, SymbolFlag.TypeDeclaration, luaInterface);
            _typeDeclarations.Add(name.RepresentText, declaration);
            StubIndexImpl.GlobalDeclaration.AddStub(DocumentId, name.RepresentText, declaration);
            StubIndexImpl.NamedTypeIndex.AddStub(DocumentId, name.RepresentText, luaInterface);
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
                    var declaration = CreateDeclaration(nameField.RepresentText, nameField,
                        SymbolFlag.ClassMember | SymbolFlag.DocField, new LuaTypeRef(type1));
                    StubIndexImpl.Members.AddStub(DocumentId, namedType.Name, declaration);
                    break;
                }
                case { IntegerField: { } integerField, Type: { } type2 }:
                {
                    var declaration = CreateDeclaration($"[{integerField.Value}]", integerField,
                        SymbolFlag.ClassMember | SymbolFlag.DocField, new LuaTypeRef(type2));
                    StubIndexImpl.Members.AddStub(DocumentId, namedType.Name, declaration);
                    break;
                }
                case { StringField: { } stringField, Type: { } type3 }:
                {
                    var declaration = CreateDeclaration(stringField.Value, stringField,
                        SymbolFlag.ClassMember | SymbolFlag.DocField, new LuaTypeRef(type3));
                    StubIndexImpl.Members.AddStub(DocumentId, namedType.Name, declaration);
                    break;
                }
                case { TypeField: { } typeField, Type: { } type4 }:
                {
                    var indexOperator = new IndexOperator(new LuaTypeRef(typeField), new LuaTypeRef(type4));
                    StubIndexImpl.TypeOperators.AddStub(DocumentId, namedType.Name, indexOperator);
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
                        SymbolFlag.ClassMember | SymbolFlag.DocField, new LuaTypeRef(type1));
                    StubIndexImpl.Members.AddStub(DocumentId, namedType.Name, declaration);
                    break;
                }
                case { IntegerField: { } integerField, Type: { } type2 }:
                {
                    var declaration = CreateDeclaration($"[{integerField.Value}]", integerField,
                        SymbolFlag.ClassMember | SymbolFlag.DocField, new LuaTypeRef(type2));
                    StubIndexImpl.Members.AddStub(DocumentId, namedType.Name, declaration);
                    break;
                }
                case { StringField: { } stringField, Type: { } type3 }:
                {
                    var declaration = CreateDeclaration(stringField.Value, stringField,
                        SymbolFlag.ClassMember | SymbolFlag.DocField, new LuaTypeRef(type3));
                    StubIndexImpl.Members.AddStub(DocumentId, namedType.Name, declaration);
                    break;
                }
                case { TypeField: { } typeField, Type: { } type4 }:
                {
                    var indexOperator = new IndexOperator(new LuaTypeRef(typeField), new LuaTypeRef(type4));
                    StubIndexImpl.TypeOperators.AddStub(DocumentId, namedType.Name, indexOperator);
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
            StubIndexImpl.Supers.AddStub(DocumentId, namedType.Name, type);
        }
    }

    private void ClassOrInterfaceGenericParamAnalysis(LuaDocTagGenericDeclareListSyntax genericDeclareList,
        ILuaNamedType namedType)
    {
        foreach (var param in genericDeclareList.Params)
        {
            if (param is { Name: { } name })
            {
                var declaration = CreateDeclaration(name.RepresentText, name,
                    SymbolFlag.GenericParameter, param.Type != null ? new LuaTypeRef(param.Type) : null);
                StubIndexImpl.GenericParams.AddStub(DocumentId, namedType.Name, declaration);
            }
        }
    }

    private void TableFieldDeclarationAnalysis(LuaTableFieldSyntax tableFieldSyntax)
    {
        if (tableFieldSyntax is { Name: { } fieldName, ParentTable: { } table })
        {
            var parentId = GetUniqueId(table, DocumentId);

            var declaration = CreateDeclaration(fieldName, tableFieldSyntax,
                SymbolFlag.ClassMember, null);
            StubIndexImpl.Members.AddStub(DocumentId, parentId, declaration);
        }
    }

    private LuaMethod? GetMethodDeclaration(LuaFuncBodySyntax? funcBody, bool colon)
    {
        var stat = funcBody?.AncestorsAndSelf.OfType<LuaStatSyntax>().FirstOrDefault();
        if (stat is null)
        {
            return null;
        }

        var comment = stat.Comments.FirstOrDefault();

        ILuaType? retType = null;
        List<Signature>? overloads = null;
        List<Symbol.Symbol>? genericParams = null;
        if (comment?.DocList is { } docList)
        {
            var list = docList.ToList();
            retType = GetRetType(list);
            overloads = list
                .OfType<LuaDocTagOverloadSyntax>()
                .Select(Compilation.SearchContext.Infer)
                .OfType<LuaMethod>()
                .Select(it => it.MainSignature)
                .ToList();

            var generic = list.OfType<LuaDocTagGenericDeclareListSyntax>().FirstOrDefault();
            if (generic is not null)
            {
                genericParams = generic.Params
                    .Select(it =>
                        new VirtualSymbol(it.Name?.RepresentText ?? string.Empty,
                            it.Type is not null ? new LuaTypeRef(it.Type) : null)
                    )
                    .Cast<Symbol.Symbol>()
                    .ToList();
            }
        }

        var parameters = new List<Symbol.Symbol>();
        if (funcBody?.ParamList is { } paramList)
        {
            parameters = GetParamListDeclaration(paramList);
        }

        return new LuaMethod(new Signature(colon, parameters, retType), overloads, genericParams);
    }

    private void LuaTableTypeAnalysis(LuaDocTableTypeSyntax luaDocTableTypeSyntax)
    {
        var className = GetUniqueId(luaDocTableTypeSyntax, DocumentId);
        foreach (var fieldSyntax in luaDocTableTypeSyntax.FieldList)
        {
            if (fieldSyntax is { NameField: { } nameToken, Type: { } type })
            {
                var declaration = CreateDeclaration(nameToken.RepresentText, nameToken,
                    SymbolFlag.ClassMember | SymbolFlag.DocField, new LuaTypeRef(type));
                StubIndexImpl.Members.AddStub(DocumentId, className, declaration);
            }
            else if (fieldSyntax is { IntegerField: { } integerField, Type: { } type2 })
            {
                var declaration = CreateDeclaration($"[{integerField.Value}]", integerField,
                    SymbolFlag.ClassMember | SymbolFlag.DocField, new LuaTypeRef(type2));
                StubIndexImpl.Members.AddStub(DocumentId, className, declaration);
            }
            else if (fieldSyntax is { StringField: { } stringField, Type: { } type3 })
            {
                var declaration = CreateDeclaration(stringField.Value, stringField,
                    SymbolFlag.ClassMember | SymbolFlag.DocField, new LuaTypeRef(type3));
                StubIndexImpl.Members.AddStub(DocumentId, className, declaration);
            }
            else if (fieldSyntax is { TypeField: { } typeField, Type: { } type4 })
            {
                var indexOperator = new IndexOperator(new LuaTypeRef(typeField), new LuaTypeRef(type4));
                StubIndexImpl.TypeOperators.AddStub(DocumentId, className, indexOperator);
            }
        }

        StubIndexImpl.NamedTypeIndex.AddStub(DocumentId, className, new LuaClass(className));
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
                        StubIndexImpl.BlockReturns.AddStub(DocumentId, block, returnStatSyntax.ExprList.ToList());
                        break;
                    }
                }
            }
        }
    }
}
