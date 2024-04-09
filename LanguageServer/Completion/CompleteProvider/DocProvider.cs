using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Kind;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace LanguageServer.Completion.CompleteProvider;

public class DocProvider : ICompleteProviderBase
{
    private List<string> Tags { get; } =
    [
        "class", "enum", "interface", "alias", "module", "field", "param", "return", "see", "deprecated",
        "type", "overload", "generic", "async", "cast", "private", "protected", "public", "operator",
        "meta", "version", "as", "nodiscard", "diagnostic", // "package",
    ];

    public void AddCompletion(CompleteContext context)
    {
        var triggerToken = context.TriggerToken;
        switch (triggerToken)
        {
            case { Kind: LuaTokenKind.TkDocStart }:
            {
                AddTagCompletion(context);
                break;
            }
            case LuaNameToken { Parent: LuaDocTagParamSyntax paramSyntax }:
            {
                AddParamNameCompletion(paramSyntax, context);
                break;
            }
            case LuaNameToken { Parent: LuaDocNameTypeSyntax }:
            {
                AddTypeNameCompletion(context);
                break;
            }
        }
    }

    private void AddTagCompletion(CompleteContext context)
    {
        foreach (var tag in Tags)
        {
            context.Add(new CompletionItem
            {
                Label = tag,
                Kind = CompletionItemKind.EnumMember,
                Detail = "tag",
            });
        }

        context.StopHere();
    }

    private void AddParamNameCompletion(LuaDocTagParamSyntax paramSyntax, CompleteContext context)
    {
        var comment = paramSyntax.Ancestors.OfType<LuaCommentSyntax>().FirstOrDefault();
        if (comment is null)
        {
            return;
        }

        if (comment.Owner is LuaFuncStatSyntax funcStat)
        {
            var paramNames = funcStat.ClosureExpr?.ParamList?.Params
                .Select(p => p.Name?.RepresentText)
                .ToList();

            if (paramNames is null) return;
            foreach (var paramName in paramNames.OfType<string>())
            {
                context.Add(new CompletionItem
                {
                    Label = paramName,
                    Kind = CompletionItemKind.Variable,
                });
            }
        }
    }

    private void AddTypeNameCompletion(CompleteContext context)
    {
        var namedTypes = context.SemanticModel.Compilation.ProjectIndex.GetNamedTypes();
        foreach (var typeDeclaration in namedTypes)
        {
            context.Add(new CompletionItem
            {
                Label = typeDeclaration.Name,
                Kind = ConvertTypedName(typeDeclaration.Kind),
                Data = typeDeclaration.Ptr.Stringify
            });
        }
    }

    private static CompletionItemKind ConvertTypedName(NamedTypeKind kind)
    {
        return kind switch
        {
            NamedTypeKind.Class => CompletionItemKind.Class,
            NamedTypeKind.Enum => CompletionItemKind.Enum,
            NamedTypeKind.Interface => CompletionItemKind.Interface,
            NamedTypeKind.Alias => CompletionItemKind.Reference,
            _ => CompletionItemKind.Text,
        };
    }
}