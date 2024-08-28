using System.Text;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Compile.Kind;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Completion;

namespace EmmyLua.LanguageServer.Completion.CompleteProvider;

public class DocProvider : ICompleteProviderBase
{
    private List<string> Tags { get; } =
    [
        "class", "enum", "interface", "alias", "module", "field", "param", "return", "see", "deprecated",
        "type", "overload", "generic", "async", "cast", "private", "protected", "public", "operator",
        "meta", "version", "as", "nodiscard", "diagnostic", "mapping", "namespace", "using" // "package",
    ];

    private List<string> Actions { get; } = ["disable-next-line", "disable", "enable"];

    public void AddCompletion(CompleteContext context)
    {
        var triggerToken = context.TriggerToken;
        switch (triggerToken)
        {
            case { Kind: LuaTokenKind.TkDocStart or LuaTokenKind.TkTagOther }:
            {
                AddTagCompletion(context);
                AddTagParamReturn(context);
                break;
            }
            case LuaNameToken { Parent: LuaDocTagParamSyntax paramSyntax }:
            {
                AddParamNameCompletion(paramSyntax, context);
                break;
            }
            case LuaNameToken { Parent: LuaDocNameTypeSyntax, RepresentText: { } filter }:
            {
                AddTypeNameCompletion(filter, context);
                break;
            }
            case LuaNameToken { Parent: LuaDocTagDiagnosticSyntax }:
            {
                AddDiagnosticActionCompletion(context);
                break;
            }
            case LuaNameToken { Parent: LuaDocDiagnosticNameListSyntax }:
            {
                AddDiagnosticCodeCompletion(context);
                break;
            }
            case LuaNameToken { Parent: LuaDocTagNamespaceSyntax or LuaDocTagUsingSyntax }:
            {
                AddTypeNameCompletion(triggerToken.RepresentText, context);
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

    private void AddTagParamReturn(CompleteContext context)
    {
        var commentSyntax = context.TriggerToken?.Ancestors.OfType<LuaCommentSyntax>().FirstOrDefault();
        if (commentSyntax is null)
        {
            return;
        }

        var funcStat = commentSyntax.Owner as LuaFuncStatSyntax;
        if (funcStat is null)
        {
            return;
        }

        var paramList = funcStat.ClosureExpr?.ParamList;
        if (paramList is not null)
        {
            var paramNameList = new List<string>();
            foreach (var param in paramList.Params)
            {
                if (param.IsVarArgs)
                {
                    paramNameList.Add("...");
                }
                else
                {
                    paramNameList.Add(param.Name?.RepresentText ?? "");
                }
            }

            var sb = new StringBuilder();
            for (var i = 0; i < paramNameList.Count; i++)
            {
                var paramName = paramNameList[i];
                if (i == 0)
                {
                    sb.Append($"param {paramName} ${{{i + 1}:any}}\n");
                }
                else
                {
                    sb.Append($"---@param {paramName} ${{{i + 1}:any}}\n");
                }
            }

            sb.Append("---@return ${0:any}");

            context.CreateSnippet("param;@return")
                .WithDetail(" ( ... )")
                .WithInsertText(sb.ToString())
                .AddToContext();
        }
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

    private void AddTypeNameCompletion(string prefix, CompleteContext context)
    {
        var prefixNamespace = string.Empty;
        var dotIndex = prefix.LastIndexOf('.');
        if (dotIndex != -1)
        {
            prefixNamespace = prefix[..dotIndex];
        }

        var namespaceOrTypes =
            context.SemanticModel.Compilation.TypeManager.GetNamespaceOrTypeInfos(prefixNamespace,
                context.SemanticModel.Document.Id);
        foreach (var namespaceOrType in namespaceOrTypes)
        {
            if (namespaceOrType.IsNamespace)
            {
                context.Add(new CompletionItem()
                {
                    Label = namespaceOrType.Name,
                    Kind = CompletionItemKind.Module,
                });
            }
            else
            {
                context.Add(new CompletionItem()
                {
                    Label = namespaceOrType.Name,
                    Kind = ConvertTypedName(namespaceOrType.Kind),
                    Data = namespaceOrType.Id.Stringify
                });
            }
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

    private void AddDiagnosticActionCompletion(CompleteContext context)
    {
        foreach (var action in Actions)
        {
            context.Add(new CompletionItem
            {
                Label = action,
                Kind = CompletionItemKind.EnumMember,
                Detail = "action",
            });
        }
    }

    private void AddDiagnosticCodeCompletion(CompleteContext context)
    {
        var diagnosticCodes = context.SemanticModel.Compilation.Diagnostics.GetDiagnosticNames();
        foreach (var diagnosticCode in diagnosticCodes)
        {
            context.Add(new CompletionItem
            {
                Label = diagnosticCode,
                Kind = CompletionItemKind.EnumMember,
                Detail = "diagnostic code",
            });
        }
    }
}