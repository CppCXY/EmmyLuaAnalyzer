using System.Diagnostics;
using EmmyLua.CodeAnalysis.Compilation.Analyzer.Declaration;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Tree;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Kind;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Semantic;

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
