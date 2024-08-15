using EmmyLua.CodeAnalysis.Compilation.Search;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.LanguageServer.Framework.Protocol.Message.DocumentSymbol;
using EmmyLua.LanguageServer.Server;
using EmmyLua.LanguageServer.Util;

namespace EmmyLua.LanguageServer.WorkspaceSymbol;

public class WorkspaceSymbolBuilder
{
    public List<Framework.Protocol.Message.WorkspaceSymbol.WorkspaceSymbol> Build(string query,
        ServerContext context, CancellationToken cancellationToken)
    {
        var result = new List<Framework.Protocol.Message.WorkspaceSymbol.WorkspaceSymbol>();
        try
        {
            var luaProject = context.LuaProject;
            var searchContext = new SearchContext(luaProject.Compilation, new SearchContextFeatures());
            var namedElements = context.LuaProject.Compilation.Db.QueryNamedElements(searchContext);
            foreach (var pair in namedElements)
            {
                var name = pair.Item1;
                if (name.StartsWith(query, StringComparison.OrdinalIgnoreCase))
                {
                    cancellationToken.ThrowIfCancellationRequested();
                    var elementIds = pair.Item2;
                    foreach (var elementId in elementIds)
                    {
                        var document = luaProject.GetDocument(elementId.DocumentId);
                        if (document is not null)
                        {
                            var ptr = new LuaElementPtr<LuaSyntaxElement>(elementId);
                            if (ptr.ToNode(document) is { } node)
                            {
                                var location = node.Location.ToLspLocation();
                                var declaration = searchContext.FindDeclaration(node);
                                result.Add(new Framework.Protocol.Message.WorkspaceSymbol.WorkspaceSymbol()
                                {
                                    Name = name,
                                    Kind = ToSymbolKind(declaration),
                                    Location = location
                                });
                            }
                        }
                    }
                }
            }

            return result;
        }
        catch (OperationCanceledException)
        {
            return result;
        }
    }

    private static SymbolKind ToSymbolKind(LuaSymbol? luaSymbol)
    {
        return luaSymbol?.Info switch
        {
            MethodInfo => SymbolKind.Method,
            ParamInfo => SymbolKind.TypeParameter,
            NamedTypeInfo => SymbolKind.Class,
            _ => SymbolKind.Variable
        };
    }
}