using EmmyLua.CodeAnalysis.Compilation.Type;

namespace EmmyLua.CodeAnalysis.Compilation.Infer.Searcher;

public class EnvSearcher : LuaSearcher
{
    private Stack<Dictionary<string, LuaType>> _envStack = new();

    public override IEnumerable<LuaType> SearchType(string className, SearchContext context)
    {
        if (_envStack.Count == 0)
        {
            yield break;
        }
        var env = _envStack.Peek();
        if (env.TryGetValue(className, out var ty))
        {
            yield return ty;
        }
    }

    public void PushEnv(Dictionary<string, LuaType> env)
    {
        _envStack.Push(env);
    }

    public void PopEnv()
    {
        _envStack.Pop();
    }
}
