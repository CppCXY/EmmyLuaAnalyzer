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
            var supers = semanticModel.Compilation.DbManager.GetSupers(name);

            var items = new List<TypeHierarchyItem>();
            foreach (var super in supers)
            {
                if (super is LuaNamedType namedType)
                {
                    var typeDefine = semanticModel.Compilation.DbManager.GetTypeLuaDeclaration(namedType.Name);
                    if (typeDefine is not null)
                    {
                        var typeDocument =
                            semanticModel.Compilation.Workspace.GetDocument(typeDefine.TypeDefinePtr.DocumentId);
                        items.Add(new TypeHierarchyItem
                        {
                            Name = namedType.Name,
                            Kind = SymbolKind.Class,
                            Uri = typeDocument!.Uri,
                            Range = typeDefine.TypeDefinePtr.ToNode(typeDocument)!.Range.ToLspRange(typeDocument)
                        });
                    }
                }
            }

            return items;
        }

        return null;
    }
}