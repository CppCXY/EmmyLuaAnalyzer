using EmmyLua.CodeAnalysis.Common;
using EmmyLua.CodeAnalysis.Compilation.Analyzer.ResolveAnalyzer;
using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Reference;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.DeclarationAnalyzer.DeclarationWalker;

public partial class DeclarationWalker
{
    private LuaType GetRetType(IEnumerable<LuaDocTagSyntax>? docList)
    {
        var returnTypes = docList?
            .OfType<LuaDocTagReturnSyntax>()
            .SelectMany(tag => tag.TypeList)
            .Select(searchContext.Infer)
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


    private void AnalyzeMethodDeclaration(LuaFuncStatSyntax luaFuncStat)
    {
        switch (luaFuncStat)
        {
            case { IsLocal: true, LocalName.Name: { } name, ClosureExpr: { } closureExpr }:
            {
                var declaration = new LuaDeclaration(
                    name.RepresentText,
                    new MethodInfo(
                        new(luaFuncStat.LocalName),
                        null,
                        new(luaFuncStat)
                    ),
                    DeclarationFeature.Local
                );
                declarationContext.AddReference(ReferenceKind.Definition, declaration, luaFuncStat.LocalName);
                AnalyzeDeclarationDoc(declaration, luaFuncStat);
                declarationContext.AddDeclaration(luaFuncStat.LocalName.Position, declaration);
                var unResolved = new UnResolvedDeclaration(declaration, new LuaExprRef(closureExpr),
                    ResolveState.UnResolvedType);
                declarationContext.AddUnResolved(unResolved);
                break;
            }
            case { IsLocal: false, NameExpr.Name: { } name2, ClosureExpr: { } closureExpr }:
            {
                var prevDeclaration = declarationContext.FindLocalDeclaration(luaFuncStat.NameExpr);
                if (prevDeclaration is null)
                {
                    var declaration = new LuaDeclaration(
                        name2.RepresentText,
                        new MethodInfo(
                            new(luaFuncStat.NameExpr),
                            null,
                            new(luaFuncStat)
                        ),
                        DeclarationFeature.Global
                    );
                    AnalyzeDeclarationDoc(declaration, luaFuncStat);
                    declarationContext.Db.AddGlobal(DocumentId, true, name2.RepresentText, declaration);
                    declarationContext.AddDeclaration(luaFuncStat.NameExpr.Position, declaration);
                    var unResolved = new UnResolvedDeclaration(declaration, new LuaExprRef(closureExpr),
                        ResolveState.UnResolvedType);
                    declarationContext.AddUnResolved(unResolved);
                }

                break;
            }
            case { IsMethod: true, IndexExpr: { } indexExpr, ClosureExpr: { } closureExpr }:
            {
                if (indexExpr is { Name: { } name })
                {
                    var declaration = new LuaDeclaration(
                        name,
                        new MethodInfo(
                            new(indexExpr),
                            null,
                            new(luaFuncStat)
                        )
                    );
                    AnalyzeDeclarationDoc(declaration, luaFuncStat);
                    var unResolved = new UnResolvedDeclaration(declaration, new LuaExprRef(closureExpr),
                        ResolveState.UnResolvedIndex | ResolveState.UnResolvedType);
                    declarationContext.AddUnResolved(unResolved);
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
            foreach (var param in genericParams)
            {
                if (param is { Name: { } name })
                {
                    var declaration = new LuaDeclaration(
                        name.RepresentText,
                        new GenericParamInfo(
                            new(param),
                            searchContext.Infer(param.Type)
                        )
                    );
                    genericParameters.Add(declaration);
                }
            }
        }

        var overloads = docList?
            .OfType<LuaDocTagOverloadSyntax>()
            .Where(it => it.TypeFunc is not null)
            .Select(it => searchContext.Infer(it.TypeFunc))
            .Cast<LuaMethodType>()
            .Select(it => it.MainSignature).ToList();

        var parameters = new List<LuaDeclaration>();
        if (closureExprSyntax.ParamList is { } paramList)
        {
            declarationContext.PushScope(closureExprSyntax);
            parameters = AnalyzeParamListDeclaration(paramList);
            declarationContext.PopScope();
        }

        if (closureExprSyntax.Parent is LuaCallArgListSyntax { Parent: LuaCallExprSyntax callExprSyntax } callArgList)
        {
            var index = callArgList.ArgList.ToList().IndexOf(closureExprSyntax);
            var unResolved = new UnResolvedClosureParameters(parameters, callExprSyntax, index);
            declarationContext.AddUnResolved(unResolved);
        }

        var mainRetType = GetRetType(docList);
        if (mainRetType.Equals(Builtin.Unknown) && closureExprSyntax.Block is null)
        {
            mainRetType = Builtin.Nil;
        }

        var isColonDefine = closureExprSyntax.Parent is LuaFuncStatSyntax { IsColonFunc: true };
        var method = genericParameters.Count != 0
            ? new LuaGenericMethodType(
                genericParameters,
                new LuaSignature(mainRetType, parameters.Cast<IDeclaration>().ToList()), overloads, isColonDefine)
            : new LuaMethodType(new LuaSignature(mainRetType, parameters.Cast<IDeclaration>().ToList()), overloads,
                isColonDefine);
        if (closureExprSyntax.Block is { } block)
        {
            var unResolved = new UnResolvedMethod(method, block, ResolveState.UnResolveReturn);
            declarationContext.AddUnResolved(unResolved);
        }

        declarationContext.Db.AddIdRelatedType(closureExprSyntax.UniqueId, method);
    }

}
