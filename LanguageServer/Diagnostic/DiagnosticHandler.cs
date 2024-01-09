using System.Diagnostics;
using EmmyLua.CodeAnalysis.Workspace;
using OmniSharp.Extensions.LanguageServer.Protocol.Client.Capabilities;
using OmniSharp.Extensions.LanguageServer.Protocol.Document;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using LuaDiagnostic = EmmyLua.CodeAnalysis.Compile.Diagnostic.Diagnostic;

namespace LanguageServer.Diagnostic;

// ReSharper disable once ClassNeverInstantiated.Global
public class DiagnosticHandler(LuaWorkspace workspace) : DocumentDiagnosticHandlerBase
{
    protected override DiagnosticsRegistrationOptions CreateRegistrationOptions(DiagnosticClientCapabilities capability,
        ClientCapabilities clientCapabilities)
    {
        return new()
        {
            Identifier = "EmmyLua",
            InterFileDependencies = false,
            WorkspaceDiagnostics = false
        };
    }

    public override Task<RelatedDocumentDiagnosticReport> Handle(DocumentDiagnosticParams request,
        CancellationToken cancellationToken)
    {
        var compilation = workspace.Compilation;
        var document = workspace.GetDocument(request.TextDocument.Uri.ToUnencodedString());
        RelatedDocumentDiagnosticReport report;
        if (document is not null)
        {
            var luaDiagnostics = compilation.GetDiagnostic(document.Id);
            var diagnostics = luaDiagnostics.Select(it => it.ToLspDiagnostic(document)).ToList();
            report = new RelatedFullDocumentDiagnosticReport()
            {
                Items = Container.From(diagnostics),
                ResultId = document.Id.Guid
            };
        }
        else
        {
            report = new RelatedFullDocumentDiagnosticReport();
        }

        return Task.FromResult(report);
    }
}

public static class DiagnosticExtensions
{
    public static OmniSharp.Extensions.LanguageServer.Protocol.Models.Diagnostic ToLspDiagnostic(
        this LuaDiagnostic diagnostic, LuaDocument document)
    {
        return new()
        {
            Code = diagnostic.Code.ToString(),
            Message = diagnostic.Message,
            Range = new()
            {
                Start = new Position()
                {
                    Line = document.GetLine(diagnostic.Range.StartOffset),
                    Character = document.GetCol(diagnostic.Range.StartOffset)
                },
                End = new Position()
                {
                    Line = document.GetLine(diagnostic.Range.EndOffset),
                    Character = document.GetCol(diagnostic.Range.EndOffset)
                }
            },
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
}