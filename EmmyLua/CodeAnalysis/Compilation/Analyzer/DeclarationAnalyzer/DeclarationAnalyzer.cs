﻿using EmmyLua.CodeAnalysis.Compilation.Analyzer.AttachDocAnalyzer;
using EmmyLua.CodeAnalysis.Compilation.Search;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.DeclarationAnalyzer;

public class DeclarationAnalyzer(LuaCompilation compilation) : LuaAnalyzer(compilation, "Declaration")
{
    public override void Analyze(AnalyzeContext analyzeContext)
    {
        foreach (var document in analyzeContext.LuaDocuments)
        {
            var builder = new DeclarationBuilder(document, Compilation, analyzeContext);
            var walker = new DeclarationWalker.DeclarationWalker(builder, Compilation);
            document.SyntaxTree.SyntaxRoot.Accept(walker);

            var tree = builder.Build();
            if (tree is not null)
            {
                Compilation.ProjectIndex.AddDeclarationTree(document.Id, tree);
            }
        }
    }
}
