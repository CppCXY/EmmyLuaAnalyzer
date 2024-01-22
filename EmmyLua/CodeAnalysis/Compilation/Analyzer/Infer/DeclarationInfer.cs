using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.Infer;

public static class DeclarationInfer
{
    public static SymbolTree? GetSymbolTree(LuaSyntaxElement element, SearchContext context)
    {
        return context.Compilation.GetSymbolTree(element.Tree.Document.Id);
    }

    public static ILuaType InferLocalName(LuaLocalNameSyntax localName, SearchContext context)
    {
        var declarationTree = GetSymbolTree(localName, context);
        if (declarationTree is null)
        {
            return context.Compilation.Builtin.Unknown;
        }

        var declaration = declarationTree.FindDeclaration(localName);
        return declaration?.FirstSymbol.Type ?? context.Compilation.Builtin.Unknown;
    }

    public static ILuaType InferSource(LuaSourceSyntax source, SearchContext context)
    {
        if (source.Block is null) return context.Compilation.Builtin.Unknown;
        var expr = context.Compilation.StubIndexImpl.BlockReturns.Get(source.Block).FirstOrDefault();
        return expr is null ? context.Compilation.Builtin.Unknown : context.Infer(expr.FirstOrDefault());
    }

    public static ILuaType InferParam(LuaParamDefSyntax paramDef, SearchContext context)
    {
        var declarationTree = GetSymbolTree(paramDef, context);
        if (declarationTree is null)
        {
            return context.Compilation.Builtin.Unknown;
        }

        var declaration = declarationTree.FindDeclaration(paramDef);
        return declaration?.FirstSymbol.Type ?? context.Compilation.Builtin.Unknown;
    }
}
