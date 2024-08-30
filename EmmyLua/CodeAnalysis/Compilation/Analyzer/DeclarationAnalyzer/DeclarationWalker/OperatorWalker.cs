using EmmyLua.CodeAnalysis.Compilation.Analyzer.ResolveAnalyzer;
using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Compilation.Type.Types;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.DeclarationAnalyzer.DeclarationWalker;

public partial class DeclarationWalker
{
    private void AnalyzeTypeOperator(LuaNamedType namedType, LuaDocTagSyntax typeTag)
    {
        foreach (var operatorSyntax in typeTag.NextOfType<LuaDocTagOperatorSyntax>())
        {
            switch (operatorSyntax.Operator?.RepresentText)
            {
                case "add":
                {
                    AddUnResolveOperator(namedType, TypeOperatorKind.Add, operatorSyntax, 1);
                    break;
                }
                case "sub":
                {
                    AddUnResolveOperator(namedType, TypeOperatorKind.Sub, operatorSyntax, 1);
                    break;
                }
                case "mul":
                {
                    AddUnResolveOperator(namedType, TypeOperatorKind.Mul, operatorSyntax, 1);
                    break;
                }
                case "div":
                {
                    AddUnResolveOperator(namedType, TypeOperatorKind.Div, operatorSyntax, 1);
                    break;
                }
                case "mod":
                {
                    AddUnResolveOperator(namedType, TypeOperatorKind.Mod, operatorSyntax, 1);
                    break;
                }
                case "pow":
                {
                    AddUnResolveOperator(namedType, TypeOperatorKind.Pow, operatorSyntax, 1);
                    break;
                }
                case "unm":
                {
                    AddUnResolveOperator(namedType, TypeOperatorKind.Unm, operatorSyntax, 0);
                    break;
                }
                case "idiv":
                {
                    AddUnResolveOperator(namedType, TypeOperatorKind.Idiv, operatorSyntax, 1);
                    break;
                }
                case "band":
                {
                    AddUnResolveOperator(namedType, TypeOperatorKind.Band, operatorSyntax, 1);
                    break;
                }
                case "bor":
                {
                    AddUnResolveOperator(namedType, TypeOperatorKind.Bor, operatorSyntax, 1);
                    break;
                }
                case "bxor":
                {
                    AddUnResolveOperator(namedType, TypeOperatorKind.Bxor, operatorSyntax, 1);
                    break;
                }
                case "bnot":
                {
                    AddUnResolveOperator(namedType, TypeOperatorKind.Bnot, operatorSyntax, 0);
                    break;
                }
                case "shl":
                {
                    AddUnResolveOperator(namedType, TypeOperatorKind.Shl, operatorSyntax, 1);
                    break;
                }
                case "shr":
                {
                    AddUnResolveOperator(namedType, TypeOperatorKind.Shr, operatorSyntax, 1);
                    break;
                }
                case "concat":
                {
                    AddUnResolveOperator(namedType, TypeOperatorKind.Concat, operatorSyntax, 1);
                    break;
                }
                case "len":
                {
                    AddUnResolveOperator(namedType, TypeOperatorKind.Len, operatorSyntax, 0);
                    break;
                }
                case "eq":
                {
                    AddUnResolveOperator(namedType, TypeOperatorKind.Eq, operatorSyntax, 1);
                    break;
                }
                case "lt":
                {
                    AddUnResolveOperator(namedType, TypeOperatorKind.Lt, operatorSyntax, 1);
                    break;
                }
                case "le":
                {
                    AddUnResolveOperator(namedType, TypeOperatorKind.Le, operatorSyntax, 1);
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
                    namedType,
                    TypeOperatorKind.Call,
                    id,
                    [new (id)],
                    ResolveState.UnResolvedType
                );
                declarationContext.AddUnResolved(unResolved);
            }

        }
    }

    private void AddUnResolveOperator(
        LuaNamedType namedType,
        TypeOperatorKind kind,
        LuaDocTagOperatorSyntax operatorSyntax,
        int paramCount
        )
    {
        SyntaxElementId operatorId = operatorSyntax.Operator?.UniqueId ?? SyntaxElementId.Empty;

        var typeIds = new List<TypeId>();
        if (operatorSyntax.Types is {} types)
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
            namedType,
            kind,
            operatorId,
            typeIds,
            ResolveState.UnResolvedType
        );
        declarationContext.AddUnResolved(unResolved);
    }
}
