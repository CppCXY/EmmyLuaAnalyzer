using EmmyLua.CodeAnalysis.Common;

namespace EmmyLua.CodeAnalysis.Compilation.Type;

public class TypeSubstitution
{
    private Dictionary<string, LuaType> TypeMap { get; } = new();

    private Dictionary<string, LuaType> Template { get; } = new();

    private Dictionary<string, List<IDeclaration>> SpreadParameters { get; } = new();

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

    public void AddSpreadParameter(string name, IEnumerable<IDeclaration> parameters)
    {
        SpreadParameters[name] = parameters.ToList();
    }

    public bool InferFinished => TypeMap.Count == Template.Count;

    public bool IsGenericParam(string paramName) => Template.ContainsKey(paramName);

    public IEnumerable<IDeclaration> GetSpreadParameters(string name) =>
        SpreadParameters.GetValueOrDefault(name) ?? Enumerable.Empty<IDeclaration>();
}
