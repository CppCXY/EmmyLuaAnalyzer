using EmmyLua.CodeAnalysis.Compilation.Analyzer.ResolveAnalyzer;
using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Reference;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Type;


namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.DeclarationAnalyzer.DeclarationWalker;

public partial class DeclarationWalker
{
    private void AnalyzeMethod(LuaFuncStatSyntax luaFuncStat)
    {
        switch (luaFuncStat)
        {
            case { IsLocal: true, LocalName.Name: { } name, ClosureExpr: { } closureExpr }:
            {
                var declaration = new LuaSymbol(
                    name.RepresentText,
                    new MethodInfo(
                        new(luaFuncStat.LocalName),
                        null,
                        new(luaFuncStat)
                    ),
                    SymbolFeature.Local
                );
                declarationContext.AddLocalDeclaration(luaFuncStat.LocalName, declaration);
                declarationContext.AddReference(ReferenceKind.Definition, declaration, luaFuncStat.LocalName);
                var unResolved = new UnResolvedSymbol(declaration, new LuaExprRef(closureExpr),
                    ResolveState.UnResolvedType);
                declarationContext.AddUnResolved(unResolved);
                break;
            }
            case { IsLocal: false, NameExpr: { Name: { } name2 } nameExpr, ClosureExpr: { } closureExpr }:
            {
                var prevDeclaration = declarationContext.FindLocalDeclaration(luaFuncStat.NameExpr);
                if (prevDeclaration is null)
                {
                    var declaration = new LuaSymbol(
                        name2.RepresentText,
                        new MethodInfo(
                            new(nameExpr),
                            null,
                            new(luaFuncStat)
                        ),
                        SymbolFeature.Global
                    );
                    declarationContext.TypeManager.AddGlobal(name2.RepresentText, declaration);
                    declarationContext.AddLocalDeclaration(nameExpr, declaration);
                    declarationContext.AddReference(ReferenceKind.Definition, declaration, nameExpr);
                    var unResolved = new UnResolvedSymbol(declaration, new LuaExprRef(closureExpr),
                        ResolveState.UnResolvedType);
                    declarationContext.AddUnResolved(unResolved);
                }
                else
                {
                    declarationContext.AddReference(ReferenceKind.Write, prevDeclaration, nameExpr);
                }

                break;
            }
            case { IsMethod: true, IndexExpr: { } indexExpr, ClosureExpr: { } closureExpr }:
            {
                if (indexExpr is { Name: { } name })
                {
                    var declaration = new LuaSymbol(
                        name,
                        new MethodInfo(
                            new(indexExpr),
                            null,
                            new(luaFuncStat)
                        )
                    );
                    declarationContext.AddAttachedDeclaration(indexExpr, declaration);
                    var unResolved = new UnResolvedSymbol(declaration, new LuaExprRef(closureExpr),
                        ResolveState.UnResolvedIndex | ResolveState.UnResolvedType);
                    declarationContext.AddUnResolved(unResolved);
                }

                break;
            }
        }
    }

    private void AnalyzeClosureExpr(LuaClosureExprSyntax closureExprSyntax)
    {
        foreach (var ancestor in closureExprSyntax.Ancestors)
        {
            if (ancestor is LuaStatSyntax or LuaTableFieldSyntax)
            {
                declarationContext.SetElementRelatedClosure(ancestor, closureExprSyntax);
                break;
            }
        }

        var parameters = new List<LuaSymbol>();
        if (closureExprSyntax.ParamList is { } paramList)
        {
            foreach (var param in paramList.Params)
            {
                if (param.Name is { } name)
                {
                    var paramName = name.RepresentText;
                    LuaType? paramType = null;
                    if (paramName is not "self")
                    {
                        paramType = new LuaElementType(param.UniqueId);
                        declarationContext.TypeManager.AddElementType(param.UniqueId);
                    }

                    var declaration = new LuaSymbol(
                        name.RepresentText,
                        new ParamInfo(
                            new(param),
                            paramType,
                            false
                        ),
                        SymbolFeature.Local
                    );

                    declarationContext.AddLocalDeclaration(param, declaration);
                    declarationContext.AddReference(ReferenceKind.Definition, declaration, param);
                    parameters.Add(declaration);
                }
                else if (param.IsVarArgs)
                {
                    var declaration = new LuaSymbol(
                        "...",
                        new ParamInfo(
                            new(param),
                            null,
                            true,
                            true
                        ),
                        SymbolFeature.Local
                    );

                    declarationContext.AddLocalDeclaration(param, declaration);
                    parameters.Add(declaration);
                }
            }
        }

        var isColonDefine = closureExprSyntax.Parent is LuaFuncStatSyntax { IsColonFunc: true };
        var method = new LuaMethodType(
            new LuaSignature(
                Builtin.Unknown,
                parameters.ToList()
            ),
            null,
            isColonDefine);

        declarationContext.TypeManager.AddElementType(closureExprSyntax.UniqueId);
        declarationContext.TypeManager.SetBaseType(closureExprSyntax.UniqueId, method);

        if (closureExprSyntax.Parent is LuaCallArgListSyntax { Parent: LuaCallExprSyntax callExprSyntax } callArgList)
        {
            var index = callArgList.ArgList.ToList().IndexOf(closureExprSyntax);
            var unResolved = new UnResolvedClosureParameters(parameters, callExprSyntax, index);
            declarationContext.AddUnResolved(unResolved);
        }

        if (closureExprSyntax.Block is { } block)
        {
            var unResolved = new UnResolvedMethod(closureExprSyntax.UniqueId, block, ResolveState.UnResolveReturn);
            declarationContext.AddUnResolved(unResolved);
        }
    }
}
