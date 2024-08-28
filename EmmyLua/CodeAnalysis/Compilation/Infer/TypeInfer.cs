using EmmyLua.CodeAnalysis.Compilation.Search;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Compilation.Type.Types;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Infer;

public static class TypeInfer
{
    public static LuaType InferType(LuaDocTypeSyntax typeSyntax, SearchContext context)
    {
        return Builtin.Unknown;
    }
}
