using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Index;

public class ProjectIndex(LuaCompilation compilation)
{
    private LuaCompilation Compilation { get; } = compilation;

    public IndexStorage<string, Declaration> NameDeclaration { get; } = new();

    public IndexStorage<string, Declaration> Members { get; } = new();

    public IndexStorage<string, Declaration> GlobalDeclaration { get; } = new();

    public IndexStorage<string, LuaType> Supers { get; } = new();

    public IndexStorage<string, NamedTypeDeclaration> NamedType { get; } = new();

    public IndexStorage<string, GenericParameterDeclaration> GenericParam { get; } = new();

    public TypeIndex TypeIndex { get; } = new(compilation);

    public void Remove(DocumentId documentId)
    {
        NameDeclaration.Remove(documentId);
        Members.Remove(documentId);
        GlobalDeclaration.Remove(documentId);
        Supers.Remove(documentId);
        NamedType.Remove(documentId);
        GenericParam.Remove(documentId);
        TypeIndex.Remove(documentId);
    }
}
