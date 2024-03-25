using EmmyLua.CodeAnalysis.Compilation.Semantic;
using EmmyLua.CodeAnalysis.Kind;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using LanguageServer.ExtensionUtil;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace LanguageServer.SemanticToken;

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

    public List<SemanticTokenType> TokenTypes = new()
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

    public List<SemanticTokenModifier> TokenModifiers = new()
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

    private void TokenizeToken(SemanticTokensBuilder builder, LuaSyntaxToken token, SemanticModel semanticModel,
        CancellationToken cancellationToken)
    {
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
                builder.Push(token.Range.ToLspRange(semanticModel.Document), SemanticTokenType.String, string.Empty);
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
                builder.Push(token.Range.ToLspRange(semanticModel.Document), SemanticTokenType.Comment, string.Empty);
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
        switch (node)
        {
            case LuaLocalNameSyntax localNameSyntax:
            {
                var modify = new List<SemanticTokenModifier>() { SemanticTokenModifier.Declaration };
                if (localNameSyntax.Attribute is { IsConst: true })
                {
                    modify.Add(SemanticTokenModifier.Readonly);
                }

                builder.Push(localNameSyntax.Range.ToLspRange(semanticModel.Document), SemanticTokenType.Variable,
                    modify);
                break;
            }
            case LuaAssignStatSyntax assignStatSyntax:
            {
                foreach (var expr in assignStatSyntax.VarList)
                {
                    if (expr is LuaNameExprSyntax { Name: { } name } nameExpr)
                    {
                        builder.Push(name.Range.ToLspRange(semanticModel.Document), SemanticTokenType.Variable,
                            string.Empty);
                    }
                }

                break;
            }
            case LuaForStatSyntax forStatSyntax:
            {
                if (forStatSyntax.IteratorName is { } itName)
                {
                    builder.Push(itName.Range.ToLspRange(semanticModel.Document), SemanticTokenType.Variable,
                        string.Empty);
                }

                break;
            }
            case LuaForRangeStatSyntax forRangeStatSyntax:
            {
                foreach (var itName in forRangeStatSyntax.IteratorNames)
                {
                    builder.Push(itName.Range.ToLspRange(semanticModel.Document), SemanticTokenType.Variable,
                        SemanticTokenModifier.Declaration);
                }

                break;
            }
            case LuaFuncStatSyntax funcStatSyntax:
            {
                if (funcStatSyntax.LocalName is { } localName)
                {
                    builder.Push(localName.Range.ToLspRange(semanticModel.Document), SemanticTokenType.Function,
                        SemanticTokenModifier.Definition);
                }
                else if (funcStatSyntax.NameExpr is { } nameExpr)
                {
                    builder.Push(nameExpr.Range.ToLspRange(semanticModel.Document), SemanticTokenType.Function,
                        SemanticTokenModifier.Definition);
                }
                else if (funcStatSyntax.IndexExpr is { } indexExpr)
                {
                    builder.Push(indexExpr.KeyElement.Range.ToLspRange(semanticModel.Document),
                        SemanticTokenType.Function, SemanticTokenModifier.Definition);
                }

                break;
            }
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