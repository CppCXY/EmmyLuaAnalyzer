using EmmyLua.CodeAnalysis.Compilation.Search;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;

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

    public bool IsPackage { get; }

    public ILocation? GetLocation(SearchContext context);

    public string RelationInformation { get; }

    public IDeclaration Instantiate(TypeSubstitution substitution);

    public LuaDocumentId DocumentId { get; }

    public SyntaxElementId UniqueId { get; }
}
