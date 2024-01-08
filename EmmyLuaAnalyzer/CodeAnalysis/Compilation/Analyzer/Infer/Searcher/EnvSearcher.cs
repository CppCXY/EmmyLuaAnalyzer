using EmmyLuaAnalyzer.CodeAnalysis.Compilation.Type;

namespace EmmyLuaAnalyzer.CodeAnalysis.Compilation.Analyzer.Infer.Searcher;

public class EnvSearcher : LuaSearcher
{
    private Stack<Dictionary<string, ILuaType>> _envStack = new();

    public override IEnumerable<ILuaType> SearchType(string className, SearchContext context)
    {
        var env = _envStack.Peek();
        if (env.TryGetValue(className, out var ty))
        {
            yield return ty;
        }
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
