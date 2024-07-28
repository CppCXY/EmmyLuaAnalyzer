using EmmyLua.CodeAnalysis.Type;
using EmmyLua.LanguageServer.Framework.Protocol.Message.DocumentSymbol;
using EmmyLua.LanguageServer.Server;

namespace EmmyLua.LanguageServer.WorkspaceSymbol;

public class WorkspaceSymbolBuilder
{
    public List<Framework.Protocol.Message.WorkspaceSymbol.WorkspaceSymbol> Build(string query,
        ServerContext context, CancellationToken cancellationToken)
    {
        var result = new List<Framework.Protocol.Message.WorkspaceSymbol.WorkspaceSymbol>();
        try
        {
            // var luaProject = context.LuaProject;
            // var searchContext = new SearchContext(luaProject.Compilation, new SearchContextFeatures());
            // var globals = context.LuaProject.Compilation.TypeManager.GetAllGlobalInfos();
            // foreach (var global in globals)
            // {
            //     if (global.Name.StartsWith(query, StringComparison.OrdinalIgnoreCase))
            //     {
            //         cancellationToken.ThrowIfCancellationRequested();
            //         var location = global.GetLocation(searchContext)?.ToLspLocation();
            //         if (location is not null)
            //         {
            //             result.Add(new Framework.Protocol.Message.WorkspaceSymbol.WorkspaceSymbol()
            //             {
            //                 Name = global.Name,
            //                 Kind = ToSymbolKind(global.Type),
            //                 Location = location
            //             });
            //         }
            //     }
            // }
            //
            // var members = context.LuaProject.Compilation.Db.QueryAllMembers();
            // foreach (var member in members)
            // {
            //     if (member.Name.StartsWith(query, StringComparison.OrdinalIgnoreCase))
            //     {
            //         cancellationToken.ThrowIfCancellationRequested();
            //         var document = luaProject.GetDocument(member.DocumentId);
            //         if (document is not null && member.Info.Ptr.ToNode(document) is { } node)
            //         {
            //             result.Add(new Framework.Protocol.Message.WorkspaceSymbol.WorkspaceSymbol()
            //             {
            //                 Name = member.Name,
            //                 Kind = ToSymbolKind(member.Type),
            //                 Location = node.Range.ToLspLocation(document)
            //             });
            //         }
            //     }
            // }

            return result;
        }
        catch (OperationCanceledException)
        {
            return result;
        }
    }

    private static SymbolKind ToSymbolKind(LuaType? type)
    {
        return type switch
        {
            LuaNamedType => SymbolKind.Variable,
            LuaMethodType => SymbolKind.Method,
            _ => SymbolKind.Variable
        };
    }
}