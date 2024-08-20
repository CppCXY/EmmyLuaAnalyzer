﻿using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Type;
using EmmyLua.CodeAnalysis.Type.Manager.TypeInfo;
using EmmyLua.CodeAnalysis.Type.Types;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.DeclarationAnalyzer.DeclarationWalker;

public partial class DeclarationWalker
{
    private void AnalyzeTypeOperator(LuaNamedType namedType, LuaDocTagSyntax typeTag)
    {
        var operators = new List<TypeOperator>();
        foreach (var operatorSyntax in typeTag.NextOfType<LuaDocTagOperatorSyntax>())
        {
            switch (operatorSyntax.Operator?.RepresentText)
            {
                case "add":
                {
                    var type = searchContext.Infer(operatorSyntax.Types.FirstOrDefault());
                    var retType = searchContext.Infer(operatorSyntax.ReturnType);
                    var opDeclaration = new LuaSymbol(
                        string.Empty,
                        retType,
                        new TypeOpInfo(new(operatorSyntax)));
                    var op = new BinaryOperator(TypeOperatorKind.Add, namedType, type, retType, opDeclaration);
                    operators.Add(op);
                    break;
                }
                case "sub":
                {
                    var type = searchContext.Infer(operatorSyntax.Types.FirstOrDefault());
                    var retType = searchContext.Infer(operatorSyntax.ReturnType);
                    var opDeclaration = new LuaSymbol(
                        string.Empty,
                        retType,
                        new TypeOpInfo(new(operatorSyntax)));
                    var op = new BinaryOperator(TypeOperatorKind.Sub, namedType, type, retType, opDeclaration);
                    operators.Add(op);
                    break;
                }
                case "mul":
                {
                    var type = searchContext.Infer(operatorSyntax.Types.FirstOrDefault());
                    var retType = searchContext.Infer(operatorSyntax.ReturnType);
                    var opDeclaration = new LuaSymbol(
                        string.Empty,
                        retType,
                        new TypeOpInfo(new(operatorSyntax)));
                    var op = new BinaryOperator(TypeOperatorKind.Mul, namedType, type, retType, opDeclaration);
                    operators.Add(op);
                    break;
                }
                case "div":
                {
                    var type = searchContext.Infer(operatorSyntax.Types.FirstOrDefault());
                    var retType = searchContext.Infer(operatorSyntax.ReturnType);
                    var opDeclaration = new LuaSymbol(
                        string.Empty,
                        retType,
                        new TypeOpInfo(new(operatorSyntax)));
                    var op = new BinaryOperator(TypeOperatorKind.Div, namedType, type, retType, opDeclaration);
                    operators.Add(op);
                    break;
                }
                case "mod":
                {
                    var type = searchContext.Infer(operatorSyntax.Types.FirstOrDefault());
                    var retType = searchContext.Infer(operatorSyntax.ReturnType);
                    var opDeclaration = new LuaSymbol(
                        string.Empty,
                        retType,
                        new TypeOpInfo(new(operatorSyntax)));
                    var op = new BinaryOperator(TypeOperatorKind.Mod, namedType, type, retType, opDeclaration);
                    operators.Add(op);
                    break;
                }
                case "pow":
                {
                    var type = searchContext.Infer(operatorSyntax.Types.FirstOrDefault());
                    var retType = searchContext.Infer(operatorSyntax.ReturnType);
                    var opDeclaration = new LuaSymbol(
                        string.Empty,
                        retType,
                        new TypeOpInfo(new(operatorSyntax)));
                    var op = new BinaryOperator(TypeOperatorKind.Pow, namedType, type, retType, opDeclaration);
                    operators.Add(op);
                    break;
                }
                case "unm":
                {
                    var retType = searchContext.Infer(operatorSyntax.ReturnType);
                    var opDeclaration = new LuaSymbol(
                        string.Empty,
                        retType,
                        new TypeOpInfo(new(operatorSyntax)));
                    var op = new UnaryOperator(TypeOperatorKind.Unm, namedType, retType, opDeclaration);
                    operators.Add(op);
                    break;
                }
                case "idiv":
                {
                    var type = searchContext.Infer(operatorSyntax.Types.FirstOrDefault());
                    var retType = searchContext.Infer(operatorSyntax.ReturnType);
                    var opDeclaration = new LuaSymbol(
                        string.Empty,
                        retType,
                        new TypeOpInfo(new(operatorSyntax)));
                    var op = new BinaryOperator(TypeOperatorKind.Idiv, namedType, type, retType, opDeclaration);
                    operators.Add(op);
                    break;
                }
                case "band":
                {
                    var type = searchContext.Infer(operatorSyntax.Types.FirstOrDefault());
                    var retType = searchContext.Infer(operatorSyntax.ReturnType);
                    var opDeclaration = new LuaSymbol(
                        string.Empty,
                        retType,
                        new TypeOpInfo(new(operatorSyntax)));
                    var op = new BinaryOperator(TypeOperatorKind.Band, namedType, type, retType, opDeclaration);
                    operators.Add(op);
                    break;
                }
                case "bor":
                {
                    var type = searchContext.Infer(operatorSyntax.Types.FirstOrDefault());
                    var retType = searchContext.Infer(operatorSyntax.ReturnType);
                    var opDeclaration = new LuaSymbol(
                        string.Empty,
                        retType,
                        new TypeOpInfo(new(operatorSyntax)));
                    var op = new BinaryOperator(TypeOperatorKind.Bor, namedType, type, retType, opDeclaration);
                    operators.Add(op);
                    break;
                }
                case "bxor":
                {
                    var type = searchContext.Infer(operatorSyntax.Types.FirstOrDefault());
                    var retType = searchContext.Infer(operatorSyntax.ReturnType);
                    var opDeclaration = new LuaSymbol(
                        string.Empty,
                        retType,
                        new TypeOpInfo(new(operatorSyntax)));
                    var op = new BinaryOperator(TypeOperatorKind.Bxor, namedType, type, retType, opDeclaration);
                    operators.Add(op);
                    break;
                }
                case "bnot":
                {
                    var retType = searchContext.Infer(operatorSyntax.ReturnType);
                    var opDeclaration = new LuaSymbol(
                        string.Empty,
                        retType,
                        new TypeOpInfo(new(operatorSyntax)));
                    var op = new UnaryOperator(TypeOperatorKind.Bnot, namedType, retType, opDeclaration);
                    operators.Add(op);
                    break;
                }
                case "shl":
                {
                    var type = searchContext.Infer(operatorSyntax.Types.FirstOrDefault());
                    var retType = searchContext.Infer(operatorSyntax.ReturnType);
                    var opDeclaration = new LuaSymbol(
                        string.Empty,
                        retType,
                        new TypeOpInfo(new(operatorSyntax)));
                    var op = new BinaryOperator(TypeOperatorKind.Shl, namedType, type, retType, opDeclaration);
                    operators.Add(op);
                    break;
                }
                case "shr":
                {
                    var type = searchContext.Infer(operatorSyntax.Types.FirstOrDefault());
                    var retType = searchContext.Infer(operatorSyntax.ReturnType);
                    var opDeclaration = new LuaSymbol(
                        string.Empty,
                        retType,
                        new TypeOpInfo(new(operatorSyntax)));
                    var op = new BinaryOperator(TypeOperatorKind.Shr, namedType, type, retType, opDeclaration);
                    operators.Add(op);
                    break;
                }
                case "concat":
                {
                    var type = searchContext.Infer(operatorSyntax.Types.FirstOrDefault());
                    var retType = searchContext.Infer(operatorSyntax.ReturnType);
                    var opDeclaration = new LuaSymbol(
                        string.Empty,
                        retType,
                        new TypeOpInfo(new(operatorSyntax)));
                    var op = new BinaryOperator(TypeOperatorKind.Concat, namedType, type, retType, opDeclaration);
                    operators.Add(op);
                    break;
                }
                case "len":
                {
                    var retType = searchContext.Infer(operatorSyntax.ReturnType);
                    var opDeclaration = new LuaSymbol(
                        string.Empty,
                        retType,
                        new TypeOpInfo(new(operatorSyntax)));
                    var op = new UnaryOperator(TypeOperatorKind.Len, namedType, retType, opDeclaration);
                    operators.Add(op);
                    break;
                }
                case "eq":
                {
                    var type = searchContext.Infer(operatorSyntax.Types.FirstOrDefault());
                    var opDeclaration = new LuaSymbol(
                        string.Empty,
                        type,
                        new TypeOpInfo(new(operatorSyntax)));
                    var op = new BinaryOperator(TypeOperatorKind.Eq, namedType, type, Builtin.Boolean, opDeclaration);
                    operators.Add(op);
                    break;
                }
                case "lt":
                {
                    var type = searchContext.Infer(operatorSyntax.Types.FirstOrDefault());
                    var opDeclaration = new LuaSymbol(
                        string.Empty,
                        type,
                        new TypeOpInfo(new(operatorSyntax)));
                    var op = new BinaryOperator(TypeOperatorKind.Lt, namedType, type, Builtin.Boolean, opDeclaration);
                    operators.Add(op);
                    break;
                }
                case "le":
                {
                    var type = searchContext.Infer(operatorSyntax.Types.FirstOrDefault());
                    var opDeclaration = new LuaSymbol(
                        string.Empty,
                        type,
                        new TypeOpInfo(new(operatorSyntax)));
                    var op = new BinaryOperator(TypeOperatorKind.Le, namedType, type, Builtin.Boolean, opDeclaration);
                    operators.Add(op);
                    break;
                }
            }
        }

        if (operators.Count > 0)
        {
            declarationContext.TypeManager.AddOperators(namedType, operators);
        }

        var overloads = new List<TypeInfo.OverloadStub>();
        foreach (var overloadSyntax in typeTag.NextOfType<LuaDocTagOverloadSyntax>())
        {
            var overloadType = searchContext.Infer(overloadSyntax.TypeFunc);
            if (overloadType is LuaMethodType methodType)
            {
                var overload = new TypeInfo.OverloadStub(
                    DocumentId,
                    methodType
                );
                overloads.Add(overload);
            }
        }

        if (overloads.Count > 0)
        {
            declarationContext.TypeManager.AddOverloads(namedType, overloads);
        }
    }
}
