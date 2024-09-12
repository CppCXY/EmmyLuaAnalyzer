using EmmyLua.CodeAnalysis.Compilation.Analyzer.ResolveAnalyzer;
using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Compilation.Type.TypeInfo;
using EmmyLua.CodeAnalysis.Compilation.Type.Types;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.TypeAnalyzer;

public class TypeAnalyzer(LuaCompilation compilation) : LuaAnalyzer(compilation, "Type")
{
    public override void Analyze(AnalyzeContext analyzeContext)
    {
        foreach (var document in analyzeContext.LuaDocuments)
        {
            var comments = document.SyntaxTree.SyntaxRoot.Descendants.OfType<LuaCommentSyntax>();
            foreach (var comment in comments)
            {
                AnalyzeComment(comment);
            }
        }
    }

    private void AnalyzeComment(LuaCommentSyntax commentSyntax)
    {
        foreach (var tagDoc in commentSyntax.DocList)
        {
            switch (tagDoc)
            {
                case LuaDocTagClassSyntax classSyntax:
                {
                    AnalyzeClass(classSyntax);
                    break;
                }
            }
        }

    }

    private void AnalyzeClass(LuaDocTagClassSyntax classSyntax)
    {
        if (classSyntax.Name?.RepresentText is not { } className)
        {
            return;
        }

        var classType = new LuaNamedType(classSyntax.DocumentId, className);
        var classTypeInfo = Compilation.TypeManager.FindTypeInfo(classType);
        if (classTypeInfo is null)
        {
            return;
        }


    }

    private void AnalyzeTypeSupers(LuaTypeInfo luaTypeInfo, LuaNamedType namedType,
        IEnumerable<LuaDocTypeSyntax> extendList)
    {
        foreach (var extend in extendList)
        {
            if (extend is LuaDocTableTypeSyntax { Body: { } body })
            {
                AnalyzeTagDocBody(luaTypeInfo, namedType, body);
            }
            else
            {
                var type = searchContext.Infer(extend);
                if (type is LuaNamedType superNamedType)
                {
                    luaTypeInfo.AddSuper(superNamedType);
                }
            }
        }
    }

    private void AnalyzeTypeGenericParam(
        LuaTypeInfo luaTypeInfo,
        LuaDocGenericDeclareListSyntax generic
    )
    {
        foreach (var param in generic.Params)
        {
            if (param is { Name: { } name })
            {
                var type = param.Type is not null ? new LuaTypeRef(LuaTypeId.Create(param.Type)) : null;
                var declaration = new LuaSymbol(
                    name.RepresentText,
                    type,
                    new GenericParamInfo(new(param)));
                luaTypeInfo.AddGenericParameter(declaration);
            }
        }
    }

    private void AnalyzeTagDocBody(LuaTypeInfo luaTypeInfo, LuaNamedType namedType, LuaDocBodySyntax docBody)
    {
        foreach (var field in docBody.FieldList)
        {
            if (field is { TypeField: { } typeField, Type: { } type4, UniqueId: { } id })
            {
                var unResolved = new UnResolvedDocOperator(
                    luaTypeInfo,
                    namedType,
                    TypeOperatorKind.Index,
                    id,
                    [LuaTypeId.Create(typeField), LuaTypeId.Create(type4)],
                    ResolveState.UnResolvedType
                );
                // declarationContext.AddUnResolved(unResolved);
                continue;
            }

            // if (AnalyzeDocDetailField(field) is { } fieldSymbol)
            // {
            //     luaTypeInfo.AddDeclaration(fieldSymbol);
            // }
        }
    }
}
