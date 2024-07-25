using EmmyLua.CodeAnalysis.Compilation.Declaration;

namespace EmmyLua.CodeAnalysis.Type;

public class TypeSubstitution
{
    private Dictionary<string, LuaType> TypeMap { get; } = new();

    private Dictionary<string, LuaType> Template { get; } = new();

    private Dictionary<string, List<LuaDeclaration>> SpreadParameters { get; } = new();

    public void SetTemplate(Dictionary<string, LuaType> defaultTypeMap)
    {
        foreach (var (key, value) in defaultTypeMap)
        {
            Template[key] = value;
        }
    }

    public LuaType Substitute(string name, LuaType defaultType)
    {
        return TypeMap.GetValueOrDefault(name, defaultType);
    }

    public LuaType? Substitute(string name)
    {
        return TypeMap.GetValueOrDefault(name);
    }

    public void AnalyzeDefaultType()
    {
        foreach (var (key, value) in Template)
        {
            TypeMap.TryAdd(key, value);
        }
    }

    public void Add(string name, LuaType type, bool force = false)
    {
        if (Template.ContainsKey(name) || force)
        {
            TypeMap[name] = type;
        }
    }

    public void AddSpreadParameter(string name, IEnumerable<LuaDeclaration> parameters)
    {
        SpreadParameters[name] = parameters.ToList();
    }

    public bool InferFinished => TypeMap.Count == Template.Count;

    public bool IsGenericParam(string paramName) => Template.ContainsKey(paramName);

    public IEnumerable<LuaDeclaration> GetSpreadParameters(string name) =>
        SpreadParameters.GetValueOrDefault(name) ?? Enumerable.Empty<LuaDeclaration>();
}
