using EmmyLua.CodeAnalysis.Compilation.Semantic;
using EmmyLua.CodeAnalysis.Syntax.Kind;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.LanguageServer.Framework.Protocol.Capabilities.Common;
using EmmyLua.LanguageServer.Framework.Protocol.Message.SemanticToken;
using EmmyLua.LanguageServer.Framework.Protocol.Model;
using EmmyLua.LanguageServer.Util;

namespace EmmyLua.LanguageServer.SemanticToken;

public class SemanticTokensAnalyzer
{
    public SemanticTokensLegend Legend { get; }

    public bool MultiLineTokenSupport { get; set; }

    public SemanticTokensAnalyzer()
    {
        Legend = new()
        {
            TokenTypes = TokenTypes,
            TokenModifiers = TokenModifiers
        };
    }

    private List<string> TokenTypes { get; } =
    [
        SemanticTokenTypes.Namespace,
        SemanticTokenTypes.Type,
        SemanticTokenTypes.Class,
        SemanticTokenTypes.Enum,
        SemanticTokenTypes.Interface,
        SemanticTokenTypes.Struct,
        SemanticTokenTypes.TypeParameter,
        SemanticTokenTypes.Parameter,
        SemanticTokenTypes.Variable,
        SemanticTokenTypes.Property,
        SemanticTokenTypes.EnumMember,
        SemanticTokenTypes.Event,
        SemanticTokenTypes.Function,
        SemanticTokenTypes.Method,
        SemanticTokenTypes.Macro,
        SemanticTokenTypes.Keyword,
        SemanticTokenTypes.Modifier,
        SemanticTokenTypes.Comment,
        SemanticTokenTypes.String,
        SemanticTokenTypes.Number,
        SemanticTokenTypes.Regexp,
        SemanticTokenTypes.Operator,
        SemanticTokenTypes.Decorator,
    ];

    private List<string> TokenModifiers { get; } =
    [
        SemanticTokenModifiers.Declaration,
        SemanticTokenModifiers.Definition,
        SemanticTokenModifiers.Readonly,
        SemanticTokenModifiers.Static,
        SemanticTokenModifiers.Deprecated,
        SemanticTokenModifiers.Abstract,
        SemanticTokenModifiers.Async,
        SemanticTokenModifiers.Modification,
        SemanticTokenModifiers.Documentation,
        SemanticTokenModifiers.DefaultLibrary,
    ];

