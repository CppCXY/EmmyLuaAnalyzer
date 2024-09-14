﻿using EmmyLua.CodeAnalysis.Compilation.Analyzer.ResolveAnalyzer;
using EmmyLua.CodeAnalysis.Compilation.Reference;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.DeclarationAnalyzer.DeclarationWalker;

public partial class DeclarationWalker
{
    private void AnalyzeLocalStat(LuaLocalStatSyntax localStatSyntax)
    {
        foreach (var (localName, (expr, idx)) in localStatSyntax.NameExprPairs)
        {
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
                builder.AddLocalDeclaration(localName, declaration);
                builder.AddReference(ReferenceKind.Definition, declaration, localName);
                if (expr is not null)
                {
                    var unResolveDeclaration = new UnResolvedSymbol(
                        declaration,
                        new LuaExprRef(expr, idx),
                        ResolveState.UnResolvedType
                    );
                    builder.AddUnResolved(unResolveDeclaration);
                }
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
                // builder.TypeManager.AddLocalTypeInfo(param.UniqueId);
                builder.AddLocalDeclaration(param, declaration);
                builder.AddReference(ReferenceKind.Definition, declaration, param);
                parameters.Add(declaration);
            }
        }

        var unResolved = new UnResolvedForRangeParameter(parameters, forRangeStatSyntax.ExprList.ToList());
        builder.AddUnResolved(unResolved);
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
            builder.AddReference(ReferenceKind.Definition, declaration, forStatSyntax.IteratorName);
            builder.AddLocalDeclaration(forStatSyntax.IteratorName, declaration);
        }
    }

    private void AnalyzeAssignStat(LuaAssignStatSyntax luaAssignStat)
    {
        foreach (var (varExpr, (expr, idx)) in luaAssignStat.VarExprPairs)
        {
            switch (varExpr)
            {
                case LuaNameExprSyntax { Name: { } name } nameExpr:
                {
                    var prevDeclaration = builder.FindLocalDeclaration(nameExpr);
                    if (prevDeclaration is null)
                    {
                        var nameText = name.RepresentText;
                        var declaration = new LuaSymbol(
                            nameText,
                            null,
                            new GlobalInfo(new(nameExpr)),
                            SymbolFeature.Global
                        );
                        builder.GlobalIndex.AddGlobal(nameText, declaration);
                        builder.AddLocalDeclaration(nameExpr, declaration);
                        builder.AddReference(ReferenceKind.Definition, declaration, nameExpr);
                        var unResolveDeclaration = new UnResolvedSymbol(
                            declaration,
                            new LuaExprRef(expr, idx),
                            ResolveState.UnResolvedType
                        );
                        builder.AddUnResolved(unResolveDeclaration);
                    }
                    else
                    {
                        builder.AddReference(ReferenceKind.Write, prevDeclaration, nameExpr);
                    }
                    break;
                }
                case LuaIndexExprSyntax indexExpr:
                {
                    if (indexExpr.Name is { } name)
                    {
                        // var valueExprPtr = relatedExpr?.RetId == 0
                        //     ? new(relatedExpr.Expr)
                        //     : LuaElementPtr<LuaExprSyntax>.Empty;
                        // var declaration = new LuaSymbol(
                        //     name,
                        //     null,
                        //     new IndexInfo(new(indexExpr), valueExprPtr)
                        // );
                        // builder.AddAttachedDeclaration(varExpr, declaration);
                        // var unResolveDeclaration = new UnResolvedSymbol(declaration, relatedExpr,
                        //     ResolveState.UnResolvedType | ResolveState.UnResolvedIndex);
                        // builder.AddUnResolved(unResolveDeclaration);
                    }

                    break;
                }
            }
        }
    }
}
