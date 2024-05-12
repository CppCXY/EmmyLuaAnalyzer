using System.Text;
using EmmyLua.CodeAnalysis.Compilation.Semantic;
using EmmyLua.CodeAnalysis.Compilation.Semantic.Render;
using EmmyLua.CodeAnalysis.Compilation.Semantic.Render.Renderer;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Kind;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace EmmyLua.LanguageServer.SignatureHelper;

public class SignatureHelperBuilder
{
    private LuaRenderFeature RenderFeature { get; } = new(
        true,
        true,
        false,
        100
    );

    public SignatureHelp? Build(SemanticModel semanticModel, LuaSyntaxToken triggerToken, SignatureHelpParams request)
    {
        LuaCallExprSyntax callExpr = null!;
        LuaCallArgListSyntax callArgs = null!;
        if (!request.Context.IsRetrigger)
        {
            if (triggerToken.Parent is not LuaCallArgListSyntax callArgs1)
            {
                return null;
            }

            callArgs = callArgs1;
            if (callArgs.Parent is not LuaCallExprSyntax callExpr1)
            {
                return null;
            }

            callExpr = callExpr1;
        }
        else
        {
            var callExpr2 = triggerToken.Ancestors.OfType<LuaCallExprSyntax>().FirstOrDefault();
            if (callExpr2 is null)
            {
                return null;
            }

            callExpr = callExpr2;
            var callArgs2 = callExpr.ArgList;
            if (callArgs2 is null)
            {
                return null;
            }

            callArgs = callArgs2;
        }

        var parentType = semanticModel.Context.Infer(callExpr.PrefixExpr);
        var signatureInfos = new List<SignatureInformation>();
        var activeParameter = callArgs.ChildTokens(LuaTokenKind.TkComma)
            .Count(comma => comma.Position <= triggerToken.Position);

        var activeSignature = 0;
        var colonCall = callExpr.PrefixExpr is LuaIndexExprSyntax {IsColonIndex: true};

        TypeHelper.Each(parentType, type =>
        {
            switch (type)
            {
                case LuaMethodType luaMethod:
                {
                    var signatures = new List<LuaSignature>();
                    if (luaMethod is LuaGenericMethodType genericMethodType)
                    {
                        signatures = genericMethodType.GetInstantiatedSignatures(callExpr, callArgs.ArgList.ToList(),
                            semanticModel.Context);
                    }
                    else
                    {
                        signatures.Add(luaMethod.MainSignature);
                        if (luaMethod.Overloads is not null)
                        {
                            signatures.AddRange(luaMethod.Overloads);
                        }
                    }

                    ResolveSignature(
                        signatures,
                        activeParameter,
                        ref activeSignature,
                        signatureInfos,
                        colonCall,
                        luaMethod.ColonDefine,
                        semanticModel
                    );
                    break;
                }
            }
        });


        return new SignatureHelp()
        {
            ActiveParameter = activeParameter,
            ActiveSignature = activeSignature,
            Signatures = signatureInfos
        };
    }

    private void ResolveSignature(
        List<LuaSignature> signatures,
        int originActiveParameter,
        ref int activeSignature,
        List<SignatureInformation> signatureInfos,
        bool colonCall,
        bool colonDefine,
        SemanticModel semanticModel
    )
    {
        var maxActiveParameter = 0;
        for (var sigIndex = 0; sigIndex < signatures.Count; sigIndex++)
        {
            var signature = signatures[sigIndex];
            var activeParameter = originActiveParameter;
            var parameters = signature.Parameters;
            var parameterInfos = new List<ParameterInformation>();
            switch ((colonDefine, colonCall))
            {
                case (true, false):
                {
                    parameterInfos.Add(new ParameterInformation()
                    {
                        Label = "self",
                    });

                    break;
                }
                case (false, true):
                {
                    parameters = parameters.Skip(1).ToList();
                    break;
                }
            }

            foreach (var parameter in parameters)
            {
                var syntaxElement = parameter.Info.Ptr.ToNode(semanticModel.Context);
                if (syntaxElement is not null)
                {
                    parameterInfos.Add(new ParameterInformation()
                    {
                        Label = parameter.Name,
                        Documentation = new StringOrMarkupContent(new MarkupContent()
                        {
                            Kind = MarkupKind.Markdown,
                            Value = semanticModel.RenderSymbol(syntaxElement, RenderFeature)
                        })
                    });
                }
                else
                {
                    parameterInfos.Add(new ParameterInformation()
                    {
                        Label = parameter.Name,
                    });
                }
            }

            var returnType = signature.ReturnType;
            if (parameters.LastOrDefault() is {Name: "..."})
            {
                if (activeParameter >= parameterInfos.Count)
                {
                    activeParameter = parameterInfos.Count - 1;
                }
            }

            var sb = new StringBuilder();
            sb.Append('(');
            for (var i = 0; i < parameterInfos.Count; i++)
            {
                sb.Append(parameterInfos[i].Label);

                if (i < parameterInfos.Count - 1)
                {
                    sb.Append(", ");
                }
            }

            sb.Append(") -> ");
            sb.Append(semanticModel.RenderBuilder.RenderType(returnType, RenderFeature with {ShowTypeLink = false}));

            signatureInfos.Add(new SignatureInformation()
            {
                Label = sb.ToString(),
                Parameters = parameterInfos,
                ActiveParameter = activeParameter
            });

            if (activeParameter >= maxActiveParameter)
            {
                maxActiveParameter = activeParameter;
                activeSignature = sigIndex;
            }
        }
    }
}