using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Compilation.TypeOperator;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.Stub;

public class StubIndexImpl(LuaCompilation compilation)
{
    public LuaCompilation Compilation { get; } = compilation;

    public StubIndex<string, Symbol.Symbol> Members { get; } = new();

    public StubIndex<string, ILuaNamedType> NamedTypeIndex { get; } = new();

    public StubIndex<string, Symbol.Symbol> GlobalDeclaration { get; } = new();

    public StubIndex<string, ILuaOperator> TypeOperators { get; } = new();

    public StubIndex<string, ILuaType> Supers { get; } = new();

    public StubIndex<string, Symbol.Symbol> GenericParams { get; } = new();

    public StubIndex<LuaSyntaxElement, LuaMethod> Methods { get; } = new();

    public StubIndex<LuaBlockSyntax, List<LuaExprSyntax>> BlockReturns { get; } = new();

    public void Remove(DocumentId documentId)
    {
        Members.RemoveStub(documentId);
        NamedTypeIndex.RemoveStub(documentId);
        GlobalDeclaration.RemoveStub(documentId);
        TypeOperators.RemoveStub(documentId);
        Supers.RemoveStub(documentId);
        GenericParams.RemoveStub(documentId);
        Methods.RemoveStub(documentId);
        BlockReturns.RemoveStub(documentId);
    }
}
