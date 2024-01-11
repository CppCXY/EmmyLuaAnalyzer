using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Compilation.TypeOperator;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Workspace;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.Stub;

public class StubIndexImpl(LuaCompilation compilation)
{
    public LuaCompilation Compilation { get; } = compilation;

    public StubIndex<string, Declaration.Declaration> ShortNameIndex { get; } = new();

    public StubIndex<string, Declaration.Declaration> Members { get; } = new();

    public StubIndex<string, ILuaNamedType> NamedTypeIndex { get; } = new();

    public StubIndex<string, Declaration.Declaration> GlobalDeclaration { get; } = new();

    public StubIndex<string, ILuaOperator> TypeOperators { get; } = new();

    public StubIndex<string, ILuaType> Supers { get; } = new();

    public StubIndex<string, Declaration.Declaration> GenericParams { get; } = new();

    public StubIndex<LuaSyntaxElement, LuaMethod> Methods { get; } = new();

    public StubIndex<LuaSourceSyntax, LuaExprSyntax> Modules { get; } = new();

    public void Remove(DocumentId documentId)
    {
        ShortNameIndex.RemoveStub(documentId);
        Members.RemoveStub(documentId);
        NamedTypeIndex.RemoveStub(documentId);
        GlobalDeclaration.RemoveStub(documentId);
        TypeOperators.RemoveStub(documentId);
        Supers.RemoveStub(documentId);
        GenericParams.RemoveStub(documentId);
        Methods.RemoveStub(documentId);
        Modules.RemoveStub(documentId);
    }
}
