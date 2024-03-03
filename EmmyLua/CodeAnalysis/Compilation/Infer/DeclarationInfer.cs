using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Infer;

public static class DeclarationInfer
{
    public static SymbolTree? GetSymbolTree(LuaSyntaxElement element, SearchContext context)
    {
        return context.Compilation.GetSymbolTree(element.Tree.Document.Id);
    }

    public static LuaType InferLocalName(LuaLocalNameSyntax localName, SearchContext context)
    {
        var declarationTree = GetSymbolTree(localName, context);
        if (declarationTree is null)
        {
            return context.Compilation.Builtin.Unknown;
        }

        var symbol = declarationTree.FindSymbol(localName);
        return symbol?.DeclarationType ?? context.Compilation.Builtin.Unknown;
    }

    public static LuaType InferSource(LuaSourceSyntax source, SearchContext context)
    {
        if (source.Block is null) return context.Compilation.Builtin.Unknown;
        // var expr = context.Compilation.ProjectIndex.MainBlockReturns.Get(source.Block).FirstOrDefault();
        // return expr is null ? context.Compilation.Builtin.Unknown : context.Infer(expr.FirstOrDefault());
        throw new NotImplementedException();
    }

    public static LuaType InferParam(LuaParamDefSyntax paramDef, SearchContext context)
    {
        var symbolTree = GetSymbolTree(paramDef, context);
        if (symbolTree is null)
        {
            return context.Compilation.Builtin.Unknown;
        }

        var symbol = symbolTree.FindSymbol(paramDef);
        return symbol?.DeclarationType ?? context.Compilation.Builtin.Unknown;
    }
}
