using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Type;
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
                    var type = searchContext.Infer(operatorSyntax.Types.FirstOrDefault());
                    var retType = searchContext.Infer(operatorSyntax.ReturnType);
                    var opDeclaration = new LuaDeclaration(
                        string.Empty,
                        new TypeOpInfo(
                            new(operatorSyntax),
                            retType
                        ));
                    var op = new BinaryOperator(TypeOperatorKind.Add, namedType, type, retType, opDeclaration);
                    declarationContext.Db.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "sub":
                {
                    var type = searchContext.Infer(operatorSyntax.Types.FirstOrDefault());
                    var retType = searchContext.Infer(operatorSyntax.ReturnType);
                    var opDeclaration = new LuaDeclaration(
                        string.Empty,
                        new TypeOpInfo(
                            new(operatorSyntax),
                            retType
                        ));
                    var op = new BinaryOperator(TypeOperatorKind.Sub, namedType, type, retType, opDeclaration);
                    declarationContext.Db.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "mul":
                {
                    var type = searchContext.Infer(operatorSyntax.Types.FirstOrDefault());
                    var retType = searchContext.Infer(operatorSyntax.ReturnType);
                    var opDeclaration = new LuaDeclaration(
                        string.Empty,
                        new TypeOpInfo(
                            new(operatorSyntax),
                            retType
                        ));
                    var op = new BinaryOperator(TypeOperatorKind.Mul, namedType, type, retType, opDeclaration);
                    declarationContext.Db.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "div":
                {
                    var type = searchContext.Infer(operatorSyntax.Types.FirstOrDefault());
                    var retType = searchContext.Infer(operatorSyntax.ReturnType);
                    var opDeclaration = new LuaDeclaration(
                        string.Empty,
                        new TypeOpInfo(
                            new(operatorSyntax),
                            retType
                        ));
                    var op = new BinaryOperator(TypeOperatorKind.Div, namedType, type, retType, opDeclaration);
                    declarationContext.Db.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "mod":
                {
                    var type = searchContext.Infer(operatorSyntax.Types.FirstOrDefault());
                    var retType = searchContext.Infer(operatorSyntax.ReturnType);
                    var opDeclaration = new LuaDeclaration(
                        string.Empty,
                        new TypeOpInfo(
                            new(operatorSyntax),
                            retType
                        ));
                    var op = new BinaryOperator(TypeOperatorKind.Mod, namedType, type, retType, opDeclaration);
                    declarationContext.Db.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "pow":
                {
                    var type = searchContext.Infer(operatorSyntax.Types.FirstOrDefault());
                    var retType = searchContext.Infer(operatorSyntax.ReturnType);
                    var opDeclaration = new LuaDeclaration(
                        string.Empty,
                        new TypeOpInfo(
                            new(operatorSyntax),
                            retType
                        ));
                    var op = new BinaryOperator(TypeOperatorKind.Pow, namedType, type, retType, opDeclaration);
                    declarationContext.Db.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "unm":
                {
                    var retType = searchContext.Infer(operatorSyntax.ReturnType);
                    var opDeclaration = new LuaDeclaration(
                        string.Empty,
                        new TypeOpInfo(
                            new(operatorSyntax),
                            retType
                        ));
                    var op = new UnaryOperator(TypeOperatorKind.Unm, namedType, retType, opDeclaration);
                    declarationContext.Db.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "idiv":
                {
                    var type = searchContext.Infer(operatorSyntax.Types.FirstOrDefault());
                    var retType = searchContext.Infer(operatorSyntax.ReturnType);
                    var opDeclaration = new LuaDeclaration(
                        string.Empty,
                        new TypeOpInfo(
                            new(operatorSyntax),
                            retType
                        ));
                    var op = new BinaryOperator(TypeOperatorKind.Idiv, namedType, type, retType, opDeclaration);
                    declarationContext.Db.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "band":
                {
                    var type = searchContext.Infer(operatorSyntax.Types.FirstOrDefault());
                    var retType = searchContext.Infer(operatorSyntax.ReturnType);
                    var opDeclaration = new LuaDeclaration(
                        string.Empty,
                        new TypeOpInfo(
                            new(operatorSyntax),
                            retType
                        ));
                    var op = new BinaryOperator(TypeOperatorKind.Band, namedType, type, retType, opDeclaration);
                    declarationContext.Db.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "bor":
                {
                    var type = searchContext.Infer(operatorSyntax.Types.FirstOrDefault());
                    var retType = searchContext.Infer(operatorSyntax.ReturnType);
                    var opDeclaration = new LuaDeclaration(
                        string.Empty,
                        new TypeOpInfo(
                            new(operatorSyntax),
                            retType
                        ));
                    var op = new BinaryOperator(TypeOperatorKind.Bor, namedType, type, retType, opDeclaration);
                    declarationContext.Db.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "bxor":
                {
                    var type = searchContext.Infer(operatorSyntax.Types.FirstOrDefault());
                    var retType = searchContext.Infer(operatorSyntax.ReturnType);
                    var opDeclaration = new LuaDeclaration(
                        string.Empty,
                        new TypeOpInfo(
                            new(operatorSyntax),
                            retType
                        ));
                    var op = new BinaryOperator(TypeOperatorKind.Bxor, namedType, type, retType, opDeclaration);
                    declarationContext.Db.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "bnot":
                {
                    var retType = searchContext.Infer(operatorSyntax.ReturnType);
                    var opDeclaration = new LuaDeclaration(
                        string.Empty,
                        new TypeOpInfo(
                            new(operatorSyntax),
                            retType
                        ));
                    var op = new UnaryOperator(TypeOperatorKind.Bnot, namedType, retType, opDeclaration);
                    declarationContext.Db.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "shl":
                {
                    var type = searchContext.Infer(operatorSyntax.Types.FirstOrDefault());
                    var retType = searchContext.Infer(operatorSyntax.ReturnType);
                    var opDeclaration = new LuaDeclaration(
                        string.Empty,
                        new TypeOpInfo(
                            new(operatorSyntax),
                            retType
                        ));
                    var op = new BinaryOperator(TypeOperatorKind.Shl, namedType, type, retType, opDeclaration);
                    declarationContext.Db.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "shr":
                {
                    var type = searchContext.Infer(operatorSyntax.Types.FirstOrDefault());
                    var retType = searchContext.Infer(operatorSyntax.ReturnType);
                    var opDeclaration = new LuaDeclaration(
                        string.Empty,
                        new TypeOpInfo(
                            new(operatorSyntax),
                            retType
                        ));
                    var op = new BinaryOperator(TypeOperatorKind.Shr, namedType, type, retType, opDeclaration);
                    declarationContext.Db.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "concat":
                {
                    var type = searchContext.Infer(operatorSyntax.Types.FirstOrDefault());
                    var retType = searchContext.Infer(operatorSyntax.ReturnType);
                    var opDeclaration = new LuaDeclaration(
                        string.Empty,
                        new TypeOpInfo(
                            new(operatorSyntax),
                            retType
                        ));
                    var op = new BinaryOperator(TypeOperatorKind.Concat, namedType, type, retType, opDeclaration);
                    declarationContext.Db.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "len":
                {
                    var retType = searchContext.Infer(operatorSyntax.ReturnType);
                    var opDeclaration = new LuaDeclaration(
                        string.Empty,
                        new TypeOpInfo(
                            new(operatorSyntax),
                            retType
                        ));
                    var op = new UnaryOperator(TypeOperatorKind.Len, namedType, retType, opDeclaration);
                    declarationContext.Db.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "eq":
                {
                    var type = searchContext.Infer(operatorSyntax.Types.FirstOrDefault());
                    var opDeclaration = new LuaDeclaration(
                        string.Empty,
                        new TypeOpInfo(
                            new(operatorSyntax),
                            type
                        ));
                    var op = new BinaryOperator(TypeOperatorKind.Eq, namedType, type, Builtin.Boolean, opDeclaration);
                    declarationContext.Db.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "lt":
                {
                    var type = searchContext.Infer(operatorSyntax.Types.FirstOrDefault());
                    var opDeclaration = new LuaDeclaration(
                        string.Empty,
                        new TypeOpInfo(
                            new(operatorSyntax),
                            type
                        ));
                    var op = new BinaryOperator(TypeOperatorKind.Lt, namedType, type, Builtin.Boolean, opDeclaration);
                    declarationContext.Db.AddTypeOperator(DocumentId, op);
                    break;
                }
                case "le":
                {
                    var type = searchContext.Infer(operatorSyntax.Types.FirstOrDefault());
                    var opDeclaration = new LuaDeclaration(
                        string.Empty,
                        new TypeOpInfo(
                            new(operatorSyntax),
                            type
                        ));
                    var op = new BinaryOperator(TypeOperatorKind.Le, namedType, type, Builtin.Boolean, opDeclaration);
                    declarationContext.Db.AddTypeOperator(DocumentId, op);
                    break;
                }
            }
        }

        foreach (var overloadSyntax in typeTag.NextOfType<LuaDocTagOverloadSyntax>())
        {
            var overloadType = searchContext.Infer(overloadSyntax.TypeFunc);
            if (overloadType is LuaMethodType methodType)
            {
                declarationContext.Db.AddTypeOverload(DocumentId, namedType.Name, methodType);
            }
        }
    }
}
