using EmmyLua.CodeAnalysis.Compilation.Analyzer.ResolveAnalyzer;
using EmmyLua.CodeAnalysis.Compilation.Reference;
using EmmyLua.CodeAnalysis.Compilation.Signature;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Compilation.Type.Types;
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
                var declaration = new LuaSymbol(
                    name.RepresentText,
                    new LuaMethodType(LuaSignatureId.Create(closureExpr)),
                    new MethodInfo(
                        new(luaFuncStat.LocalName),
                        new(luaFuncStat)
                    ),
                    SymbolFeature.Local
                );
                builder.AddLocalDeclaration(luaFuncStat.LocalName, declaration);
                builder.AddReference(ReferenceKind.Definition, declaration, luaFuncStat.LocalName);
                break;
            }
            case { IsLocal: false, NameExpr: { Name: { } name2 } nameExpr, ClosureExpr: { } closureExpr }:
            {
                var prevDeclaration = builder.FindLocalDeclaration(luaFuncStat.NameExpr);
                if (prevDeclaration is null)
                {
                    var declaration = new LuaSymbol(
                        name2.RepresentText,
                        new LuaMethodType(LuaSignatureId.Create(closureExpr)),
                        new MethodInfo(
                            new(nameExpr),
                            new(luaFuncStat)
                        ),
                        SymbolFeature.Global
                    );
                    builder.GlobalIndex.AddGlobal(name2.RepresentText, declaration);
                    builder.AddLocalDeclaration(nameExpr, declaration);
                    builder.AddReference(ReferenceKind.Definition, declaration, nameExpr);
                }
                else
                {
                    builder.AddReference(ReferenceKind.Write, prevDeclaration, nameExpr);
                }

                break;
            }
            case { IsMethod: true, IndexExpr: { } indexExpr, ClosureExpr: { } closureExpr }:
            {
                if (indexExpr is { Name: { } name })
                {
                    var declaration = new LuaSymbol(
                        name,
                        new LuaMethodType(LuaSignatureId.Create(closureExpr)),
                        new MethodInfo(
                            new(indexExpr),
                            new(luaFuncStat)
                        )
                    );
                    builder.AddAttachedDeclaration(indexExpr, declaration);
                    var unResolved = new UnResolvedSymbol(
                        declaration,
                        null,
                        ResolveState.UnResolvedIndex);
                    builder.AddUnResolved(unResolved);
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
                builder.AddRelatedClosure(ancestor, closureExprSyntax);
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
                    var declaration = new LuaSymbol(
                        paramName,
                        null,
                        new ParamInfo(
                            new(param),
                            false
                        ),
                        SymbolFeature.Local
                    );

                    builder.AddLocalDeclaration(param, declaration);
                    builder.AddReference(ReferenceKind.Definition, declaration, param);
                    parameters.Add(declaration);
                }
                else if (param.IsVarArgs)
                {
                    var declaration = new LuaSymbol(
                        "...",
                        null,
                        new ParamInfo(
                            new(param),
                            true,
                            true
                        ),
                        SymbolFeature.Local
                    );

                    builder.AddLocalDeclaration(param, declaration);
                    parameters.Add(declaration);
                }
            }
        }

        var isColonDefine = closureExprSyntax.Parent is LuaFuncStatSyntax { IsColonFunc: true };
        var signature = new LuaSignature(
            null,
            parameters,
            isColonDefine
        );

        Compilation.SignatureManager.AddSignature(LuaSignatureId.Create(closureExprSyntax), signature);
        // var method = new LuaMethodType(
        //     new LuaSignature(
        //         Builtin.Unknown,
        //         parameters.ToList()
        //     ),
        //     null,
        //     isColonDefine);

        // builder.TypeManager.AddLocalTypeInfo(closureExprSyntax.UniqueId);
        // builder.TypeManager.SetBaseType(closureExprSyntax.UniqueId, method);

        if (closureExprSyntax.Parent is LuaCallArgListSyntax { Parent: LuaCallExprSyntax callExprSyntax } callArgList)
        {
            var index = callArgList.ArgList.ToList().IndexOf(closureExprSyntax);
            var unResolved = new UnResolvedClosureParameters(parameters, callExprSyntax, index);
            builder.AddUnResolved(unResolved);
        }

        if (closureExprSyntax.Block is { } block)
        {
            var unResolved = new UnResolvedMethod(closureExprSyntax.UniqueId, block, ResolveState.UnResolveReturn);
            builder.AddUnResolved(unResolved);
        }
    }
}
