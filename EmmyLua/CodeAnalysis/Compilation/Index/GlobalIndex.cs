using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Compilation.Type.TypeInfo;
using EmmyLua.CodeAnalysis.Document;

namespace EmmyLua.CodeAnalysis.Compilation.Index;

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
            globalInfo.AddDefineId(symbol.UniqueId);
        }
        else
        {
            globalInfo = new LuaGlobalTypeInfo(NamedTypeKind.None, LuaTypeAttribute.Global);
            globalInfo.AddDefineId(symbol.UniqueId);
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
            globalInfo.AddImplement(symbol);
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
