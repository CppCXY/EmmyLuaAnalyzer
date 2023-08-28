﻿using LuaLanguageServer.LuaCore.Compile.Source;

namespace LuaLanguageServer.LuaCore.Syntax.Diagnostic;

public class Diagnostic
{
    public DiagnosticSeverity Severity { get; }

    public string Message { get; }

    public SourceRange Range { get; }

    public Diagnostic(DiagnosticSeverity severity, string message, SourceRange range)
    {
        Severity = severity;
        Message = message;
        Range = range;
    }
}
