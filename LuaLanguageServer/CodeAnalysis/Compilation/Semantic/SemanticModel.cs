using System.Diagnostics;
using LuaLanguageServer.CodeAnalysis.Compilation.Symbol;
using LuaLanguageServer.CodeAnalysis.Compilation.Type;
using LuaLanguageServer.CodeAnalysis.Kind;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;
using LuaLanguageServer.CodeAnalysis.Syntax.Tree;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Semantic;

public class SemanticModel
{
    private LuaCompilation _compilation;

    private LuaSyntaxTree _tree;

    public SemanticModel(LuaCompilation compilation, LuaSyntaxTree tree)
    {
        _compilation = compilation;
        _tree = tree;
    }

    public ILuaSymbol GetSymbol(LuaSyntaxElement element)
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