    public List<uint> Tokenize(SemanticModel semanticModel, bool isVscode, CancellationToken cancellationToken)
    {
        var innerBuilder = new SemanticTokensBuilder(TokenTypes, TokenModifiers);
        var builder = new SemanticBuilderWrapper(innerBuilder, semanticModel.Document, MultiLineTokenSupport);
        var document = semanticModel.Document;
        var syntaxTree = document.SyntaxTree;
        try
        {
            foreach (var nodeOrToken in syntaxTree.SyntaxRoot.DescendantsWithToken)
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return [];
                }

                switch (nodeOrToken)
                {
                    case LuaSyntaxToken token:
                    {
                        TokenizeToken(builder, token, isVscode);
                        break;
                    }
                    case LuaSyntaxNode node:
                    {
                        TokenizeNode(builder, node);
                        break;
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            // ignore
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
        }

        return builder.Build();
    }

    public List<uint> TokenizeByRange(SemanticModel semanticModel, bool isVscode, DocumentRange range,
        CancellationToken cancellationToken)
    {
        var innerBuilder = new SemanticTokensBuilder(TokenTypes, TokenModifiers);
        var builder = new SemanticBuilderWrapper(innerBuilder, semanticModel.Document, MultiLineTokenSupport);
        var document = semanticModel.Document;
        var syntaxTree = document.SyntaxTree;
        try
        {
            var sourceRange = range.ToSourceRange(document);
            foreach (var nodeOrToken in syntaxTree.SyntaxRoot.DescendantsInRange(sourceRange))
            {
                if (cancellationToken.IsCancellationRequested)
                {
                    return [];
                }

                switch (nodeOrToken)
                {
                    case LuaSyntaxToken token:
                    {
                        TokenizeToken(builder, token, isVscode);
                        break;
                    }
                    case LuaSyntaxNode node:
                    {
                        TokenizeNode(builder, node);
                        break;
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            // ignore
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e);
        }

        return builder.Build();
    }

    private void TokenizeToken(SemanticBuilderWrapper builder, LuaSyntaxToken token, bool isVscode)
    {
        var tokenKind = token.Kind;
        switch (tokenKind)
        {
            case LuaTokenKind.TkLocal:
            {
                if (isVscode)
                {
                    return;
                }

                builder.Push(token, SemanticTokenTypes.Keyword);
                break;
            }
            case LuaTokenKind.TkIf:
            case LuaTokenKind.TkElse:
            case LuaTokenKind.TkElseIf:
            case LuaTokenKind.TkEnd:
            case LuaTokenKind.TkFor:
            case LuaTokenKind.TkFunction:
            case LuaTokenKind.TkIn:
            case LuaTokenKind.TkRepeat:
            case LuaTokenKind.TkReturn:
            case LuaTokenKind.TkThen:
            case LuaTokenKind.TkUntil:
            case LuaTokenKind.TkGoto:
            case LuaTokenKind.TkWhile:
            case LuaTokenKind.TkBreak:
            case LuaTokenKind.TkDo:
            case LuaTokenKind.TkAnd:
            case LuaTokenKind.TkOr:
            {
                builder.Push(token, SemanticTokenTypes.Keyword);
                break;
            }
            case LuaTokenKind.TkString:
            case LuaTokenKind.TkLongString:
            {
                builder.Push(token, SemanticTokenTypes.String);
                break;
            }
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
            case LuaTokenKind.TkIDiv:
            case LuaTokenKind.TkDocMatch:
            case LuaTokenKind.TkColon:
            case LuaTokenKind.TkDbColon:
            case LuaTokenKind.TkSemicolon:
            case LuaTokenKind.TkLeftBracket:
            case LuaTokenKind.TkRightBracket:
            case LuaTokenKind.TkLeftParen:
            case LuaTokenKind.TkRightParen:
            case LuaTokenKind.TkLeftBrace:
            case LuaTokenKind.TkRightBrace:
            case LuaTokenKind.TkDots:
            case LuaTokenKind.TkComma:
            case LuaTokenKind.TkAssign:
            case LuaTokenKind.TkDot:
            {
                builder.Push(token, SemanticTokenTypes.Operator);
                break;
            }
            case LuaTokenKind.TkInt:
            case LuaTokenKind.TkFloat:
            case LuaTokenKind.TkComplex:
            {
                builder.Push(token, SemanticTokenTypes.Number);
                break;
            }
            case LuaTokenKind.TkDocDetail:
            case LuaTokenKind.TkUnknown:
            {
                builder.Push(token, SemanticTokenTypes.Comment);
                break;
            }
            case LuaTokenKind.TkTypeTemplate:
            {
                builder.Push(token, SemanticTokenTypes.String, SemanticTokenModifiers.Abstract);
                break;
            }
            case LuaTokenKind.TkTagAlias:
            case LuaTokenKind.TkTagClass:
            case LuaTokenKind.TkTagEnum:
            case LuaTokenKind.TkTagAs:
            case LuaTokenKind.TkTagField:
            case LuaTokenKind.TkTagInterface:
            case LuaTokenKind.TkTagModule:
            case LuaTokenKind.TkTagParam:
            case LuaTokenKind.TkTagReturn:
            case LuaTokenKind.TkTagSee:
            case LuaTokenKind.TkTagType:
            case LuaTokenKind.TkTagAsync:
            case LuaTokenKind.TkTagCast:
            case LuaTokenKind.TkTagDeprecated:
            case LuaTokenKind.TkTagGeneric:
            case LuaTokenKind.TkTagNodiscard:
            case LuaTokenKind.TkTagOperator:
            case LuaTokenKind.TkTagOther:
            case LuaTokenKind.TkTagOverload:
            case LuaTokenKind.TkTagVisibility:
            case LuaTokenKind.TkTagDiagnostic:
            case LuaTokenKind.TkTagMeta:
            case LuaTokenKind.TkTagVersion:
            case LuaTokenKind.TkTagMapping:
            case LuaTokenKind.TkDocEnumField:
            {
                if (!isVscode)
                {
                    builder.Push(token, SemanticTokenTypes.Keyword, SemanticTokenModifiers.Documentation);
                }

                break;
            }
        }
    }

    private void TokenizeNode(SemanticBuilderWrapper builder, LuaSyntaxNode node)
    {
        switch (node)
        {
            case LuaDocTagClassSyntax docTagClassSyntax:
            {
                if (docTagClassSyntax.Name is { } name)
                {
                    builder.Push(name, SemanticTokenTypes.Class, SemanticTokenModifiers.Declaration);
                }

                break;
            }
            case LuaDocTagEnumSyntax docTagEnumSyntax:
            {
                if (docTagEnumSyntax.Name is { } name)
                {
                    builder.Push(name, SemanticTokenTypes.Enum, SemanticTokenModifiers.Declaration);
                }

                break;
            }
            case LuaDocTagInterfaceSyntax docTagInterfaceSyntax:
            {
                if (docTagInterfaceSyntax.Name is { } name)
                {
                    builder.Push(name, SemanticTokenTypes.Interface, SemanticTokenModifiers.Declaration);
                }

                break;
            }
            case LuaDocTagAliasSyntax docTagAliasSyntax:
            {
                if (docTagAliasSyntax.Name is { } name)
                {
                    builder.Push(name, SemanticTokenTypes.Type, SemanticTokenModifiers.Declaration);
                }

                break;
            }
            case LuaDocNameTypeSyntax nameTypeSyntax:
            {
                if (nameTypeSyntax.Name is { } name)
                {
                    builder.Push(name, SemanticTokenTypes.Type);
                }

                break;
            }
        }
    }
}