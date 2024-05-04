using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Semantic;
using EmmyLua.CodeAnalysis.Compilation.Semantic.Render;
using EmmyLua.CodeAnalysis.Compilation.Semantic.Render.Renderer;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using LanguageServer.Util;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using InlayHintType = OmniSharp.Extensions.LanguageServer.Protocol.Models.InlayHint;

namespace LanguageServer.InlayHint;

public class InlayHintBuilder
{
    private LuaRenderFeature RenderFeature { get; } = new(
        false,
        false,
        true,
        100
    );
    
    public List<InlayHintType> Build(SemanticModel semanticModel, SourceRange range, InlayHintConfig config,
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
                    if (config.ParamHint)
                    {
                        CallExprHint(semanticModel, hints, callExpr, cancellationToken);
                    }

                    break;
                }
                case LuaClosureExprSyntax closureExpr:
                {
                    if (config.ParamHint)
                    {
                        ClosureExprHint(semanticModel, hints, closureExpr, cancellationToken);
                    }

                    break;
                }
                case LuaIndexExprSyntax indexExpr:
                {
                    if (config.IndexHint)
                    {
                        IndexExprHint(semanticModel, hints, indexExpr, cancellationToken);
                    }

                    break;
                }
                case LuaLocalNameSyntax localName:
                {
                    if (config.LocalHint)
                    {
                        LocalNameHint(semanticModel, hints, localName, cancellationToken);
                    }

                    break;
                }
                case LuaFuncStatSyntax funcStat:
                {
                    if (config.ParamHint)
                    {
                        OverrideHint(semanticModel, hints, funcStat, cancellationToken);
                    }

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
            var hasVarArg = parameters.LastOrDefault()?.Info is ParamInfo {IsVararg: true};
            var parameterCount = hasVarArg ? (parameters.Count - 1) : parameters.Count;
            var varCount = 0;
            for (var i = 0; i < args.Count; i++)
            {
                var arg = args[i];
                if (i < parameterCount)
                {
                    var parameter = parameters[i];
                    var document = semanticModel.Compilation.Workspace.GetDocument(parameter.Info.Ptr.DocumentId);
                    if (document is not null && parameter.Info.Ptr.ToNode(document) is { } node)
                    {
                        hints.Add(new InlayHintType()
                        {
                            Position = arg.Position.ToLspPosition(semanticModel.Document),
                            Label = new StringOrInlayHintLabelParts(new[]
                            {
                                new InlayHintLabelPart()
                                {
                                    Value = $"{parameter.Name}:",
                                    Location = node.Range.ToLspLocation(document)
                                }
                            }),
                            Kind = InlayHintKind.Parameter,
                            PaddingRight = true
                        });
                    }
                    else
                    {
                        hints.Add(new InlayHintType()
                        {
                            Position = arg.Position.ToLspPosition(semanticModel.Document),
                            Label = new StringOrInlayHintLabelParts($"{parameter.Name}:"),
                            Kind = InlayHintKind.Parameter,
                            PaddingRight = true
                        });
                    }
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

        if (callExpr.PrefixExpr is { } prefixExpr)
        {
            var luaDeclaration = semanticModel.DeclarationTree.FindDeclaration(prefixExpr, semanticModel.Context);
            if (luaDeclaration?.Info is MethodInfo info)
            {
                var funcStat = info.FuncStatPtr.ToNode(semanticModel.Context);

                if (funcStat is {Comments: { } comments})
                {
                    foreach (var comment in comments)
                    {
                        if (comment.IsAsync)
                        {
                            hints.Add(new InlayHintType()
                            {
                                Position = callExpr.Range.StartOffset.ToLspPosition(semanticModel.Document),
                                Label = new StringOrInlayHintLabelParts("await "),
                                Kind = InlayHintKind.Type,
                                PaddingRight = true
                            });
                        }
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
                parameterDic.TryAdd(parameter.Name, parameter.Info.DeclarationType);
            }

            var parameters = closureExpr.ParamList?
                .Params.Select(it => it.Name).ToList() ?? [];

            foreach (var parameter in parameters)
            {
                if (parameter is {RepresentText: { } name})
                {
                    var type = parameterDic.GetValueOrDefault(name);
                    if (type is not null && !type.Equals(Builtin.Unknown))
                    {
                        hints.Add(new InlayHintType()
                        {
                            Position = parameter.Range.EndOffset.ToLspPosition(semanticModel.Document),
                            Label = new StringOrInlayHintLabelParts(
                                $":{semanticModel.RenderBuilder.RenderType(type, RenderFeature)}"),
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
        if (indexExpr is {PrefixExpr: { } prefixExpr, KeyElement: { } keyElement})
        {
            if (document.GetLine(prefixExpr.Range.EndOffset) != document.GetLine(keyElement.Range.StartOffset))
            {
                var type = semanticModel.Context.Infer(prefixExpr);
                hints.Add(new InlayHintType()
                {
                    Position = prefixExpr.Range.EndOffset.ToLspPosition(semanticModel.Document),
                    Label = new StringOrInlayHintLabelParts(
                        $"// {semanticModel.RenderBuilder.RenderType(type, RenderFeature)}"),
                    Kind = InlayHintKind.Type,
                    PaddingLeft = true
                });
            }
        }
    }

    private void LocalNameHint(SemanticModel semanticModel, List<InlayHintType> hints, LuaLocalNameSyntax localName,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        var type = semanticModel.Context.Infer(localName);
        hints.Add(new InlayHintType()
        {
            Position = localName.Range.EndOffset.ToLspPosition(semanticModel.Document),
            Label = new StringOrInlayHintLabelParts(
                $":{semanticModel.RenderBuilder.RenderType(type, RenderFeature)}"),
            Kind = InlayHintKind.Type,
            PaddingLeft = true
        });
    }

    private void OverrideHint(SemanticModel semanticModel, List<InlayHintType> hints, LuaFuncStatSyntax funcStat,
        CancellationToken cancellationToken)
    {
        if (cancellationToken.IsCancellationRequested)
        {
            return;
        }

        if (funcStat is
            {
                IsMethod: true, IndexExpr: {PrefixExpr: { } prefixExpr, Name: { } name},
                ClosureExpr: {ParamList: { } paramList}
            })
        {
            var prefixType = semanticModel.Context.Infer(prefixExpr);
            var superMethod = semanticModel.Context.FindSuperMember(prefixType, name).FirstOrDefault();
            if (superMethod?.Info is { } info)
            {
                var document = semanticModel.Document;
                var parentDocument = semanticModel.Compilation.Workspace.GetDocument(info.Ptr.DocumentId);
                var location = new Location();
                if (parentDocument is not null)
                {
                    location = info.Ptr.ToNode(parentDocument)!.Range.ToLspLocation(parentDocument);
                    if (info.Ptr.ToNode(semanticModel.Context) is LuaIndexExprSyntax {KeyElement: { } keyElement})
                    {
                        location = keyElement.Range.ToLspLocation(parentDocument);
                    }
                }

                hints.Add(new InlayHintType()
                {
                    Position = paramList.Range.EndOffset.ToLspPosition(document),
                    Label = new StringOrInlayHintLabelParts(new[]
                    {
                        new InlayHintLabelPart()
                        {
                            Value = "override",
                            Location = location
                        }
                    }),
                    Kind = InlayHintKind.Parameter,
                    PaddingLeft = true,
                });
            }
        }
    }
}