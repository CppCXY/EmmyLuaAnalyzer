using LuaLanguageServer.CodeAnalysis.Compilation.Type;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Infer.Searcher;

public class EnvSearcher : LuaSearcher
{
    private Stack<Dictionary<string, ILuaType>> _envStack = new();

    public bool TrySearchClass(string name, SearchContext context, out LuaClass? type)
    {
        foreach (var env in _envStack)
        {
            if (env.TryGetValue(name, out var ty))
            {
                if (ty is LuaClass luaClass)
                {
                    type = luaClass;
                    return true;
                }
            }
        }

        type = null;
        return false;
    }

    public void PushEnv(Dictionary<string, ILuaType> env)
    {
        _envStack.Push(env);
    }

    public void PopEnv()
    {
        _envStack.Pop();
    }
}
