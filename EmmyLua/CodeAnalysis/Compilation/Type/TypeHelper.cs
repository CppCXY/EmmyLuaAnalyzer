namespace EmmyLua.CodeAnalysis.Compilation.Type;

public static class TypeHelper
{
    public static void Each(LuaType type, Action<LuaType> action)
    {
        switch (type)
        {
            case LuaUnionType unionType:
            {
                foreach (var t in unionType.Types)
                {
                    action(t);
                }

                break;
            }
            default:
            {
                action(type);
                break;
            }
        }
    }
}
