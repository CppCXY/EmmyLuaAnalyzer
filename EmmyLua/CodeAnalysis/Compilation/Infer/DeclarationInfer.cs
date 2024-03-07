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
            return Builtin.Unknown;
        }

        var symbol = declarationTree.FindSymbol(localName);
        return symbol?.DeclarationType ?? Builtin.Unknown;
    }

    public static LuaType InferSource(LuaSourceSyntax source, SearchContext context)
    {
        return context.Compilation.ProjectIndex.GetExportType(source.Tree.Document.Id) ?? Builtin.Unknown;
    }

    public static LuaType InferParam(LuaParamDefSyntax paramDef, SearchContext context)
    {
        var symbolTree = GetSymbolTree(paramDef, context);
        if (symbolTree is null)
        {
            return Builtin.Unknown;
        }

        var symbol = symbolTree.FindSymbol(paramDef);
        return symbol?.DeclarationType ?? Builtin.Unknown;
    }
}
