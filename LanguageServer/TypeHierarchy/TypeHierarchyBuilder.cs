using EmmyLua.CodeAnalysis.Compilation;
using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Semantic;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using LanguageServer.Util;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace LanguageServer.TypeHierarchy;

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
        var supers = compilation.DbManager.GetSupers(name);
        var items = new List<TypeHierarchyItem>();
        foreach (var super in supers)
        {
            if (super is LuaNamedType superNamedType)
            {
                var typeDefine = compilation.DbManager.GetTypeLuaDeclaration(superNamedType.Name);
                if (typeDefine is not null)
                {
                    var typeDocument = compilation.Workspace.GetDocument(typeDefine.TypeDefinePtr.DocumentId);
                    if (typeDocument is not null
                        && typeDefine.TypeDefinePtr.ToNode(typeDocument) is { Range: { } sourceRange })
                    {
                        var range = sourceRange.ToLspRange(typeDocument);
                        items.Add(new TypeHierarchyItem
                        {
                            Name = $"super {superNamedType.Name}",
                            Kind = ToSymbolKind(typeDefine),
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
        var subTypes = compilation.DbManager.GetSubTypes(name);
        var items = new List<TypeHierarchyItem>();
        foreach (var subTypeName in subTypes)
        {
            var typeDefine = compilation.DbManager.GetTypeLuaDeclaration(subTypeName);
            if (typeDefine is not null)
            {
                var typeDocument = compilation.Workspace.GetDocument(typeDefine.TypeDefinePtr.DocumentId);
                if (typeDocument is not null
                    && typeDefine.TypeDefinePtr.ToNode(typeDocument) is { Range: { } sourceRange })
                {
                    var range = sourceRange.ToLspRange(typeDocument);
                    items.Add(new TypeHierarchyItem
                    {
                        Name = $"subtype {subTypeName}",
                        Kind = ToSymbolKind(typeDefine),
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

    private static SymbolKind ToSymbolKind(NamedTypeLuaDeclaration namedTypeLuaDeclaration)
    {
        return namedTypeLuaDeclaration.Kind switch
        {
            NamedTypeKind.Class => SymbolKind.Class,
            NamedTypeKind.Interface => SymbolKind.Interface,
            NamedTypeKind.Enum => SymbolKind.Enum,
            _ => SymbolKind.Class
        };
    }
}