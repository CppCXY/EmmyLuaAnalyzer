using EmmyLua.CodeAnalysis.Compilation.Semantic;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.LanguageServer.Util;

namespace EmmyLua.LanguageServer.DocumentRender;

public class EmmyAnnotatorBuilder
{
    public List<EmmyAnnotatorResponse> Build(SemanticModel semanticModel)
    {
        var document = semanticModel.Document;
        var context = semanticModel.Context;
        var globalAnnotator = new EmmyAnnotatorResponse(semanticModel.Document.Uri, EmmyAnnotatorType.Global);
        var paramAnnotator = new EmmyAnnotatorResponse(semanticModel.Document.Uri, EmmyAnnotatorType.Param);
        var upvalueAnnotator = new EmmyAnnotatorResponse(semanticModel.Document.Uri, EmmyAnnotatorType.Upvalue);
        var responses = new List<EmmyAnnotatorResponse>() {globalAnnotator, paramAnnotator, upvalueAnnotator};
        foreach (var node in semanticModel.Document.SyntaxTree.SyntaxRoot.Descendants)
        {
            switch (node)
            {
                case LuaParamDefSyntax {Name: { } paramName}:
                {
                    paramAnnotator.Ranges.Add(new RenderRange(paramName.Range.ToLspRange(document)));
                    break;
                }
                case LuaNameExprSyntax nameExpr:
                {
                    if (nameExpr.Name is {Text: not "self"} nameToken)
                    {
                        var declaration = context.FindDeclaration(nameExpr) as LuaSymbol;
                        if (declaration is null || declaration.IsGlobal)
                        {
                            globalAnnotator.Ranges.Add(new RenderRange(nameToken.Range.ToLspRange(document)));
                        }
                        else if (declaration is {Info: ParamInfo})
                        {
                            paramAnnotator.Ranges.Add(new RenderRange(nameToken.Range.ToLspRange(document)));
                        }
                        else if (context.IsUpValue(nameExpr, declaration))
                        {
                            upvalueAnnotator.Ranges.Add(new RenderRange(nameToken.Range.ToLspRange(document)));
                        }
                    }

                    break;
                }
            }
        }
        
        return responses;
    }
}