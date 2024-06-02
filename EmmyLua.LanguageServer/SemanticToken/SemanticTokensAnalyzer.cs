using EmmyLua.CodeAnalysis.Compilation.Semantic;
using EmmyLua.CodeAnalysis.Syntax.Kind;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.LanguageServer.Util;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace EmmyLua.LanguageServer.SemanticToken;

public class SemanticTokensAnalyzer
{
    public SemanticTokensLegend Legend { get; }

    public SemanticTokensAnalyzer()
    {
        Legend = new()
        {
            TokenTypes = TokenTypes,
            TokenModifiers = TokenModifiers
        };
    }

    public readonly List<SemanticTokenType> TokenTypes = new()
    {
        SemanticTokenType.Comment,
        SemanticTokenType.Keyword,
        SemanticTokenType.String,
        SemanticTokenType.Number,
        SemanticTokenType.Regexp,
        SemanticTokenType.Operator,
        SemanticTokenType.Type,
        SemanticTokenType.Class,
        SemanticTokenType.Interface,
        SemanticTokenType.Enum,
        SemanticTokenType.TypeParameter,
        SemanticTokenType.Function,
        SemanticTokenType.Method,
        SemanticTokenType.Property,
        SemanticTokenType.Variable,
        SemanticTokenType.Parameter,
        SemanticTokenType.Label,
        SemanticTokenType.Modifier,
        SemanticTokenType.EnumMember,
        SemanticTokenType.Decorator
    };

    public readonly List<SemanticTokenModifier> TokenModifiers = new()
    {
        SemanticTokenModifier.Declaration,
        SemanticTokenModifier.Definition,
        SemanticTokenModifier.Readonly,
        SemanticTokenModifier.Static,
        SemanticTokenModifier.Abstract,
        SemanticTokenModifier.Deprecated,
        SemanticTokenModifier.Async,
        SemanticTokenModifier.DefaultLibrary,
    };

    public void Tokenize(SemanticTokensBuilder builder, SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        var document = semanticModel.Document;
        var syntaxTree = document.SyntaxTree;
        try
        {
            foreach (var nodeOrToken in syntaxTree.SyntaxRoot.DescendantsWithToken)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return;
                }

                switch (nodeOrToken)
                {
                    case LuaSyntaxToken token:
                    {
                        TokenizeToken(builder, token, semanticModel, cancellationToken);
                        break;
                    }
                    case LuaSyntaxNode node:
                    {
                        TokenizeNode(builder, node, semanticModel, cancellationToken);
                        break;
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            // ignored
        }
    }

    private void TokenizeToken(SemanticTokensBuilder builder, LuaSyntaxToken token, SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var tokenKind = token.Kind;
        switch (tokenKind)
        {
            case LuaTokenKind.TkIf:
            case LuaTokenKind.TkElse:
            case LuaTokenKind.TkElseIf:
            case LuaTokenKind.TkEnd:
            case LuaTokenKind.TkFor:
            case LuaTokenKind.TkFunction:
            case LuaTokenKind.TkIn:
            // case LuaTokenKind.TkLocal:
            case LuaTokenKind.TkRepeat:
            case LuaTokenKind.TkReturn:
            case LuaTokenKind.TkThen:
            case LuaTokenKind.TkUntil:
            case LuaTokenKind.TkGoto:
            case LuaTokenKind.TkWhile:
            case LuaTokenKind.TkBreak:
            case LuaTokenKind.TkDo:
            {
                builder.Push(token.Range.ToLspRange(semanticModel.Document), SemanticTokenType.Keyword, string.Empty);
                break;
            }
            case LuaTokenKind.TkString:
            case LuaTokenKind.TkLongString:
            {
                var range = token.Range.ToLspRange(semanticModel.Document);
                if (range.Start.Line == range.End.Line)
                {
                    builder.Push(token.Range.ToLspRange(semanticModel.Document), SemanticTokenType.String,
                        string.Empty);
                }

                break;
            }
            case LuaTokenKind.TkAnd:
            case LuaTokenKind.TkOr:
            case LuaTokenKind.TkConcat:
            case LuaTokenKind.TkEq:
            case LuaTokenKind.TkNe:
            case LuaTokenKind.TkLe:
            case LuaTokenKind.TkGe:
            case LuaTokenKind.TkShl:
            case LuaTokenKind.TkShr:
            case LuaTokenKind.TkBitXor:
            case LuaTokenKind.TkBitAnd:
            case LuaTokenKind.TkBitOr:
            case LuaTokenKind.TkPlus:
            case LuaTokenKind.TkMinus:
            case LuaTokenKind.TkMul:
            case LuaTokenKind.TkDiv:
            case LuaTokenKind.TkMod:
            case LuaTokenKind.TkPow:
            case LuaTokenKind.TkLen:
            {
                builder.Push(token.Range.ToLspRange(semanticModel.Document), SemanticTokenType.Operator, string.Empty);
                break;
            }
            case LuaTokenKind.TkInt:
            case LuaTokenKind.TkFloat:
            case LuaTokenKind.TkComplex:
            {
                builder.Push(token.Range.ToLspRange(semanticModel.Document), SemanticTokenType.Number, string.Empty);
                break;
            }
            case LuaTokenKind.TkDocDetail:
            {
                var range = token.Range.ToLspRange(semanticModel.Document);
                if (range.Start.Line == range.End.Line)
                {
                    builder.Push(token.Range.ToLspRange(semanticModel.Document), SemanticTokenType.Comment,
                        string.Empty);
                }

                break;
            }
            case LuaTokenKind.TkTypeTemplate:
            {
                builder.Push(token.Range.ToLspRange(semanticModel.Document), SemanticTokenType.Type, string.Empty);
                break;
            }
            // TODO 
            // case LuaTokenKind.TkTagAlias:
            // case LuaTokenKind.TkTagClass:
            // case LuaTokenKind.TkTagEnum:
            // case LuaTokenKind.TkTagAs:
            // case LuaTokenKind.TkTagField:
            // case LuaTokenKind.TkTagInterface:
            // case LuaTokenKind.TkTagModule:
            // {
            //     builder.Push(token.Range.ToLspRange(semanticModel.Document), SemanticTokenType.Label, string.Empty);
            //     break;
            // }
        }
    }

    private void TokenizeNode(SemanticTokensBuilder builder, LuaSyntaxNode node, SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        switch (node)
        {
            case LuaDocTagClassSyntax docTagClassSyntax:
            {
                if (docTagClassSyntax.Name is { } name)
                {
                    builder.Push(name.Range.ToLspRange(semanticModel.Document), SemanticTokenType.Class,
                        SemanticTokenModifier.Declaration);
                }

                break;
            }
            case LuaDocTagEnumSyntax docTagEnumSyntax:
            {
                if (docTagEnumSyntax.Name is { } name)
                {
                    builder.Push(name.Range.ToLspRange(semanticModel.Document), SemanticTokenType.Enum,
                        SemanticTokenModifier.Declaration);
                }

                break;
            }
            case LuaDocTagInterfaceSyntax docTagInterfaceSyntax:
            {
                if (docTagInterfaceSyntax.Name is { } name)
                {
                    builder.Push(name.Range.ToLspRange(semanticModel.Document), SemanticTokenType.Interface,
                        SemanticTokenModifier.Declaration);
                }

                break;
            }
            case LuaDocNameTypeSyntax nameTypeSyntax:
            {
                if (nameTypeSyntax.Name is { } name)
                {
                    builder.Push(name.Range.ToLspRange(semanticModel.Document), SemanticTokenType.Type, string.Empty);
                }

                break;
            }
        }
    }
}