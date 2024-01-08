using System.Diagnostics;
using EmmyLuaAnalyzer.CodeAnalysis.Compilation.Analyzer.Declaration;
using EmmyLuaAnalyzer.CodeAnalysis.Syntax.Node;
using EmmyLuaAnalyzer.CodeAnalysis.Syntax.Tree;
using EmmyLuaAnalyzer.CodeAnalysis.Compilation.Type;
using EmmyLuaAnalyzer.CodeAnalysis.Kind;
using EmmyLuaAnalyzer.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLuaAnalyzer.CodeAnalysis.Compilation.Semantic;

public class SemanticModel(LuaCompilation compilation, LuaSyntaxTree tree)
{
    private LuaCompilation _compilation = compilation;

    private LuaSyntaxTree _tree = tree;

    public Declaration GetDeclaration(LuaSyntaxElement element)
    {
        // return element switch
        // {
        //     LuaLocalNameSyntax localName => GetSymbol(localName),
        //     LuaParamDefSyntax paramDef => GetSymbol(paramDef),
        //     LuaFuncStatSyntax funcStat => GetSymbol(funcStat),
        //     LuaSourceSyntax source => GetSymbol(source),
        //     _ => throw new NotImplementedException()
        // };

        throw new NotImplementedException();
    }
}
