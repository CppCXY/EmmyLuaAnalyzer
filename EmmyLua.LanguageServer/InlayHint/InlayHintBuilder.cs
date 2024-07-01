using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Semantic;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.LanguageServer.Server.Render;
using EmmyLua.LanguageServer.Util;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using InlayHintType = OmniSharp.Extensions.LanguageServer.Protocol.Models.InlayHint;

namespace EmmyLua.LanguageServer.InlayHint;

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
        var renderBuilder = new LuaRenderBuilder(semanticModel.Context);
        var syntaxTree = semanticModel.Document.SyntaxTree;
        var hints = new List<InlayHintType>();
        var sourceBlock = syntaxTree.SyntaxRoot.Block;
        if (sourceBlock is null)
        {
            return hints;
        }

        try
        {
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
                            ClosureExprHint(semanticModel, hints, closureExpr, renderBuilder, cancellationToken);
                        }

                        break;
                    }
                    case LuaIndexExprSyntax indexExpr:
                    {
                        if (config.IndexHint)
                        {
                            IndexExprHint(semanticModel, hints, indexExpr, renderBuilder, cancellationToken);
                        }

                        break;
                    }
                    case LuaLocalNameSyntax localName:
                    {
                        if (config.LocalHint)
                        {
                            LocalNameHint(semanticModel, hints, localName, renderBuilder, cancellationToken);
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
        }
        catch (OperationCanceledException)
        {
            // ignore
        }

        return hints;
    }

    private void CallExprHint(SemanticModel semanticModel, List<InlayHintType> hints, LuaCallExprSyntax callExpr,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (semanticModel.Context.InferAndUnwrap(callExpr.PrefixExpr) is LuaMethodType method)
        {
            var args = callExpr.ArgList?.ArgList.ToList() ?? [];
            var perfectSignature = semanticModel.Context.FindPerfectMatchSignature(method, callExpr, args);

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
                            Label = new StringOrInlayHintLabelParts("self:"),
                            Kind = InlayHintKind.Parameter,
                            PaddingRight = true
                        });
                    }

                    args = args.Skip(1).ToList();
                    break;
                }
            }

            var parameters = perfectSignature.Parameters.Skip(skipParam).ToList();
            var hasVarArg = parameters.LastOrDefault() is LuaDeclaration { Info: ParamInfo { IsVararg: true } };
            var parameterCount = hasVarArg ? (parameters.Count - 1) : parameters.Count;
            var varCount = 0;
            for (var i = 0; i < args.Count; i++)
            {
                var arg = args[i];
                if (i < parameterCount)
                {
                    var parameter = parameters[i];
                    var nullableText = string.Empty;
                    var location = parameter.GetLocation(semanticModel.Context)?.ToLspLocation();
                    if (parameter is LuaDeclaration { Info: ParamInfo { Nullable: true } })
                    {
                        nullableText = "?";
                    }

                    hints.Add(new InlayHintType()
                    {
                        Position = arg.Position.ToLspPosition(semanticModel.Document),
                        Label = new StringOrInlayHintLabelParts(new[]
                        {
                            new InlayHintLabelPart()
                            {
                                Value = $"{parameter.Name}{nullableText}:",
                                Location = location
                            }
                        }),
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

        if (callExpr.PrefixExpr is { } prefixExpr)
        {
            var luaDeclaration = semanticModel.Context.FindDeclaration(prefixExpr);
            if (luaDeclaration is { IsAsync: true })
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

    private void ClosureExprHint(SemanticModel semanticModel, List<InlayHintType> hints,
        LuaClosureExprSyntax closureExpr,
        LuaRenderBuilder renderBuilder,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (semanticModel.Context.InferAndUnwrap(closureExpr) is LuaMethodType method)
        {
            var mainSignature = method.MainSignature;
            var parameterDic = new Dictionary<string, LuaType?>();
            foreach (var parameter in mainSignature.Parameters)
            {
                parameterDic.TryAdd(parameter.Name, parameter.Type);
            }

            var parameters = closureExpr.ParamList?
                .Params.Select(it => it.Name).ToList() ?? [];

            foreach (var parameter in parameters)
            {
                if (parameter is { RepresentText: { } name })
                {
                    var type = parameterDic.GetValueOrDefault(name);
                    if (type is not null && !type.Equals(Builtin.Unknown))
                    {
                        hints.Add(new InlayHintType()
                        {
                            Position = parameter.Range.EndOffset.ToLspPosition(semanticModel.Document),
                            Label = new StringOrInlayHintLabelParts(
                                $":{renderBuilder.RenderType(type, RenderFeature)}"),
                            Kind = InlayHintKind.Parameter,
                            PaddingLeft = true
                        });
                    }
                }
            }
        }
    }

    private void IndexExprHint(
        SemanticModel semanticModel,
        List<InlayHintType> hints,
        LuaIndexExprSyntax indexExpr,
        LuaRenderBuilder renderBuilder,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var document = semanticModel.Document;
        if (indexExpr is { PrefixExpr: { } prefixExpr, KeyElement: { } keyElement })
        {
            if (document.GetLine(prefixExpr.Range.EndOffset) != document.GetLine(keyElement.Range.StartOffset))
            {
                var type = semanticModel.Context.InferAndUnwrap(prefixExpr);
                hints.Add(new InlayHintType()
                {
                    Position = prefixExpr.Range.EndOffset.ToLspPosition(semanticModel.Document),
                    Label = new StringOrInlayHintLabelParts(
                        $"// {renderBuilder.RenderType(type, RenderFeature)}"),
                    Kind = InlayHintKind.Type,
                    PaddingLeft = true
                });
            }
        }
    }

    private void LocalNameHint(
        SemanticModel semanticModel,
        List<InlayHintType> hints,
        LuaLocalNameSyntax localName,
        LuaRenderBuilder renderBuilder,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (localName.Parent is LuaFuncStatSyntax)
        {
            return;
        }

        var type = semanticModel.Context.InferAndUnwrap(localName);
        hints.Add(new InlayHintType()
        {
            Position = localName.Range.EndOffset.ToLspPosition(semanticModel.Document),
            Label = new StringOrInlayHintLabelParts(
                $":{renderBuilder.RenderType(type, RenderFeature)}"),
            Kind = InlayHintKind.Type,
            PaddingLeft = true
        });
    }

    private void OverrideHint(SemanticModel semanticModel, List<InlayHintType> hints, LuaFuncStatSyntax funcStat,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (funcStat is
            {
                IsMethod: true, IndexExpr: { PrefixExpr: { } prefixExpr, Name: { } name },
                ClosureExpr: { ParamList: { } paramList }
            })
        {
            var prefixType = semanticModel.Context.InferAndUnwrap(prefixExpr);
            var superMethod = semanticModel.Context.FindSuperMember(prefixType, name).FirstOrDefault();
            if (superMethod is LuaDeclaration { Info: { } info })
            {
                var document = semanticModel.Document;
                var parentDocument = semanticModel.Compilation.Workspace.GetDocument(info.Ptr.DocumentId);
                var location = new Location();
                if (parentDocument is not null)
                {
                    location = info.Ptr.ToNode(parentDocument)!.Range.ToLspLocation(parentDocument);
                    if (info.Ptr.ToNode(semanticModel.Context) is LuaIndexExprSyntax { KeyElement: { } keyElement })
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