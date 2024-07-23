using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.LanguageServer.Framework.Protocol.Model;
using EmmyLua.LanguageServer.Framework.Protocol.Model.TextEdit;

namespace EmmyLua.LanguageServer.ExecuteCommand.Commands;

public class DiagnosticAction : ICommandBase
{
    private static readonly string CommandName = "emmy.diagnosticAction";

    public string Name { get; } = CommandName;

    public async Task ExecuteAsync(List<LSPAny>? parameters, CommandExecutor executor)
    {
        if (parameters is null or { Count: < 3 })
        {
            return;
        }

        var codeName = parameters[0].Value is string name ? name : string.Empty;
        var action = parameters[1].Value is string actionName ? actionName : string.Empty;
        var documentId = LuaDocumentId.VirtualDocumentId;
        if (parameters[2].Value is int id)
        {
            documentId = new LuaDocumentId(id);
        }

        var document = executor.Context.LuaProject.GetDocument(documentId);
        if (document is null)
        {
            return;
        }

        var offset = 0;
        if (parameters[3].Value is string pos)
        {
            var parts = pos.Split('_').Select(int.Parse).ToList();
            offset = document.GetOffset(parts[0], parts[1]);
        }

        switch (action)
        {
            case "disable-next-line":
            {
                var token = document.SyntaxTree.SyntaxRoot.TokenAt(offset);
                var stat = token?.Ancestors.OfType<LuaStatSyntax>().FirstOrDefault();
                if (stat is not null && codeName.Length > 0)
                {
                    await ApplyStatDisableAsync(document, stat, codeName, executor);
                }

                break;
            }
            case "disable":
            {
                if (codeName.Length > 0)
                {
                    await ApplyDisableFileAsync(document, codeName, executor);
                }

                break;
            }
        }
    }

    private async Task ApplyStatDisableAsync(LuaDocument document, LuaStatSyntax stat, string codeName,
        CommandExecutor executor)
    {
        var tagDiagnostic = stat.Comments
            .FirstOrDefault()?
            .DocList.OfType<LuaDocTagDiagnosticSyntax>()
            .FirstOrDefault();
        if (tagDiagnostic is { Diagnostics.Range: { } prevRange, Action.Text: "disable-next-line" })
        {
            var line = document.GetLine(prevRange.EndOffset);
            var col = document.GetCol(prevRange.EndOffset);
            await executor.ApplyEditAsync(document.Uri, new TextEdit()
            {
                NewText = $", {codeName}",
                Range = new ()
                {
                    Start = new Position() { Line = line, Character = col },
                    End = new Position() { Line = line, Character = col }
                }
            });
        }
        else
        {
            var indentText = string.Empty;
            if (stat.GetPrevToken() is LuaWhitespaceToken { RepresentText: { } indentText2 })
            {
                indentText = indentText2;
            }

            var line = document.GetLine(stat.Range.StartOffset);
            var col = document.GetCol(stat.Range.StartOffset);
            await executor.ApplyEditAsync(document.Uri, new TextEdit()
            {
                NewText = $"---@diagnostic disable-next-line: {codeName}\n{indentText}",
                Range = new ()
                {
                    Start = new Position() { Line = line, Character = col },
                    End = new Position() { Line = line, Character = col }
                }
            });
        }
    }

    private async Task ApplyDisableFileAsync(LuaDocument document, string codeName, CommandExecutor executor)
    {
        var firstChild = document.SyntaxTree.SyntaxRoot.Block?.ChildrenWithTokens.FirstOrDefault();
        if (firstChild is LuaCommentSyntax commentSyntax)
        {
            var tagDiagnostic = commentSyntax.DocList.OfType<LuaDocTagDiagnosticSyntax>().FirstOrDefault();
            if (tagDiagnostic is not null)
            {
                if (tagDiagnostic is { Diagnostics.Range: { } prevRange, Action.Text: "disable" })
                {
                    var line = document.GetLine(prevRange.EndOffset);
                    var col = document.GetCol(prevRange.EndOffset);
                    await executor.ApplyEditAsync(document.Uri, new TextEdit()
                    {
                        NewText = $", {codeName}",
                        Range = new ()
                        {
                            Start = new Position() { Line = line, Character = col },
                            End = new Position() { Line = line, Character = col }
                        }
                    });
                    return;
                }
            }
        }

        await executor.ApplyEditAsync(document.Uri, new TextEdit()
        {
            NewText = $"---@diagnostic disable: {codeName}\n",
            Range = new ()
            {
                Start = new Position() { Line = 0, Character = 0 },
                End = new Position() { Line = 0, Character = 0 }
            }
        });
    }

    public static Command MakeCommand(string title, string codeName, string action, LuaDocumentId documentId,
        DocumentRange range)
    {
        return new Command()
        {
            Title = title,
            Name = CommandName,
            Arguments = [codeName, action, documentId.Id, $"{range.Start.Line}_{range.Start.Character}"]
        };
    }
}