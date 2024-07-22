using System.Diagnostics;
using EmmyLua.CodeAnalysis.Common;
using EmmyLua.CodeAnalysis.Diagnostics;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.LanguageServer.Framework.Protocol.Model;
using EmmyLua.LanguageServer.Framework.Protocol.Model.TextEdit;
using LuaDiagnostic = EmmyLua.CodeAnalysis.Diagnostics.Diagnostic;
using Diagnostic = EmmyLua.LanguageServer.Framework.Protocol.Model.Diagnostic;
using DiagnosticTag = EmmyLua.LanguageServer.Framework.Protocol.Model.Diagnostic.DiagnosticTag;
using LuaDiagnosticTag = EmmyLua.CodeAnalysis.Diagnostics.DiagnosticTag;
using DiagnosticSeverity = EmmyLua.LanguageServer.Framework.Protocol.Model.Diagnostic.DiagnosticSeverity;
using LuaDiagnosticServerity = EmmyLua.CodeAnalysis.Diagnostics.DiagnosticSeverity;

namespace EmmyLua.LanguageServer.Util;

public static class LspExtension
{
    public static Location ToLspLocation(this LuaLocation location)
    {
        return new()
        {
            Uri = location.Uri,
            Range = new()
            {
                Start = new Position()
                {
                    Line = location.StartLine,
                    Character = location.StartCol
                },
                End = new Position()
                {
                    Line = location.EndLine,
                    Character = location.EndCol
                }
            }
        };
    }

    public static Diagnostic.Diagnostic ToLspDiagnostic(this LuaDiagnostic diagnostic, LuaDocument document)
    {
        return new()
        {
            Code = DiagnosticCodeHelper.GetName(diagnostic.Code),
            Message = diagnostic.Message,
            Tags = diagnostic.Tag switch
            {
                LuaDiagnosticTag.Unnecessary =>
                    [DiagnosticTag.Unnecessary],
                LuaDiagnosticTag.Deprecated => [ DiagnosticTag.Deprecated ],
                _ => []
            },
            Range = diagnostic.Range.ToLspRange(document),
            Severity = diagnostic.Severity switch
            {
                LuaDiagnosticServerity.Error => DiagnosticSeverity.Error,
                LuaDiagnosticServerity.Warning => DiagnosticSeverity.Warning,
                LuaDiagnosticServerity.Information =>
                    DiagnosticSeverity.Information,
                LuaDiagnosticServerity.Hint => DiagnosticSeverity.Hint,
                _ => throw new UnreachableException()
            },
            Data = diagnostic.Data,
            Source = "EmmyLua",
        };
    }

    public static DocumentRange ToLspRange(this SourceRange range, LuaDocument document)
    {
        return new()
        {
            Start = new Position()
            {
                Line = document.GetLine(range.StartOffset),
                Character = document.GetCol(range.StartOffset)
            },
            End = new Position()
            {
                Line = document.GetLine(range.EndOffset),
                Character = document.GetCol(range.EndOffset)
            }
        };
    }

    public static DocumentRange ToLspRange(this LuaLocation location)
    {
        return new()
        {
            Start = new Position()
            {
                Line = location.StartLine,
                Character = location.StartCol
            },
            End = new Position()
            {
                Line = location.EndLine,
                Character = location.EndCol
            }
        };
    }

    public static Location ToLspLocation(this SourceRange range, LuaDocument document)
    {
        return new()
        {
            Uri = document.Uri,
            Range = range.ToLspRange(document)
        };
    }

    public static (string, TextEdit) ToTextEdit(this LuaLocation location, string text)
    {
        return (
            location.Uri,
            new()
            {
                Range = location.ToLspRange(),
                NewText = text
            });
    }

    public static SourceRange ToSourceRange(this DocumentRange range, LuaDocument document)
    {
        var start = document.GetOffset(range.Start.Line, range.Start.Character);
        var length = document.GetOffset(range.End.Line, range.End.Character) - start;
        return new()
        {
            StartOffset = start,
            Length = length
        };
    }

    public static Position ToLspPosition(this int pos, LuaDocument document)
    {
        return new()
        {
            Line = document.GetLine(pos),
            Character = document.GetCol(pos)
        };
    }
}