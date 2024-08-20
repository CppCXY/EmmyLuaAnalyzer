using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Type.Types;

namespace EmmyLua.CodeAnalysis.Type;

public class TypeSubstitution
{
    private Dictionary<string, LuaType> TypeMap { get; } = new();

    private Dictionary<string, LuaTypeTemplate> Template { get; } = new();

    private Dictionary<string, List<LuaSymbol>> SpreadParameters { get; } = new();

    public void SetTemplate(Dictionary<string, LuaTypeTemplate> defaultTypeMap)
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

    public void AddSpreadParameter(string name, IEnumerable<LuaSymbol> parameters)
    {
        SpreadParameters[name] = parameters.ToList();
    }

    public bool InferFinished => TypeMap.Count == Template.Count;

    public bool IsGenericParam(string paramName) => Template.ContainsKey(paramName);

    public IEnumerable<LuaSymbol> GetSpreadParameters(string name) =>
        SpreadParameters.GetValueOrDefault(name) ?? Enumerable.Empty<LuaSymbol>();
}
