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
            case { IsMethod: true, IndexExpr: { Name: { } name } indexExpr, ClosureExpr: { } closureExpr }:
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
                builder.AddIndexExprMember(indexExpr, declaration);

                break;
            }
        }
    }

    private void AnalyzeClosureExpr(LuaClosureExprSyntax closureExprSyntax)
    {
        var docList = FindAttachedDoc(closureExprSyntax);
        var parameterDict = new Dictionary<string, ParameterInfo>();
        LuaType? returnType = null;
        foreach (var docTagSyntax in docList)
        {
            if (docTagSyntax is LuaDocTagParamSyntax paramSyntax)
            {
                if (paramSyntax is { Name.RepresentText: { } name, Type: { } paramType })
                {
                    var type = builder.CreateRef(paramType);
                    var nullable = paramSyntax.Nullable;
                    parameterDict[name] = new ParameterInfo(nullable, type);
                }
                else if (paramSyntax is { VarArgs: { } _, Type: { } paramType2 })
                {
                    var type = builder.CreateRef(paramType2);
                    parameterDict["..."] = new ParameterInfo(true, type);
                }
            }
            else if (docTagSyntax is LuaDocTagReturnSyntax returnSyntax)
            {
                var returnTypes = returnSyntax.TypeList
                    .Select(builder.CreateRef)
                    .Cast<LuaType>()
                    .ToList();
                returnType = returnTypes.Count switch
                {
                    0 => Builtin.Nil,
                    1 => returnTypes[0],
                    _ => new LuaMultiReturnType(returnTypes)
                };
            }
        }

        var parameters = new List<LuaSymbol>();
        if (closureExprSyntax.ParamList is { } paramList)
        {
            foreach (var param in paramList.Params)
            {
                if (param.Name is { } name)
                {
                    LuaType? type = null;
                    var nullable = false;
                    if (parameterDict.TryGetValue(name.RepresentText, out var info))
                    {
                        type = info.Type;
                        nullable = info.Nullable;
                    }

                    var paramName = name.RepresentText;
                    var declaration = new LuaSymbol(
                        paramName,
                        type,
                        new ParamInfo(
                            new(param),
                            false,
                            nullable
                        ),
                        SymbolFeature.Local
                    );

                    builder.AddLocalDeclaration(param, declaration);
                    builder.AddReference(ReferenceKind.Definition, declaration, param);
                    parameters.Add(declaration);
                }
                else if (param.IsVarArgs)
                {
                    LuaType? type = null;
                    if (parameterDict.TryGetValue("...", out var info))
                    {
                        type = info.Type;
                    }
                    var declaration = new LuaSymbol(
                        "...",
                        type,
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
            returnType,
            parameters,
            isColonDefine
        );

        Compilation.SignatureManager.AddSignature(LuaSignatureId.Create(closureExprSyntax), signature);
        // TODO overload

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
