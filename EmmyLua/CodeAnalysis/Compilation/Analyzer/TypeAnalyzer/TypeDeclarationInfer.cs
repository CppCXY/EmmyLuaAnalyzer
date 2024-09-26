// using System.Diagnostics;
// using EmmyLua.CodeAnalysis.Compilation.Search;
// using EmmyLua.CodeAnalysis.Compilation.Symbol;
// using EmmyLua.CodeAnalysis.Compilation.Type;
// using EmmyLua.CodeAnalysis.Compilation.Type.Types;
// using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
//
// namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.TypeAnalyzer;
//
// public static class TypeDeclarationInfer
// {
//     public static LuaType InferType(LuaDocTypeSyntax type)
//     {
//         switch (type)
//         {
//             case LuaDocTableTypeSyntax tableType:
//                 return InferTableType(tableType);
//             case LuaDocArrayTypeSyntax arrayType:
//                 return InferArrayType(arrayType);
//             case LuaDocUnionTypeSyntax unionType:
//                 return InferUnionType(unionType);
//             case LuaDocLiteralTypeSyntax literalType:
//                 return InferLiteralType(literalType);
//             case LuaDocFuncTypeSyntax funcType:
//                 return InferFuncType(funcType);
//             case LuaDocNameTypeSyntax nameType:
//                 return InferNameType(nameType);
//             case LuaDocParenTypeSyntax parenType:
//                 return InferParenType(parenType);
//             case LuaDocTupleTypeSyntax tupleType:
//                 return InferTupleType(tupleType);
//             case LuaDocGenericTypeSyntax genericType:
//                 return InferGenericType(genericType);
//             case LuaDocVariadicTypeSyntax genericVarargType:
//                 return InferVariadicType(genericVarargType);
//             case LuaDocExpandTypeSyntax expandType:
//                 return InferExpandType(expandType);
//             // case LuaDocAggregateTypeSyntax aggregateType:
//             //     return InferAggregateType(aggregateType, context);
//             case LuaDocTemplateTypeSyntax templateType:
//                 return InferTemplateType(templateType);
//             default:
//                 throw new UnreachableException();
//         }
//     }
//
//     private static LuaType InferTableType(LuaDocTableTypeSyntax tableType)
//     {
//         return new LuaElementRef(tableType.UniqueId);
//     }
//
//     private static LuaType InferArrayType(LuaDocArrayTypeSyntax arrayType, SearchContext context)
//     {
//         var baseTy = context.Infer(arrayType.BaseType);
//         return new LuaArrayType(baseTy);
//     }
//
//     private static LuaType InferUnionType(LuaDocUnionTypeSyntax unionType, SearchContext context)
//     {
//         var types = unionType.TypeList.Select(context.Infer).ToList();
//         if (types.Count == 1)
//         {
//             return types[0];
//         }
//
//         return new LuaUnionType(types);
//     }
//
//     private static LuaType InferLiteralType(LuaDocLiteralTypeSyntax literalType)
//     {
//         if (literalType.Integer != null)
//         {
//             return new LuaIntegerLiteralType(literalType.Integer.Value);
//         }
//
//         if (literalType.String != null)
//         {
//             return new LuaStringLiteralType(literalType.String.Value);
//         }
//
//         return Builtin.Unknown;
//     }
//
//     private static LuaType InferFuncType(LuaDocFuncTypeSyntax funcType, SearchContext context)
//     {
//         var typedParameters = new List<LuaSymbol>();
//         foreach (var typedParam in funcType.ParamList)
//         {
//             if (typedParam is { Name: { } name, Nullable: { } nullable })
//             {
//                 var type = context.Infer(typedParam.Type);
//                 if (nullable)
//                 {
//                     type = type.Union(Builtin.Nil, context);
//                 }
//
//                 var paramDeclaration = new LuaSymbol(
//                     name.RepresentText,
//                     type,
//                     new ParamInfo(new(typedParam), false));
//                 typedParameters.Add(paramDeclaration);
//             }
//             else if (typedParam is { VarArgs: { } varArgs })
//             {
//                 var paramDeclaration = new LuaSymbol("...",
//                     context.Infer(typedParam.Type),
//                     new ParamInfo(new(typedParam), true));
//                 typedParameters.Add(paramDeclaration);
//             }
//         }
//
//         var returnTypes = funcType.ReturnType.Select(context.Infer).ToList();
//         LuaType returnType = Builtin.Unknown;
//         if (returnTypes.Count == 1)
//         {
//             returnType = returnTypes[0];
//         }
//         else if (returnTypes.Count > 1)
//         {
//             returnType = new LuaMultiReturnType(returnTypes);
//         }
//
//         return new LuaMethodType(returnType, typedParameters, false);
//     }
//
//     private static LuaType InferNameType(LuaDocNameTypeSyntax nameType, SearchContext context)
//     {
//         if (nameType.Name is { RepresentText: { } name })
//         {
//             return new LuaNamedType(nameType.DocumentId, name);
//         }
//
//         return Builtin.Unknown;
//     }
//
//     private static LuaType InferParenType(LuaDocParenTypeSyntax parenType, SearchContext context)
//     {
//         return parenType.Type != null
//             ? InferType(parenType.Type, context)
//             : Builtin.Unknown;
//     }
//
//     private static LuaType InferTupleType(LuaDocTupleTypeSyntax tupleType, SearchContext context)
//     {
//         var tupleMembers = tupleType.TypeList
//             .Select((it, i) =>
//                 // lua from 1 start
//                 context.Infer(it)
//             )
//             .ToList();
//         return new LuaTupleType(tupleMembers);
//     }
//
//     private static LuaType InferGenericType(LuaDocGenericTypeSyntax genericType, SearchContext context)
//     {
//         var typeArgs = genericType.GenericArgs.Select(context.Infer).ToList();
//         if (genericType is { Name: { } name })
//         {
//             var nameText = name.RepresentText;
//             if (nameText == "instance" && typeArgs.FirstOrDefault() is { } firstType)
//             {
//                 return new InstanceType(firstType);
//             }
//
//             return new LuaGenericType(genericType.DocumentId, name.RepresentText, typeArgs);
//         }
//
//         return Builtin.Unknown;
//     }
//
//     private static LuaType InferVariadicType(LuaDocVariadicTypeSyntax variadicType, SearchContext context)
//     {
//         if (variadicType is { Name: { } name })
//         {
//             return new LuaVariadicType(new LuaNamedType(variadicType.DocumentId, name.RepresentText));
//         }
//
//         return new LuaVariadicType(Builtin.Unknown);
//     }
//
//     private static LuaType InferExpandType(LuaDocExpandTypeSyntax expandType, SearchContext context)
//     {
//         if (expandType is { Name: { } name })
//         {
//             return new LuaExpandTplType(name.RepresentText);
//         }
//
//         return Builtin.Unknown;
//     }
//
//     // private static LuaType InferAggregateType(LuaDocAggregateTypeSyntax aggregateType, SearchContext context)
//     // {
//     //     // var declarations = aggregateType.TypeList
//     //     //     .Select((it, i) =>
//     //     //         new LuaSymbol(
//     //     //             string.Empty,
//     //     //             context.Infer(it),
//     //     //             new AggregateMemberInfo(new(it))
//     //     //         )
//     //     //     )
//     //     //     .ToList();
//     //     // return new LuaAggregateType(declarations);
//     //     return Builtin.Unknown;
//     // }
//
//     private static LuaType InferTemplateType(LuaDocTemplateTypeSyntax templateType, SearchContext context)
//     {
//         var prefixName = templateType.PrefixName?.RepresentText ?? string.Empty;
//         if (templateType.Name?.Name is { } name)
//         {
//             return new LuaStrTplType(prefixName, name);
//         }
//
//         return Builtin.Unknown;
//     }
// }
