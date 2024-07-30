using EmmyLua.CodeAnalysis.Compilation;
using EmmyLua.CodeAnalysis.Compilation.Search;
using EmmyLua.CodeAnalysis.Compilation.Semantic;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Type;
using EmmyLua.LanguageServer.Framework.Protocol.Message.DocumentSymbol;
using EmmyLua.LanguageServer.Framework.Protocol.Message.TypeHierarchy;
using EmmyLua.LanguageServer.Util;


namespace EmmyLua.LanguageServer.TypeHierarchy;

public class TypeHierarchyBuilder
{
    public List<TypeHierarchyItem>? BuildPrepare(SemanticModel semanticModel, LuaSyntaxNode node)
    {
        if (node is LuaDocTagClassSyntax { Name: { RepresentText: { } name } })
        {
            var items = new List<TypeHierarchyItem>();
            var luaNamedType = new LuaNamedType(semanticModel.Document.Id, name);
            items.AddRange(BuildSupers(semanticModel.Compilation, luaNamedType));
            items.AddRange(BuildSubTypes(semanticModel.Compilation, luaNamedType));
            return items;
        }

        return null;
    }

    public List<TypeHierarchyItem> BuildSupers(LuaCompilation compilation, LuaNamedType namedType)
    {
        var context = new SearchContext(compilation, new());
        var typeInfo = compilation.TypeManager.FindTypeInfo(namedType);
        if (typeInfo is null)
        {
            return [];
        }

        var items = new List<TypeHierarchyItem>();
        if (typeInfo.Supers is not null)
        {
            foreach (var super in typeInfo.Supers)
            {
                var superTypeInfo = compilation.TypeManager.FindTypeInfo(super);
                if (superTypeInfo is not null)
                {
                    var typeDocument = compilation.Project.GetDocument(superTypeInfo.MainDocumentId);
                    if (typeDocument is not null && superTypeInfo.GetLocation(context) is {} location)
                    {
                        items.Add(new TypeHierarchyItem
                        {
                            Name = $"super {super.Name}",
                            Kind = ToSymbolKind(superTypeInfo.Kind),
                            Uri = typeDocument.Uri,
                            Range = location.ToLspRange(),
                            SelectionRange = location.ToLspRange(),
                            Data = $"{super.DocumentId.Id.ToString()}|{super.Name}",
                        });
                    }
                }
            }
        }

        return items;
    }

    public List<TypeHierarchyItem> BuildSubTypes(LuaCompilation compilation, LuaNamedType namedType)
    {
        var context = new SearchContext(compilation, new());
        var typeInfo = compilation.TypeManager.FindTypeInfo(namedType);
        if (typeInfo is null)
        {
            return [];
        }
        
        var items = new List<TypeHierarchyItem>();
        if (typeInfo.SubTypes is not null)
        {
            foreach (var subType in typeInfo.SubTypes)
            {
                var subTypeInfo = compilation.TypeManager.FindTypeInfo(subType);
                if (subTypeInfo is not null)
                {
                    var typeDocument = compilation.Project.GetDocument(subTypeInfo.MainDocumentId);
                    if (typeDocument is not null && subTypeInfo.GetLocation(context) is {} location)
                    {
                        items.Add(new TypeHierarchyItem
                        {
                            Name = $"subtype {subType.Name}",
                            Kind = ToSymbolKind(subTypeInfo.Kind),
                            Uri = typeDocument.Uri,
                            Range = location.ToLspRange(),
                            SelectionRange = location.ToLspRange(),
                            Data = $"{subType.DocumentId.Id.ToString()}|{subType.Name}",
                        });
                    }
                }
            }
        }

        return items;
    }

    private static SymbolKind ToSymbolKind(NamedTypeKind kind)
    {
        return kind switch
        {
            NamedTypeKind.Class => SymbolKind.Class,
            NamedTypeKind.Interface => SymbolKind.Interface,
            NamedTypeKind.Enum => SymbolKind.Enum,
            _ => SymbolKind.Class
        };
    }
}