using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Compilation.Type.TypeInfo;
using EmmyLua.CodeAnalysis.Compilation.Type.Types;
using EmmyLua.CodeAnalysis.Container;
using EmmyLua.CodeAnalysis.Document;

namespace EmmyLua.CodeAnalysis.Compilation.Index;

public class GlobalIndex(LuaCompilation compilation)
{
    private Dictionary<string, LuaGlobalTypeInfo> _globalInfos = new();

    private Dictionary<string, LuaDocumentId> _globalDefinedDocumentIds = new();

    private Dictionary<string, Dictionary<LuaDocumentId, LuaSymbol>> _globalSymbols = new();

    public void Remove(LuaDocumentId documentId)
    {
        var typeManager = compilation.TypeManager;
        var toBeRemoved = new List<string>();
        foreach (var (name, globalInfo) in _globalInfos)
        {
            if (globalInfo.Remove(documentId, typeManager))
            {
                toBeRemoved.Add(name);
            }
        }

        if (toBeRemoved.Count > 0)
        {
            foreach (var name in toBeRemoved)
            {
                _globalInfos.Remove(name);
            }
        }

        toBeRemoved.Clear();

        foreach (var (name, id) in _globalDefinedDocumentIds)
        {
            if (id == documentId)
            {
                _globalDefinedDocumentIds.Remove(name);
                break;
            }
        }


        foreach (var (name, symbols) in _globalSymbols)
        {
            symbols.Remove(documentId);
            if (symbols.Count == 0)
            {
                toBeRemoved.Add(name);
            }
        }

        if (toBeRemoved.Count > 0)
        {
            foreach (var name in toBeRemoved)
            {
                _globalSymbols.Remove(name);
            }
        }
    }

    public void AddGlobal(string name, LuaSymbol symbol)
    {
        if (_globalInfos.TryGetValue(name, out var globalInfo))
        {
            globalInfo.AddDefineId(symbol.UniqueId);
        }
        else
        {
            globalInfo = new LuaGlobalTypeInfo(NamedTypeKind.None, LuaTypeAttribute.Global);
            globalInfo.AddDefineId(symbol.UniqueId);
            _globalInfos[name] = globalInfo;
        }

        if (!_globalSymbols.TryGetValue(name, out var symbols))
        {
            symbols = new();
            _globalSymbols[name] = symbols;
        }

        symbols[symbol.DocumentId] = symbol;
    }

    public void AddDefinedDocumentId(string name, LuaDocumentId documentId)
    {
        _globalDefinedDocumentIds[name] = documentId;
    }

    public void AddGlobalMember(string name, LuaSymbol symbol)
    {
        if (_globalInfos.TryGetValue(name, out var globalInfo))
        {
            globalInfo.AddImplement(symbol);
        }

        // var typeManager = compilation.TypeManager;
        // var globalSymbol = FindGlobalSymbol(name);
        // if (globalSymbol is {Type: LuaNamedType namedType})
        // {
        //     var typeInfo = typeManager.find
        // }
    }

    public LuaSymbol? FindGlobalSymbol(string name)
    {
        if (_globalSymbols.TryGetValue(name, out var symbols))
        {
            if (_globalDefinedDocumentIds.TryGetValue(name, out var documentId))
            {
                if (symbols.TryGetValue(documentId, out var symbol))
                {
                    return symbol;
                }
            }
        }

        return null;
    }

    public LuaGlobalTypeInfo? Query(string name)
    {
        return _globalInfos.GetValueOrDefault(name);
    }

    public IEnumerable<LuaGlobalTypeInfo> QueryAll()
    {
        return _globalInfos.Values;
    }
}
