using EmmyLua.CodeAnalysis.Compilation.Analyzer.ResolveAnalyzer;
using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Compilation.Type.TypeInfo;
using EmmyLua.CodeAnalysis.Compilation.Type.Types;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.DeclarationAnalyzer.DeclarationWalker;

public partial class DeclarationWalker
{
    private void AnalyzeTypeOperator(LuaTypeInfo luaTypeInfo, LuaNamedType namedType, LuaDocTagSyntax typeTag)
    {
        foreach (var operatorSyntax in typeTag.NextOfType<LuaDocTagOperatorSyntax>())
        {
            switch (operatorSyntax.Operator?.RepresentText)
            {
                case "add":
                {
                    AddUnResolveOperator(luaTypeInfo, namedType, TypeOperatorKind.Add, operatorSyntax, 1);
                    break;
                }
                case "sub":
                {
                    AddUnResolveOperator(luaTypeInfo, namedType, TypeOperatorKind.Sub, operatorSyntax, 1);
                    break;
                }
                case "mul":
                {
                    AddUnResolveOperator(luaTypeInfo, namedType, TypeOperatorKind.Mul, operatorSyntax, 1);
                    break;
                }
                case "div":
                {
                    AddUnResolveOperator(luaTypeInfo, namedType, TypeOperatorKind.Div, operatorSyntax, 1);
                    break;
                }
                case "mod":
                {
                    AddUnResolveOperator(luaTypeInfo, namedType, TypeOperatorKind.Mod, operatorSyntax, 1);
                    break;
                }
                case "pow":
                {
                    AddUnResolveOperator(luaTypeInfo, namedType, TypeOperatorKind.Pow, operatorSyntax, 1);
                    break;
                }
                case "unm":
                {
                    AddUnResolveOperator(luaTypeInfo, namedType, TypeOperatorKind.Unm, operatorSyntax, 0);
                    break;
                }
                case "idiv":
                {
                    AddUnResolveOperator(luaTypeInfo, namedType, TypeOperatorKind.Idiv, operatorSyntax, 1);
                    break;
                }
                case "band":
                {
                    AddUnResolveOperator(luaTypeInfo, namedType, TypeOperatorKind.Band, operatorSyntax, 1);
                    break;
                }
                case "bor":
                {
                    AddUnResolveOperator(luaTypeInfo, namedType, TypeOperatorKind.Bor, operatorSyntax, 1);
                    break;
                }
                case "bxor":
                {
                    AddUnResolveOperator(luaTypeInfo, namedType, TypeOperatorKind.Bxor, operatorSyntax, 1);
                    break;
                }
                case "bnot":
                {
                    AddUnResolveOperator(luaTypeInfo, namedType, TypeOperatorKind.Bnot, operatorSyntax, 0);
                    break;
                }
                case "shl":
                {
                    AddUnResolveOperator(luaTypeInfo, namedType, TypeOperatorKind.Shl, operatorSyntax, 1);
                    break;
                }
                case "shr":
                {
                    AddUnResolveOperator(luaTypeInfo, namedType, TypeOperatorKind.Shr, operatorSyntax, 1);
                    break;
                }
                case "concat":
                {
                    AddUnResolveOperator(luaTypeInfo, namedType, TypeOperatorKind.Concat, operatorSyntax, 1);
                    break;
                }
                case "len":
                {
                    AddUnResolveOperator(luaTypeInfo, namedType, TypeOperatorKind.Len, operatorSyntax, 0);
                    break;
                }
                case "eq":
                {
                    AddUnResolveOperator(luaTypeInfo, namedType, TypeOperatorKind.Eq, operatorSyntax, 1);
                    break;
                }
                case "lt":
                {
                    AddUnResolveOperator(luaTypeInfo, namedType, TypeOperatorKind.Lt, operatorSyntax, 1);
                    break;
                }
                case "le":
                {
                    AddUnResolveOperator(luaTypeInfo, namedType, TypeOperatorKind.Le, operatorSyntax, 1);
                    break;
                }
            }
        }

        // var overloads = new List<LuaTypeInfo.OverloadStub>();
        foreach (var overloadSyntax in typeTag.NextOfType<LuaDocTagOverloadSyntax>())
        {
            if (overloadSyntax.TypeFunc is { UniqueId: { } id })
            {
                var unResolved = new UnResolvedDocOperator(
                    luaTypeInfo,
                    namedType,
                    TypeOperatorKind.Call,
                    id,
                    [new(id)],
                    ResolveState.UnResolvedType
                );
                declarationContext.AddUnResolved(unResolved);
            }
        }
    }

    private void AddUnResolveOperator(
        LuaTypeInfo luaTypeInfo,
        LuaNamedType namedType,
        TypeOperatorKind kind,
        LuaDocTagOperatorSyntax operatorSyntax,
        int paramCount
    )
    {
        SyntaxElementId operatorId = operatorSyntax.Operator?.UniqueId ?? SyntaxElementId.Empty;

        var typeIds = new List<TypeId>();
        if (operatorSyntax.Types is { } types)
        {
            foreach (var type in types)
            {
                if (paramCount <= 0)
                {
                    break;
                }

                if (type.UniqueId is { } id)
                {
                    typeIds.Add(new(id));
                }

                paramCount--;
            }
        }

        if (operatorSyntax.ReturnType is { UniqueId: { } id2 })
        {
            typeIds.Add(new(id2));
        }

        var unResolved = new UnResolvedDocOperator(
            luaTypeInfo,
            namedType,
            kind,
            operatorId,
            typeIds,
            ResolveState.UnResolvedType
        );
        declarationContext.AddUnResolved(unResolved);
    }
}
