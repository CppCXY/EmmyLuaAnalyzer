using EmmyLua.CodeAnalysis.Compilation.Semantic;
using EmmyLua.CodeAnalysis.Compilation.Semantic.Render;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using LanguageServer.ExtensionUtil;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using InlayHintType = OmniSharp.Extensions.LanguageServer.Protocol.Models.InlayHint;

namespace LanguageServer.InlayHint;

public class InlayHintBuilder
{
    public List<InlayHintType> Build(SemanticModel semanticModel, SourceRange range,
        CancellationToken cancellationToken)
    {
        var syntaxTree = semanticModel.DeclarationTree.SyntaxTree;
        var hints = new List<InlayHintType>();
        var sourceBlock = syntaxTree.SyntaxRoot.Block;
        if (sourceBlock is null)
        {
            return hints;
        }

        foreach (var node in sourceBlock.DescendantsInRange(range))
        {
            switch (node)
            {
                case LuaCallExprSyntax callExpr:
                {
                    CallExprHint(semanticModel, hints, callExpr, cancellationToken);
                    break;
                }
                case LuaClosureExprSyntax closureExpr:
                {
                    ClosureExprHint(semanticModel, hints, closureExpr, cancellationToken);
                    break;
                }
                case LuaIndexExprSyntax indexExpr:
                {
                    IndexExprHint(semanticModel, hints, indexExpr, cancellationToken);
                    break;
                }
            }
        }

        return hints;
    }

    private void CallExprHint(SemanticModel semanticModel, List<InlayHintType> hints, LuaCallExprSyntax callExpr,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        if (semanticModel.Context.Infer(callExpr.PrefixExpr) is LuaMethodType method)
        {
            var args = callExpr.ArgList?.ArgList.ToList() ?? [];
            var perfectSignature = method.FindPerfectMatchSignature(callExpr, args, semanticModel.Context);

            var colonCall = false;
            if (callExpr.PrefixExpr is LuaIndexExprSyntax indexExpr)
            {
                colonCall = indexExpr.IsColonIndex;
            }

            var colonDefine = method.ColonDefine;
            var skipParam = 0;
            switch ((colonCall, colonDefine))
            {
                case (true, false):
                {
                    skipParam = 1;
                    break;
                }
                case (false, true):
                {
                    if (args.Count >= 1)
                    {
                        hints.Add(new InlayHintType()
                        {
                            Position = args[0].Position.ToLspPosition(semanticModel.Document),
                            Label = new StringOrInlayHintLabelParts($"self:"),
                            Kind = InlayHintKind.Parameter,
                            PaddingRight = true
                        });
                    }

                    args = args.Skip(1).ToList();
                    break;
                }
            }

            var parameters = perfectSignature.Parameters.Skip(skipParam).ToList();
            var hasVarArg = parameters.LastOrDefault()?.IsVararg ?? false;
            var parameterCount = hasVarArg ? (parameters.Count - 1) : parameters.Count;
            var varCount = 0;
            for (var i = 0; i < args.Count; i++)
            {
                var arg = args[i];
                if (i < parameterCount)
                {
                    var parameter = parameters[i];
                    hints.Add(new InlayHintType()
                    {
                        Position = arg.Position.ToLspPosition(semanticModel.Document),
                        Label = new StringOrInlayHintLabelParts($"{parameter.Name}:"),
                        Kind = InlayHintKind.Parameter,
                        PaddingRight = true
                    });
                }
                else
                {
                    if (hasVarArg)
                    {
                        hints.Add(new InlayHintType()
                        {
                            Position = arg.Position.ToLspPosition(semanticModel.Document),
                            Label = new StringOrInlayHintLabelParts($"var{varCount}:"),
                            Kind = InlayHintKind.Parameter,
                            PaddingLeft = true
                        });

                        varCount++;
                    }
                }
            }
        }
    }

    private void ClosureExprHint(SemanticModel semanticModel, List<InlayHintType> hints,
        LuaClosureExprSyntax closureExpr,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        if (semanticModel.Context.Infer(closureExpr) is LuaMethodType method)
        {
            var mainSignature = method.MainSignature;
            var parameterDic = new Dictionary<string, LuaType?>();
            foreach (var parameter in mainSignature.Parameters)
            {
                parameterDic.TryAdd(parameter.Name, parameter.DeclarationType);
            }
            var parameters = closureExpr.ParamList?
                .Params.Select(it => it.Name).ToList() ?? [];

            foreach (var parameter in parameters)
            {
                if(parameter is { RepresentText: {} name })
                {
                    var type = parameterDic.GetValueOrDefault(name);
                    if (type is not null && !type.Equals(Builtin.Unknown))
                    {
                        hints.Add(new InlayHintType()
                        {
                            Position = parameter.Range.EndOffset.ToLspPosition(semanticModel.Document),
                            Label = new StringOrInlayHintLabelParts(
                                $":{LuaTypeRender.RenderType(type, semanticModel.Context)}"),
                            Kind = InlayHintKind.Parameter,
                            PaddingLeft = true
                        });
                    }
                }
            }
        }
    }
    
    private void IndexExprHint(SemanticModel semanticModel, List<InlayHintType> hints, LuaIndexExprSyntax indexExpr,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        var document = semanticModel.Document;
        if (indexExpr is { PrefixExpr: {} prefixExpr, KeyElement: {} keyElement })
        {
            if (document.GetLine(prefixExpr.Range.EndOffset) != document.GetLine(keyElement.Range.StartOffset))
            {
                var type = semanticModel.Context.Infer(prefixExpr);
                hints.Add(new InlayHintType()
                {
                    Position = prefixExpr.Range.EndOffset.ToLspPosition(semanticModel.Document),
                    Label = new StringOrInlayHintLabelParts(
                        $"// {LuaTypeRender.RenderType(type, semanticModel.Context)}"),
                    Kind = InlayHintKind.Type,
                    PaddingLeft = true
                });
            }
        }
        
    }
}