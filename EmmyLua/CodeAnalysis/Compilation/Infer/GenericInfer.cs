using EmmyLua.CodeAnalysis.Compilation.Type;

namespace EmmyLua.CodeAnalysis.Compilation.Infer;

public static class GenericInfer
{
    public static IGenericImpl InferGeneric(IGenericBase baseType, List<ILuaType> args, SearchContext context)
    {
        if (baseType == context.Compilation.Builtin.Table)
        {
            switch (args.Count)
            {
                case 1:
                    return new LuaTable(context.Compilation.Builtin.Unknown, args[0]);
                case 2:
                    return new LuaTable(args[0], args[1]);
            }
        }

        return new LuaGenericImpl(baseType, args);
    }
}
