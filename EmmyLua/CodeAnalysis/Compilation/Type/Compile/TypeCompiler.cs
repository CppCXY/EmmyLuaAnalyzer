using EmmyLua.CodeAnalysis.Compilation.Type.Types;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Type.Compile;

public static class TypeCompiler
{
    public static LuaType? Compile(LuaDocTypeSyntax typeSyntax, LuaCommentSyntax commentSyntax, TypeContext context)
    {
        var stack = new Stack<LuaType>();
        CompileType(typeSyntax, commentSyntax, context, stack);

        if (stack.Count == 0)
        {
            return null;
        }

        return stack.Peek();
    }


    private static void CompileType(LuaDocTypeSyntax typeSyntax, LuaCommentSyntax commentSyntax, TypeContext context,
        Stack<LuaType> stack)
    {
        switch (typeSyntax)
        {
            case LuaDocGenericTypeSyntax genericTypeSyntax:
            {
                CompileGenericType(genericTypeSyntax, commentSyntax, context, stack);
                break;
            }
            case LuaDocNameTypeSyntax nameTypeSyntax:
            {
                CompileNameType(nameTypeSyntax, commentSyntax, context, stack);
                break;
            }
            // case LuaDocUnionTypeSyntax unionTypeSyntax:
            // {
            //     CompileUnionType(unionTypeSyntax, commentSyntax, context, stack);
            //     break;
            // }
            // case LuaDocArrayTypeSyntax arrayTypeSyntax:
            // {
            //     CompileArrayType(arrayTypeSyntax, commentSyntax, context, stack);
            //     break;
            // }
        }
    }

    private static void CompileGenericType(LuaDocGenericTypeSyntax genericTypeSyntax, LuaCommentSyntax commentSyntax,
        TypeContext context, Stack<LuaType> stack)
    {
        // var type = context.FindType(genericTypeSyntax.Name);
        // if (type is not null)
        // {
        //     stack.Push(type);
        // }
    }

    private static void CompileNameType(LuaDocNameTypeSyntax nameTypeSyntax, LuaCommentSyntax commentSyntax,
        TypeContext context, Stack<LuaType> stack)
    {
        if (nameTypeSyntax.Name?.RepresentText is { } name)
        {
            var type = context.FindType(name, commentSyntax);
            if (type is not null)
            {
                stack.Push(type);
            }
            else
            {
                // nameTypeSyntax
            }
        }

        // var type = context.FindType(nameTypeSyntax.Name);
        // if (type is not null)
        // {
        //     stack.Push(type);
        // }
    }
}
