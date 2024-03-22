using System.Diagnostics;
using EmmyLua.CodeAnalysis.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using Range = OmniSharp.Extensions.LanguageServer.Protocol.Models.Range;
using LuaDiagnostic = EmmyLua.CodeAnalysis.Compile.Diagnostic.Diagnostic;

namespace LanguageServer.ExtensionUtil;

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
            Code = diagnostic.Code.ToString(),
            Message = diagnostic.Message,
            Tags = diagnostic.Tag switch
            {
                EmmyLua.CodeAnalysis.Compile.Diagnostic.DiagnosticTag.Unnecessary =>
                    new[] { DiagnosticTag.Unnecessary },
                EmmyLua.CodeAnalysis.Compile.Diagnostic.DiagnosticTag.Deprecated => new[] { DiagnosticTag.Deprecated },
                _ => Array.Empty<DiagnosticTag>()
            },
            Range = diagnostic.Range.ToLspRange(document),
            Severity = diagnostic.Severity switch
            {
                EmmyLua.CodeAnalysis.Compile.Diagnostic.DiagnosticSeverity.Error => DiagnosticSeverity.Error,
                EmmyLua.CodeAnalysis.Compile.Diagnostic.DiagnosticSeverity.Warning => DiagnosticSeverity.Warning,
                EmmyLua.CodeAnalysis.Compile.Diagnostic.DiagnosticSeverity.Information =>
                    DiagnosticSeverity.Information,
                EmmyLua.CodeAnalysis.Compile.Diagnostic.DiagnosticSeverity.Hint => DiagnosticSeverity.Hint,
                _ => throw new UnreachableException()
            },
            Source = "EmmyLua"
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
}