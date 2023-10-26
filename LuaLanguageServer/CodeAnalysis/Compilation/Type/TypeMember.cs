using LuaLanguageServer.CodeAnalysis.Compilation.Infer;
using LuaLanguageServer.CodeAnalysis.Compilation.StubIndex;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public abstract class LuaTypeMember
{
    public ILuaType? ContainingType { get; }

    public LuaTypeMember(ILuaType? containingType)
    {
        ContainingType = containingType;
    }

    public abstract ILuaType? GetType(SearchContext context);

    public abstract bool MatchKey(IndexKey key, SearchContext context);
}

