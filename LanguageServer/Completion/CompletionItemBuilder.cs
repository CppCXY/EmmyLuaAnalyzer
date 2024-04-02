using System.Text;
using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Compilation.Semantic;
using EmmyLua.CodeAnalysis.Compilation.Semantic.Render;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace LanguageServer.Completion;

public class CompletionItemBuilder
{
    private string Label { get; set; }

    private string? InsertText { get; set; }

    private CompletionItemKind Kind { get; set; } = CompletionItemKind.Variable;

    private LuaType Type { get; set; }

    private string? Data { get; set; }
    
    private Command? Command { get; set; }

    private TextEditOrInsertReplaceEdit? TextOrReplaceEdit { get; set; }
    
    private bool Colon { get; set; } = false;

    private bool Disable { get; set; } = false;
    
    private SearchContext Context => SemanticModel.Context;
    
    private SemanticModel SemanticModel { get; }

    public static CompletionItemBuilder Create(string label, LuaType? type, SemanticModel semanticModel)
    {
        return new CompletionItemBuilder(label, type ?? Builtin.Any, semanticModel);
    }

    private CompletionItemBuilder(string label, LuaType type,SemanticModel semanticModel)
    {
        Label = label;
        Type = type;
        SemanticModel = semanticModel;
    }

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

    public IEnumerable<CompletionItem> Build()
    {
        if (Disable)
        {
            yield break;
        }
        
        switch (Type)
        {
            case LuaMethodType methodType:
            {
                var mainSignature = methodType.MainSignature;
                yield return new CompletionItem
                {
                    Label = Label,
                    Kind = CompletionItemKind.Method,
                    LabelDetails = new CompletionItemLabelDetails()
                    {
                        Detail = RenderSignatureParams(mainSignature, methodType.ColonDefine),
                        Description = LuaTypeRender.RenderType(mainSignature.ReturnType, Context)
                    },
                    InsertText = RenderInsertTextFuncParams(Label,mainSignature,methodType.ColonDefine),
                    InsertTextFormat = InsertTextFormat.Snippet,
                    Data = Data,
                    Command = Command,
                    TextEdit = TextOrReplaceEdit
                };

                if (methodType.Overloads is not null)
                {
                    foreach (var signature in methodType.Overloads)
                    {
                        yield return new CompletionItem
                        {
                            Label = Label,
                            Kind = CompletionItemKind.Method,
                            LabelDetails = new CompletionItemLabelDetails()
                            {
                                Detail = RenderSignatureParams(signature, methodType.ColonDefine),
                                Description = LuaTypeRender.RenderType(signature.ReturnType, Context),
                            },
                            InsertText = RenderInsertTextFuncParams(Label,signature,methodType.ColonDefine),
                            InsertTextFormat = InsertTextFormat.Snippet,
                            Data = Data,
                            Command = Command,
                            TextEdit = TextOrReplaceEdit
                        };
                    }
                }

                break;
            }
            default:
            {
                if (!Colon)
                {
                    yield return new CompletionItem()
                    {
                        Label = Label,
                        Kind = Kind,
                        LabelDetails = new CompletionItemLabelDetails()
                        {
                            Description = LuaTypeRender.RenderType(Type, Context),
                        },
                        InsertText = InsertText,
                        Data = Data,
                        Command = Command,
                        TextEdit = TextOrReplaceEdit
                    };
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
        int paramsCnt = 1;
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
}