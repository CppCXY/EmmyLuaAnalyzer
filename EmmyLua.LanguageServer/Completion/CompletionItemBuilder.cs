using System.Text;
using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Search;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Completion;
using EmmyLua.LanguageServer.Framework.Protocol.Model;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Kind;
using EmmyLua.LanguageServer.Framework.Protocol.Model.Union;

namespace EmmyLua.LanguageServer.Completion;

public class CompletionItemBuilder(string label, LuaType type, CompleteContext completeContext)
{
    private string Label { get; set; } = label;

    private string? InsertText { get; set; }

    private CompletionItemKind Kind { get; set; } = CompletionItemKind.Variable;

    private LuaType Type { get; set; } = type;

    private string? Data { get; set; }

    private Command? Command { get; set; }

    private TextEditOrInsertReplaceEdit? TextOrReplaceEdit { get; set; }

    private bool Colon { get; set; } = false;

    private bool Disable { get; set; } = false;

    private bool IsDeprecated { get; set; } = false;

    private SearchContext SearchContext => CompleteContext.SemanticModel.Context;

    private CompleteContext CompleteContext { get; } = completeContext;

    public CompletionItemBuilder WithKind(CompletionItemKind kind)
    {
        Kind = kind;
        return this;
    }

    public CompletionItemBuilder WithData(string data)
    {
        Data = data;
        return this;
    }

    public CompletionItemBuilder WithCheckDeclaration(LuaDeclaration declaration)
    {
        if (declaration.IsDeprecated)
        {
            IsDeprecated = true;
        }
        
        if (declaration.RequiredVersions is not null)
        {
            var feature = CompleteContext.ServerContext.LuaProject.Features;
            var languageLevel = feature.Language.LanguageLevel;
            if (!declaration.ValidateLuaVersion(languageLevel))
            {
                Disable = true;
            }

            var frameworkVersion = feature.FrameworkVersions;
            if (!Disable && !declaration.ValidateFrameworkVersions(frameworkVersion))
            {
                Disable = true;
            }
        }

        return this;
    }

    public CompletionItemBuilder WithColon(bool colon)
    {
        Colon = colon;
        return this;
    }

    public CompletionItemBuilder WithInsertText(string insertText)
    {
        InsertText = insertText;
        return this;
    }

    public CompletionItemBuilder WithDotCheckBracketLabel(LuaIndexExprSyntax indexExpr)
    {
        if (Label.StartsWith('['))
        {
            var dot = indexExpr.Dot;
            if (dot is not null)
            {
                Disable = true;
            }
        }

        return this;
    }

    public CompletionItemBuilder WithCommand(Command command)
    {
        Command = command;
        return this;
    }

    public CompletionItemBuilder WithTextEditOrReplaceEdit(TextEditOrInsertReplaceEdit textOrReplaceEdit)
    {
        TextOrReplaceEdit = textOrReplaceEdit;
        return this;
    }

    public void AddToContext()
    {
        if (Disable)
        {
            return;
        }

        switch (Type)
        {
            case LuaMethodType methodType:
            {
                var mainSignature = methodType.MainSignature;
                CompleteContext.Add(GetSignatureCompletionItem(Label, mainSignature, methodType.ColonDefine));

                if (methodType.Overloads is not null)
                {
                    foreach (var signature in methodType.Overloads)
                    {
                        CompleteContext.Add(GetSignatureCompletionItem(Label, signature, methodType.ColonDefine));
                    }
                }

                break;
            }
            default:
            {
                if (!Colon)
                {
                    var completionItem = new CompletionItem()
                    {
                        Label = Label,
                        Kind = Kind,
                        LabelDetails = new CompletionItemLabelDetails()
                        {
                            Description = CompleteContext.RenderBuilder.RenderType(Type,
                                CompleteContext.RenderFeature),
                        },
                        InsertText = InsertText,
                        Data = Data,
                        Command = Command,
                        TextEdit = TextOrReplaceEdit
                    };
                    if (IsDeprecated)
                    {
                        completionItem = completionItem with
                        {
                            Tags = [CompletionItemTag.Deprecated]
                        };
                    }

                    CompleteContext.Add(completionItem);
                }

                break;
            }
        }
    }

    private string RenderInsertTextFuncParams(string name, LuaSignature signature, bool colonDefine)
    {
        var sb = new StringBuilder();
        sb.Append(name);
        sb.Append('(');
        var parameters = signature.Parameters;
        var paramsCnt = 1;
        switch ((colonDefine, Colon))
        {
            case (true, false):
            {
                sb.Append("${1:self}");
                if (parameters.Count > 0)
                {
                    sb.Append(", ");
                }

                paramsCnt++;
                break;
            }
            case (false, true):
            {
                parameters = parameters.Skip(1).ToList();
                break;
            }
        }

        for (var i = 0; i < parameters.Count; i++)
        {
            var parameter = parameters[i];
            sb.Append($"${{{paramsCnt}:{parameter.Name}}}");
            if (i < parameters.Count - 1)
            {
                sb.Append(", ");
            }

            paramsCnt++;
        }

        sb.Append(')');
        return sb.ToString();
    }

    private string RenderSignatureParams(LuaSignature signature, bool colonDefine)
    {
        var sb = new StringBuilder();
        sb.Append('(');
        var parameters = signature.Parameters;
        switch ((colonDefine, Colon))
        {
            case (true, false):
            {
                sb.Append("self");
                if (parameters.Count > 0)
                {
                    sb.Append(", ");
                }

                break;
            }
            case (false, true):
            {
                parameters = parameters.Skip(1).ToList();
                break;
            }
        }

        for (var i = 0; i < parameters.Count; i++)
        {
            var parameter = parameters[i];
            sb.Append(parameter.Name);
            if (i < parameters.Count - 1)
            {
                sb.Append(", ");
            }
        }

        sb.Append(')');
        return sb.ToString();
    }

    private CompletionItem GetSignatureCompletionItem(string label, LuaSignature signature, bool colonDefine)
    {
        var completionItem = new CompletionItem
        {
            Label = label,
            Kind = CompletionItemKind.Method,
            LabelDetails = new CompletionItemLabelDetails()
            {
                Detail = RenderSignatureParams(signature, colonDefine),
                Description =
                    CompleteContext.RenderBuilder.RenderType(signature.ReturnType, CompleteContext.RenderFeature)
            },
            InsertText = InsertText,
            Data = Data,
            Command = Command,
            TextEdit = TextOrReplaceEdit
        };

        if (CompleteContext.CompletionConfig.CallSnippet)
        {
            completionItem = completionItem with
            {
                InsertText = RenderInsertTextFuncParams(label, signature, colonDefine),
                InsertTextFormat = InsertTextFormat.Snippet,
            };
        }

        if (IsDeprecated)
        {
            completionItem = completionItem with
            {
                Tags = [CompletionItemTag.Deprecated]
            };
        }

        return completionItem;
    }
}