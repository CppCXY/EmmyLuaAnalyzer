using EmmyLua.CodeAnalysis.Compilation.Search;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Type;
using EmmyLua.CodeAnalysis.Type.Types;

namespace EmmyLua.CodeAnalysis.Compilation.Infer;

public static class TypeInfer
{
    public static LuaType InferType(LuaDocTypeSyntax typeSyntax, SearchContext context)
    {
        return Builtin.Unknown;
    }
}
