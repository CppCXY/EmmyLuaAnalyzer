using System.Diagnostics;
using LuaLanguageServer.CodeAnalysis.Compilation.Symbol;
using LuaLanguageServer.CodeAnalysis.Kind;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;
using LuaLanguageServer.CodeAnalysis.Syntax.Tree;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Semantic;

public class SemanticModel
{
    private LuaCompilation _compilation;

    private LuaSyntaxTree _tree;

    public SemanticModel(LuaCompilation compilation, LuaSyntaxTree tree)
    {
        _compilation = compilation;
        _tree = tree;
    }

    public ILuaSymbol GetSymbol(LuaSyntaxNodeOrToken nodeOrToken)
    {
        return nodeOrToken switch
        {
            LuaSyntaxNodeOrToken.Node node => GetSymbol(node),
            LuaSyntaxNodeOrToken.Token token => GetSymbol(token),
            _ => throw new UnreachableException()
        };
    }

    public ILuaSymbol GetSymbol(LuaSyntaxNode node)
    {
        switch (node)
        {
            case LuaIndexExprSyntax indexExprSyntax:
            {
                break;
            }
            case LuaNameSyntax nameSyntax:
            {
                break;
            }
            default:
            {
                break;
            }
        }

        throw new NotImplementedException();
    }

    public ILuaSymbol GetSymbol(LuaSyntaxToken token)
    {
        switch (token.Kind)
        {
            case LuaTokenKind.TkName:
            {
                break;
            }
            case LuaTokenKind.TkString:
            {
                break;
            }
            default:
            {
                break;
            }
        }

        throw new NotImplementedException();
    }
}
