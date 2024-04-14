using System.Diagnostics;
using EmmyLua.CodeAnalysis.Diagnostics;
using EmmyLua.CodeAnalysis.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Diagnostic = OmniSharp.Extensions.LanguageServer.Protocol.Models.Diagnostic;
using DiagnosticSeverity = OmniSharp.Extensions.LanguageServer.Protocol.Models.DiagnosticSeverity;
using DiagnosticTag = OmniSharp.Extensions.LanguageServer.Protocol.Models.DiagnosticTag;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;
using LuaDiagnostic = EmmyLua.CodeAnalysis.Diagnostics.Diagnostic;

namespace LanguageServer.Util;

public static class LocationExtension
{
    public static Location ToLspLocation(this LuaLocation location)
    {
        return new()
        {
            Uri = location.Document.Uri,
            Range = location.Range.ToLspRange(location.Document)
        };
    }

    public static Diagnostic ToLspDiagnostic(this LuaDiagnostic diagnostic, LuaDocument document)
    {
        return new()
        {
            Code = DiagnosticCodeHelper.GetName(diagnostic.Code),
            Message = diagnostic.Message,
            Tags = diagnostic.Tag switch
            {
                EmmyLua.CodeAnalysis.Diagnostics.DiagnosticTag.Unnecessary =>
                    [DiagnosticTag.Unnecessary],
                EmmyLua.CodeAnalysis.Diagnostics.DiagnosticTag.Deprecated => new[] { DiagnosticTag.Deprecated },
                _ => []
            },
            Range = diagnostic.Range.ToLspRange(document),
            Severity = diagnostic.Severity switch
            {
                EmmyLua.CodeAnalysis.Diagnostics.DiagnosticSeverity.Error => DiagnosticSeverity.Error,
                EmmyLua.CodeAnalysis.Diagnostics.DiagnosticSeverity.Warning => DiagnosticSeverity.Warning,
                EmmyLua.CodeAnalysis.Diagnostics.DiagnosticSeverity.Information =>
                    DiagnosticSeverity.Information,
                EmmyLua.CodeAnalysis.Diagnostics.DiagnosticSeverity.Hint => DiagnosticSeverity.Hint,
                _ => throw new UnreachableException()
            },
            Data = diagnostic.Data,
            Source = "EmmyLua",
        };
    }

    public static Range ToLspRange(this SourceRange range, LuaDocument document)
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
            location.Document.Uri,
            new()
            {
                Range = location.Range.ToLspRange(location.Document),
                NewText = text
            });
    }
    
    public static SourceRange ToSourceRange(this Range range, LuaDocument document)
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