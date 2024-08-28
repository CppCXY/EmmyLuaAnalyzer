using EmmyLua.CodeAnalysis.Compilation.Analyzer.ResolveAnalyzer;
using EmmyLua.CodeAnalysis.Compilation.Reference;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Compilation.Type.Types;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.DeclarationAnalyzer.DeclarationWalker;

public partial class DeclarationWalker
{
    private void AnalyzeLocalStat(LuaLocalStatSyntax localStatSyntax)
    {
        var nameList = localStatSyntax.NameList.ToList();
        var exprList = localStatSyntax.ExprList.ToList();
        LuaExprSyntax? lastValidExpr = null;
        var count = nameList.Count;
        var retId = 0;
        for (var i = 0; i < count; i++)
        {
            var localName = nameList[i];
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
                var declaration = new LuaSymbol(
                    name.RepresentText,
                    null,
                    new LocalInfo(
                        new(localName),
                        localName.Attribute?.IsConst ?? false,
                        localName.Attribute?.IsClose ?? false
                    ),
                    SymbolFeature.Local
                );
                declarationContext.AddLocalDeclaration(localName, declaration);
                declarationContext.AddReference(ReferenceKind.Definition, declaration, localName);
                var unResolveDeclaration = new UnResolvedSymbol(
                    declaration,
                    relatedExpr,
                    ResolveState.UnResolvedType
                );
                declarationContext.AddUnResolved(unResolveDeclaration);
            }
        }
    }

    private void AnalyzeForRangeStat(LuaForRangeStatSyntax forRangeStatSyntax)
    {
        var parameters = new List<LuaSymbol>();
        foreach (var param in forRangeStatSyntax.IteratorNames)
        {
            if (param.Name is { } name)
            {
                var declaration = new LuaSymbol(
                    name.RepresentText,
                    null,
                    new ParamInfo(
                        new(param),
                        false
                    ),
                    SymbolFeature.Local);
                // declarationContext.TypeManager.AddDocumentElementType(param.UniqueId);
                declarationContext.AddLocalDeclaration(param, declaration);
                declarationContext.AddReference(ReferenceKind.Definition, declaration, param);
                parameters.Add(declaration);
            }
        }

        var unResolved = new UnResolvedForRangeParameter(parameters, forRangeStatSyntax.ExprList.ToList());
        declarationContext.AddUnResolved(unResolved);
    }

    private void AnalyzeForStat(LuaForStatSyntax forStatSyntax)
    {
        if (forStatSyntax is { IteratorName.Name: { } name })
        {
            var declaration = new LuaSymbol(
                name.RepresentText,
                Builtin.Integer,
                new ParamInfo(
                    new(forStatSyntax.IteratorName),
                    false
                ),
                SymbolFeature.Local);
            declarationContext.AddReference(ReferenceKind.Definition, declaration, forStatSyntax.IteratorName);
            declarationContext.AddLocalDeclaration(forStatSyntax.IteratorName, declaration);
        }
    }

    private void AnalyzeAssignStat(LuaAssignStatSyntax luaAssignStat)
    {
        var varList = luaAssignStat.VarList.ToList();
        var exprList = luaAssignStat.ExprList.ToList();
        LuaExprSyntax? lastValidExpr = null;
        var retId = 0;
        var count = varList.Count;
        for (var i = 0; i < count; i++)
        {
            var varExpr = varList[i];
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
                        var prevDeclaration = declarationContext.FindLocalDeclaration(nameExpr);
                        if (prevDeclaration is null)
                        {
                            var nameText = name.RepresentText;
                            var declaration = new LuaSymbol(
                                nameText,
                                null,
                                new GlobalInfo(new(nameExpr)),
                                SymbolFeature.Global
                            );
                            declarationContext.TypeManager.AddGlobal(nameText, declaration);
                            declarationContext.AddLocalDeclaration(nameExpr, declaration);
                            declarationContext.AddReference(ReferenceKind.Definition, declaration, nameExpr);
                            var unResolveDeclaration = new UnResolvedSymbol(
                                declaration,
                                relatedExpr,
                                ResolveState.UnResolvedType
                            );
                            declarationContext.AddUnResolved(unResolveDeclaration);
                        }
                        else
                        {
                            declarationContext.AddReference(ReferenceKind.Write, prevDeclaration, nameExpr);
                        }
                    }

                    break;
                }
                case LuaIndexExprSyntax indexExpr:
                {
                    if (indexExpr.Name is { } name)
                    {
                        var valueExprPtr = relatedExpr?.RetId == 0
                            ? new(relatedExpr.Expr)
                            : LuaElementPtr<LuaExprSyntax>.Empty;
                        var declaration = new LuaSymbol(
                            name,
                            null,
                            new IndexInfo(new(indexExpr), valueExprPtr)
                        );
                        declarationContext.AddAttachedDeclaration(varExpr, declaration);
                        var unResolveDeclaration = new UnResolvedSymbol(declaration, relatedExpr,
                            ResolveState.UnResolvedType | ResolveState.UnResolvedIndex);
                        declarationContext.AddUnResolved(unResolveDeclaration);
                    }

                    break;
                }
            }
        }
    }
}
