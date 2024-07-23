using EmmyLua.CodeAnalysis.Compilation;
using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Semantic;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.LanguageServer.Framework.Protocol.Message.DocumentSymbol;
using EmmyLua.LanguageServer.Framework.Protocol.Message.TypeHierarchy;
using EmmyLua.LanguageServer.Util;


namespace EmmyLua.LanguageServer.TypeHierarchy;

public class TypeHierarchyBuilder
{
    public List<TypeHierarchyItem>? BuildPrepare(SemanticModel semanticModel, LuaSyntaxNode node)
    {
        if (node is LuaDocTagClassSyntax { Name: { RepresentText: { } name } nameToken })
        {
            var items = new List<TypeHierarchyItem>();
            items.AddRange(BuildSupers(semanticModel.Compilation, name));
            items.AddRange(BuildSubTypes(semanticModel.Compilation, name));
            return items;
        }

        return null;
    }

    public List<TypeHierarchyItem> BuildSupers(LuaCompilation compilation, string name)
    {
        var supers = compilation.Db.QuerySupers(name);
        var items = new List<TypeHierarchyItem>();
        foreach (var super in supers)
        {
            if (super is LuaNamedType superNamedType)
            {
                var typeDefine = compilation.Db.QueryNamedTypeDefinitions(superNamedType.Name).FirstOrDefault();
                if (typeDefine is LuaDeclaration { Info: NamedTypeInfo info })
                {
                    var typeDocument = compilation.Project.GetDocument(info.TypeDefinePtr.DocumentId);
                    if (typeDocument is not null
                        && info.TypeDefinePtr.ToNode(typeDocument) is { Range: { } sourceRange })
                    {
                        var range = sourceRange.ToLspRange(typeDocument);
                        items.Add(new TypeHierarchyItem
                        {
                            Name = $"super {superNamedType.Name}",
                            Kind = ToSymbolKind(info),
                            Uri = typeDocument!.Uri,
                            Range = range,
                            SelectionRange = range,
                            Data = superNamedType.Name
                        });
                    }
                }
            }
        }

        return items;
    }

    public List<TypeHierarchyItem> BuildSubTypes(LuaCompilation compilation, string name)
    {
        var subTypes = compilation.Db.QuerySubTypes(name);
        var items = new List<TypeHierarchyItem>();
        foreach (var subTypeName in subTypes)
        {
            var typeDefine = compilation.Db.QueryNamedTypeDefinitions(subTypeName).FirstOrDefault();
            if (typeDefine is LuaDeclaration { Info: NamedTypeInfo info })
            {
                var typeDocument = compilation.Project.GetDocument(info.TypeDefinePtr.DocumentId);
                if (typeDocument is not null
                    && info.TypeDefinePtr.ToNode(typeDocument) is { Range: { } sourceRange })
                {
                    var range = sourceRange.ToLspRange(typeDocument);
                    items.Add(new TypeHierarchyItem
                    {
                        Name = $"subtype {subTypeName}",
                        Kind = ToSymbolKind(info),
                        Uri = typeDocument!.Uri,
                        Range = range,
                        SelectionRange = range,
                        Data = subTypeName,
                    });
                }
            }
        }

        return items;
    }

    private static SymbolKind ToSymbolKind(NamedTypeInfo info)
    {
        return info.Kind switch
        {
            NamedTypeKind.Class => SymbolKind.Class,
            NamedTypeKind.Interface => SymbolKind.Interface,
            NamedTypeKind.Enum => SymbolKind.Enum,
            _ => SymbolKind.Class
        };
    }
}