using LuaLanguageServer.CodeAnalysis.Compilation.Type;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;
using LuaLanguageServer.CodeAnalysis.Workspace;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Declaration;

public class DelayAnalyzeNode(
    LuaSyntaxNode node,
    DocumentId documentId,
    ILuaType? luaType,
    Declaration? prev,
    DeclarationScope? scope
    )
{
    public LuaSyntaxNode Node => node;

    public DocumentId DocumentId => documentId;

    public ILuaType? LuaType => luaType;

    public Declaration? Prev => prev;

    public DeclarationScope? Scope => scope;
}
