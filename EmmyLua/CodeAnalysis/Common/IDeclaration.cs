using EmmyLua.CodeAnalysis.Type;

namespace EmmyLua.CodeAnalysis.Common;

public interface IDeclaration
{
    public string Name { get; }

    public LuaType Type { get; }

    public bool IsDeprecated { get; }

    public bool IsLocal { get; }

    public bool IsGlobal { get; }

    public bool IsAsync { get; }

    public bool IsNoDiscard { get; }

    public bool IsPublic { get; }

    public bool IsProtected { get; }

    public bool IsPrivate { get; }

    public ILocation? GetLocation(IDocumentSystem context);

    public string RelationInfomation { get; }

    public IDeclaration Instantiate(Dictionary<string, LuaType> typeDict);
}
