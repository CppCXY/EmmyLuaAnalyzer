using EmmyLua.CodeAnalysis.Compilation.Type.Types;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Type.Compile;

public class TypeCompiler
{
    public static LuaType? Compile(LuaDocTypeSyntax typeSyntax, TypeContext context)
    {
        var stack = new Stack<LuaType>();


        return null;
    }

    private static void CompileType(LuaDocTypeSyntax typeSyntax, TypeContext context, Stack<LuaType> stack)
    {
        switch (typeSyntax)
        {
            case LuaDocGenericTypeSyntax genericTypeSyntax:
            {
                CompileGenericType(genericTypeSyntax, context, stack);
                break;
            }
            case LuaDocNameTypeSyntax nameTypeSyntax:
            {
                CompileNameType(nameTypeSyntax, context, stack);
                break;
            }
            case LuaDocUnionTypeSyntax unionTypeSyntax:
            {
                CompileUnionType(unionTypeSyntax, context, stack);
                break;
            }
            case LuaDocArrayTypeSyntax arrayTypeSyntax:
            {
                CompileArrayType(arrayTypeSyntax, context, stack);
                break;
            }
        }
    }

    private static void CompileGenericType(LuaDocGenericTypeSyntax genericTypeSyntax, TypeContext context, Stack<LuaType> stack)
    {
        // var type = context.FindType(genericTypeSyntax.Name);
        // if (type is not null)
        // {
        //     stack.Push(type);
        // }
    }

    private static void CompileNameType(LuaDocNameTypeSyntax nameTypeSyntax, TypeContext context, Stack<LuaType> stack)
    {
        if (nameTypeSyntax.Name?.RepresentText is { } name)
        {
            var type = context.FindType(name);
            if (type is not null)
            {
                stack.Push(type);
            }
        }

        // var type = context.FindType(nameTypeSyntax.Name);
        // if (type is not null)
        // {
        //     stack.Push(type);
        // }
    }
}
