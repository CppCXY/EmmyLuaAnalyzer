using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Kind;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using MediatR;
using Newtonsoft.Json.Linq;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;

namespace EmmyLua.LanguageServer.ExecuteCommand.Commands;

public class DiagnosticAction : ICommandBase
{
    private static readonly string CommandName = "emmy.diagnosticAction";

    public string Name { get; } = CommandName;

    public async Task<Unit> ExecuteAsync(JArray? parameters, CommandExecutor executor)
    {
        if (parameters is null or { Count: < 3 })
        {
            return await Unit.Task;
        }

        var codeName = parameters.Value<string>(0);
        var action = parameters.Value<string>(1);
        var documentId = LuaDocumentId.VirtualDocumentId;
        if (parameters.Value<string>(2) is { } id && int.TryParse(id, out var idInt))
        {
            documentId = new LuaDocumentId(idInt);
        }

        var document = executor.Context.LuaWorkspace.GetDocument(documentId);
        if (document is null)
        {
            return await Unit.Task;
        }

        var offset = 0;
        if (parameters.Value<string>(3) is { } pos)
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
                if (stat is not null && codeName is not null)
                {
                    await ApplyStatDisableAsync(document, stat, codeName, executor);
                }

                break;
            }
            case "disable":
            {
                if (codeName is not null)
                {
                    await ApplyDisableFileAsync(document, codeName, executor);
                }

                break;
            }
        }

        return await Unit.Task;
    }

    private async Task ApplyStatDisableAsync(LuaDocument document, LuaStatSyntax stat, string codeName,
        CommandExecutor executor)
    {
        var tagDiagnostic = stat.Comments
            .FirstOrDefault()?
            .DocList.OfType<LuaDocTagDiagnosticSyntax>()
            .FirstOrDefault();
        if (tagDiagnostic is { Diagnostics.Range: { } prevRange, Action.RepresentText: "disable-next-line" })
        {
            var line = document.GetLine(prevRange.EndOffset);
            var col = document.GetCol(prevRange.EndOffset);
            await executor.ApplyEditAsync(document.Uri, new TextEdit()
            {
                NewText = $", {codeName}",
                Range = new Range()
                {
                    Start = new Position() { Line = line, Character = col },
                    End = new Position() { Line = line, Character = col }
                }
            });
        }
        else
        {
            var indentText = string.Empty;
            if (stat.GetPrevToken() is LuaWhitespaceToken { RepresentText: {} indentText2 })
            {
                indentText = indentText2;
            }
            
            var line = document.GetLine(stat.Range.StartOffset);
            var col = document.GetCol(stat.Range.StartOffset);
            await executor.ApplyEditAsync(document.Uri, new TextEdit()
            {
                NewText = $"---@diagnostic disable-next-line: {codeName}\n{indentText}",
                Range = new Range()
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
                if (tagDiagnostic is { Diagnostics.Range: { } prevRange, Action.RepresentText: "disable" })
                {
                    var line = document.GetLine(prevRange.EndOffset);
                    var col = document.GetCol(prevRange.EndOffset);
                    await executor.ApplyEditAsync(document.Uri, new TextEdit()
                    {
                        NewText = $", {codeName}",
                        Range = new Range()
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
            Range = new Range()
            {
                Start = new Position() { Line = 0, Character = 0 },
                End = new Position() { Line = 0, Character = 0 }
            }
        });
        
    }

    public static Command MakeCommand(string title, string codeName, string action, LuaDocumentId documentId,
        Range range)
    {
        return new Command()
        {
            Title = title,
            Name = CommandName,
            Arguments = [codeName, action, documentId.Id, $"{range.Start.Line}_{range.Start.Character}"]
        };
    }
}