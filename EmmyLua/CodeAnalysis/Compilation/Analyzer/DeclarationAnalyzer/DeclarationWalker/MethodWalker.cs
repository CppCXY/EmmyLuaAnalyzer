using EmmyLua.CodeAnalysis.Common;
using EmmyLua.CodeAnalysis.Compilation.Analyzer.ResolveAnalyzer;
using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Reference;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.DeclarationAnalyzer.DeclarationWalker;

public partial class DeclarationWalker
{
    private void AnalyzeMethod(LuaFuncStatSyntax luaFuncStat)
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
                declarationContext.AddLocalDeclaration(luaFuncStat.LocalName, declaration);
                declarationContext.AddReference(ReferenceKind.Definition, declaration, luaFuncStat.LocalName);
                var unResolved = new UnResolvedDeclaration(declaration, new LuaExprRef(closureExpr),
                    ResolveState.UnResolvedType);
                declarationContext.AddUnResolved(unResolved);
                break;
            }
            case { IsLocal: false, NameExpr: { Name: { } name2 } nameExpr, ClosureExpr: { } closureExpr }:
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
                    declarationContext.AddLocalDeclaration(nameExpr, declaration);
                    declarationContext.AddReference(ReferenceKind.Definition, declaration, nameExpr);
                    declarationContext.Db.AddGlobal(DocumentId, name2.RepresentText, declaration, true);
                    var unResolved = new UnResolvedDeclaration(declaration, new LuaExprRef(closureExpr),
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
                    var declaration = new LuaDeclaration(
                        name,
                        new MethodInfo(
                            new(indexExpr),
                            null,
                            new(luaFuncStat)
                        )
                    );
                    declarationContext.AddAttachedDeclaration(indexExpr, declaration);
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
        foreach (var ancestor in closureExprSyntax.Ancestors)
        {
            if (ancestor is LuaStatSyntax or LuaTableFieldSyntax)
            {
                declarationContext.SetElementRelatedClosure(ancestor, closureExprSyntax);
                break;
            }
        }

        var parameters = new List<LuaDeclaration>();
        if (closureExprSyntax.ParamList is { } paramList)
        {
            foreach (var param in paramList.Params)
            {
                if (param.Name is { } name)
                {
                    var declaration = new LuaDeclaration(
                        name.RepresentText,
                        new ParamInfo(
                            new(param),
                            null,
                            false,
                            false
                        ),
                        DeclarationFeature.Local
                    );
                    declarationContext.AddLocalDeclaration(param, declaration);
                    declarationContext.AddReference(ReferenceKind.Definition, declaration, param);
                    parameters.Add(declaration);
                }
                else if (param.IsVarArgs)
                {
                    var declaration = new LuaDeclaration(
                        "...",
                        new ParamInfo(
                            new(param),
                            null,
                            true,
                            true
                        ),
                        DeclarationFeature.Local
                    );

                    declarationContext.AddLocalDeclaration(param, declaration);
                    parameters.Add(declaration);
                }
            }
        }

        var isColonDefine = closureExprSyntax.Parent is LuaFuncStatSyntax { IsColonFunc: true };
        var method = new LuaMethodType(
            new LuaSignature(
                Builtin.Nil,
                parameters.Cast<IDeclaration>().ToList()
            ),
            null,
            isColonDefine);

        declarationContext.Db.UpdateIdRelatedType(closureExprSyntax.UniqueId, method);

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
