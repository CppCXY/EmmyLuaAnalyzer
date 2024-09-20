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
        var attachedTypes = GetAttachedTypes(localStatSyntax);
        var nameExprPairs = localStatSyntax.NameExprPairs.ToList();
        for (var i = 0; i < nameExprPairs.Count; i++)
        {
            var (localName, (expr, idx)) = nameExprPairs[i];
            var type = ResolveVariableType(attachedTypes, i, expr, idx);

            if (localName is { Name: { } name })
            {
                var declaration = new LuaSymbol(
                    name.RepresentText,
                    type,
                    new LocalInfo(
                        new(localName),
                        localName.Attribute?.IsConst ?? false,
                        localName.Attribute?.IsClose ?? false
                    ),
                    SymbolFeature.Local
                );
                builder.AddLocalDeclaration(localName, declaration);
                builder.AddReference(ReferenceKind.Definition, declaration, localName);
                if (expr is not null && type is null)
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

    private record struct ParameterInfo(bool Nullable, LuaType Type);

    private void AnalyzeForRangeStat(LuaForRangeStatSyntax forRangeStatSyntax)
    {
        var docList = FindAttachedDoc(forRangeStatSyntax);
        var parameterDict = new Dictionary<string, ParameterInfo>();
        foreach (var paramSyntax in docList.OfType<LuaDocTagParamSyntax>())
        {
            if (paramSyntax is { Name.RepresentText: { } name, Type: { } paramType })
            {
                var type = builder.CreateRef(paramType);
                var nullable = paramSyntax.Nullable;
                parameterDict[name] = new ParameterInfo(nullable, type);
            }
        }

        var parameters = new List<LuaSymbol>();
        foreach (var param in forRangeStatSyntax.IteratorNames)
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

                var declaration = new LuaSymbol(
                    name.RepresentText,
                    type,
                    new ParamInfo(
                        new(param),
                        false,
                        nullable
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
        var attachedTypes = GetAttachedTypes(luaAssignStat);
        var varExprPairs = luaAssignStat.VarExprPairs.ToList();
        for (var i = 0; i < varExprPairs.Count; i++)
        {
            var (varExpr, (expr, idx)) = varExprPairs[i];
            var type = ResolveVariableType(attachedTypes, i, expr, idx);

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
                            type,
                            new GlobalInfo(new(nameExpr)),
                            SymbolFeature.Global
                        );
                        builder.GlobalIndex.AddGlobal(nameText, declaration);
                        builder.AddLocalDeclaration(nameExpr, declaration);
                        builder.AddReference(ReferenceKind.Definition, declaration, nameExpr);
                        if (expr is not null && type is null)
                        {
                            var unResolvedSymbol = new UnResolvedSymbol(
                                declaration,
                                new LuaExprRef(expr, idx),
                                ResolveState.UnResolvedType
                            );
                            builder.AddUnResolved(unResolvedSymbol);
                        }
                    }
                    else
                    {
                        builder.AddReference(ReferenceKind.Write, prevDeclaration, nameExpr);
                    }

                    break;
                }
                case LuaIndexExprSyntax { Name: { } name } indexExpr:
                {
                    var valueExprPtr = (idx == 0 && expr is not null)
                        ? new(expr)
                        : LuaElementPtr<LuaExprSyntax>.Empty;
                    var declaration = new LuaSymbol(
                        name,
                        type,
                        new IndexInfo(new(indexExpr), valueExprPtr)
                    );
                    builder.AddAttachedDeclaration(varExpr, declaration);
                    builder.AddIndexExprMember(indexExpr, declaration);
                    if (expr is not null && type is null)
                    {
                        var unResolved = new UnResolvedSymbol(
                            declaration,
                            new LuaExprRef(expr, idx),
                            ResolveState.UnResolvedType
                        );
                        builder.AddUnResolved(unResolved);
                    }

                    break;
                }
            }
        }
    }

    private List<LuaType> GetAttachedTypes(LuaSyntaxElement element)
    {
        return _attachedTypes.TryGetValue(element.UniqueId, out var list) ? list : [];
    }

    private LuaType? SimpleInfer(LuaExprSyntax? expr, int idx)
    {
        if (idx != 0)
        {
            return null;
        }

        switch (expr)
        {
            case LuaTableExprSyntax tableExprSyntax:
            {
                AnalyzeTableExpr(tableExprSyntax);
                var elementType = new LuaElementRef(tableExprSyntax.UniqueId);
                return elementType;
            }
            case LuaLiteralExprSyntax { Literal: { } literal }:
            {
                return literal switch
                {
                    LuaIntegerToken  => Builtin.Integer,
                    LuaStringToken  => Builtin.String,
                    LuaFloatToken  => Builtin.Number,
                    LuaBoolToken => Builtin.Boolean,
                    LuaNilToken => Builtin.Nil,
                    _ => Builtin.Unknown
                };
            }
            default:
            {
                return null;
            }
        }
    }

    private LuaType? ResolveVariableType(List<LuaType> attachedTypes, int idx, LuaExprSyntax? expr, int retIdx)
    {
        var exprType = SimpleInfer(expr, retIdx);
        var type = attachedTypes.Count > idx ? attachedTypes[idx] : exprType;
        if (type != exprType &&
            (type is LuaNamedType namedType && namedType.DocumentId == DocumentId) &&
            exprType is LuaElementRef elementType)
        {
            var typeInfo = builder.TypeManager.FindTypeInfo(namedType);
            var elementTypeInfo = builder.TypeManager.FindTypeInfo(elementType.Id);
            if (typeInfo is not null && elementTypeInfo is { Declarations.Values: {} declarations})
            {
                foreach (var declaration in declarations)
                {
                    typeInfo.AddDeclaration(declaration);
                }
            }
        }

        return type;
    }
}
