﻿using EmmyLua.CodeAnalysis.Compile;
using EmmyLua.CodeAnalysis.Compile.Lexer;
using EmmyLua.CodeAnalysis.Compile.Parser;
using EmmyLua.CodeAnalysis.Compile.Source;
using EmmyLua.CodeAnalysis.Syntax.Binder;
using EmmyLua.CodeAnalysis.Syntax.Green;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Workspace;

namespace EmmyLua.CodeAnalysis.Syntax.Tree;

public class LuaSyntaxTree
{
    public LuaSource Source { get; }

    public GreenNode GreenRoot { get; }

    public List<Compile.Diagnostic.Diagnostic> Diagnostics { get; }

    private LuaSourceSyntax? _root;

    public BinderData? BinderData { get; private set; }

    public static LuaSyntaxTree ParseText(string text, LuaLanguage language)
    {
        var source = LuaSource.From(text, language);
        return Create(source);
    }

    public static LuaSyntaxTree ParseText(string text)
    {
        return ParseText(text, LuaLanguage.Default);
    }

    public static LuaSyntaxTree Create(LuaSource source)
    {
        var parser = new LuaParser(new LuaLexer(source));
        var builder = new LuaGreenTreeBuilder(parser);
        var (root, diagnostics) = builder.Build();

        return new LuaSyntaxTree(source, root, diagnostics);
    }

    private LuaSyntaxTree(LuaSource source, GreenNode root, List<Compile.Diagnostic.Diagnostic> diagnostics)
    {
        Source = source;
        GreenRoot = root;
        Diagnostics = diagnostics;
    }

    public LuaSourceSyntax SyntaxRoot
    {
        get
        {
            // ReSharper disable once ConvertIfStatementToNullCoalescingAssignment
            if (_root is null)
            {
                _root = SyntaxFactory.CreateSyntax(GreenRoot, this, null) as LuaSourceSyntax;
                BinderData = BinderAnalysis.Analysis(_root!);
            }

            return _root!;
        }
    }

    public void PushDiagnostic(Compile.Diagnostic.Diagnostic diagnostic)
    {
        Diagnostics.Add(diagnostic);
    }
}
