using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Semantic;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using LanguageServer.Util;

namespace LanguageServer.DocumentRender;

public class EmmyAnnotatorBuilder
{
    public List<EmmyAnnotatorResponse> Build(SemanticModel semanticModel)
    {
        var document = semanticModel.Document;
        var declarationTree = semanticModel.DeclarationTree;
        var context = semanticModel.Context;
        var globalAnnotator = new EmmyAnnotatorResponse(semanticModel.Document.Uri, EmmyAnnotatorType.Global);
        var paramAnnotator = new EmmyAnnotatorResponse(semanticModel.Document.Uri, EmmyAnnotatorType.Param);
        var upvalueAnnotator = new EmmyAnnotatorResponse(semanticModel.Document.Uri, EmmyAnnotatorType.Upvalue);
        var responses = new List<EmmyAnnotatorResponse>() { globalAnnotator, paramAnnotator, upvalueAnnotator };
        foreach (var node in semanticModel.Document.SyntaxTree.SyntaxRoot.Descendants)
        {
            switch (node)
            {
                case LuaParamDefSyntax { Name: { } paramName }:
                {
                    paramAnnotator.ranges.Add(new RenderRange(paramName.Range.ToLspRange(document)));
                    break;
                }
                case LuaNameExprSyntax nameExpr:
                {
                    if (nameExpr.Name is { RepresentText: { } name2 } nameToken && name2 != "self")
                    {
                        var declaration = declarationTree.FindDeclaration(nameExpr, context);
                        if (declaration?.Feature == DeclarationFeature.Global)
                        {
                            globalAnnotator.ranges.Add(new RenderRange(nameToken.Range.ToLspRange(document)));
                        }
                        else if (declaration is ParameterLuaDeclaration)
                        {
                            paramAnnotator.ranges.Add(new RenderRange(nameToken.Range.ToLspRange(document)));
                        }
                        else if (declaration is not null && declarationTree.IsUpValue(nameExpr, declaration))
                        {
                            upvalueAnnotator.ranges.Add(new RenderRange(nameToken.Range.ToLspRange(document)));
                        }
                    }
                    
                    break;
                }
            }
        }


        return responses;
    }
}