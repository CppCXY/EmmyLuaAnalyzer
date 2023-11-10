using LuaLanguageServer.CodeAnalysis.Compilation.Type;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Infer.Searcher;

public class EnvSearcher : ILuaSearcher
{
    private Stack<Dictionary<string, ILuaType>> _envStack = new();

    public bool TrySearchType(string name, SearchContext context, out ILuaType type)
    {
        foreach (var env in _envStack)
        {
            if (env.TryGetValue(name, out var ty))
            {
                type = ty;
                return true;
            }
        }

        type = context.Compilation.Builtin.Unknown;
        return false;
    }

    public IEnumerable<LuaTypeMember> SearchMembers(ILuaType type, SearchContext context)
    {
        return Enumerable.Empty<LuaTypeMember>();
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
