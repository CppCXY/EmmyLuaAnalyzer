using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Compilation.Type.TypeInfo;
using EmmyLua.CodeAnalysis.Document;

namespace EmmyLua.CodeAnalysis.Compilation.Type;

public class GlobalIndex
{
    private Dictionary<string, LuaGlobalTypeInfo> GlobalInfos { get; } = new();

    private Dictionary<LuaDocumentId, HashSet<string>> GlobalLocations { get; } = new();

    public void Remove(LuaDocumentId documentId, LuaTypeManager typeManager)
    {
        if (GlobalLocations.TryGetValue(documentId, out var globalNames))
        {
            var toBeRemove = new List<string>();
            foreach (var globalName in globalNames)
            {
                if (GlobalInfos.TryGetValue(globalName, out var globalInfo))
                {
                    if (globalInfo.Remove(documentId, typeManager))
                    {
                        GlobalInfos.Remove(globalName);
                        toBeRemove.Add(globalName);
                    }
                }
            }

            if (toBeRemove.Count != 0)
            {
                foreach (var globalName in toBeRemove)
                {
                    globalNames.Remove(globalName);
                }
            }
        }
    }

    public void AddGlobal(string name, LuaSymbol symbol)
    {
        if (GlobalInfos.TryGetValue(name, out var globalInfo))
        {
            globalInfo.DefinedSymbol.TryAdd(symbol.DocumentId, symbol);
        }
        else
        {
            globalInfo = new LuaGlobalTypeInfo()
            {
                Name = name
            };
            globalInfo.DefinedSymbol.TryAdd(symbol.DocumentId, symbol);
            GlobalInfos[name] = globalInfo;
        }

        if (GlobalLocations.TryGetValue(symbol.DocumentId, out var globalNames))
        {
            globalNames.Add(name);
        }
        else
        {
            globalNames = [name];
            GlobalLocations[symbol.DocumentId] = globalNames;
        }
    }

    public void AddGlobalMember(string name, LuaSymbol symbol)
    {
        if (GlobalInfos.TryGetValue(name, out var globalInfo))
        {
            globalInfo.Declarations ??= new Dictionary<string, LuaSymbol>();
            globalInfo.Declarations.TryAdd(symbol.Name, symbol);
        }

        if (GlobalLocations.TryGetValue(symbol.DocumentId, out var globalNames))
        {
            globalNames.Add(name);
        }
        else
        {
            globalNames = [name];
            GlobalLocations[symbol.DocumentId] = globalNames;
        }
    }

    public LuaGlobalTypeInfo? Query(string name)
    {
        return GlobalInfos.GetValueOrDefault(name);
    }

    public IEnumerable<LuaGlobalTypeInfo> QueryAll()
    {
        return GlobalInfos.Values;
    }
}
