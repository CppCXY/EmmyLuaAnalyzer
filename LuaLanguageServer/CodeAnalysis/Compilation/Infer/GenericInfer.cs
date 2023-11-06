using LuaLanguageServer.CodeAnalysis.Compilation.Type;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Infer;

public static class GenericInfer
{
    public static IGeneric InferGeneric(ILuaNamedType baseType, List<ILuaType> args, SearchContext context)
    {
        if (baseType == context.Compilation.Builtin.Table)
        {
            switch (args.Count)
            {
                case 1:
                    return new PrimitiveGenericTable(context.Compilation.Builtin.Unknown, args[0]);
                case 2:
                    return new PrimitiveGenericTable(args[0], args[1]);
            }
        }
        return new Generic(baseType, args);
    }
}
