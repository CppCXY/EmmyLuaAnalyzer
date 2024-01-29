using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Compilation.TypeOperator;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Stub;

public class Stub(LuaCompilation compilation)
{
    public LuaCompilation Compilation { get; } = compilation;

    public StubIndex<string, Symbol.Symbol> Members { get; } = new();

    public StubIndex<string, ILuaNamedType> NamedTypeIndex { get; } = new();

    public StubIndex<string, Symbol.Symbol> GlobalDeclaration { get; } = new();

    public StubIndex<string, ILuaOperator> TypeOperators { get; } = new();

    public StubIndex<string, ILuaType> Supers { get; } = new();

    public StubIndex<string, Symbol.Symbol> GenericParams { get; } = new();

    public StubIndex<LuaFuncBodySyntax, LuaMethod> Methods { get; } = new();

    public StubIndex<LuaBlockSyntax, List<LuaExprSyntax>> MainBlockReturns { get; } = new();

    public void Remove(DocumentId documentId)
    {
        Members.RemoveStub(documentId);
        NamedTypeIndex.RemoveStub(documentId);
        GlobalDeclaration.RemoveStub(documentId);
        TypeOperators.RemoveStub(documentId);
        Supers.RemoveStub(documentId);
        GenericParams.RemoveStub(documentId);
        Methods.RemoveStub(documentId);
        MainBlockReturns.RemoveStub(documentId);
    }
}
