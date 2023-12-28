using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Declaration;
using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Infer;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public class LuaMultiRetType : LuaType
{
    private readonly List<ILuaType> _types = new();

    public LuaMultiRetType(List<ILuaType> rets) : base(TypeKind.MultiRet)
    {
        _types.AddRange(rets);
    }

    public override IEnumerable<Declaration> GetMembers(SearchContext context)
    {
        return Enumerable.Empty<Declaration>();
    }

    public ILuaType? GetRetType(int index)
    {
        return index < _types.Count ? _types[index] : null;
    }
}
