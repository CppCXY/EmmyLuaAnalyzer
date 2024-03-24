using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Semantic.Reference;

public class References(SearchContext context)
{
    public IEnumerable<LuaLocation> FindReferences(LuaSyntaxElement element)
    {
        var declarationTree = context.Compilation.GetDeclarationTree(element.Tree.Document.Id);
        if (declarationTree is null)
        {
            return Enumerable.Empty<LuaLocation>();
        }

        var declaration = declarationTree.FindDeclaration(element, context);
        return declaration switch
        {
            LocalLuaDeclaration localDeclaration => LocalReferences(localDeclaration, declarationTree),
            GlobalLuaDeclaration globalDeclaration => GlobalReferences(globalDeclaration),
            MethodLuaDeclaration methodDeclaration => MethodReferences(methodDeclaration),
            DocFieldLuaDeclaration fieldDeclaration => DocFieldReferences(fieldDeclaration),
            // EnumFieldLuaDeclaration
            TableFieldLuaDeclaration tableFieldDeclaration => TableFieldReferences(tableFieldDeclaration),
            NamedTypeLuaDeclaration namedTypeDeclaration => NamedTypeReferences(namedTypeDeclaration),
            _ => Enumerable.Empty<LuaLocation>()
        };
    }

    private IEnumerable<LuaLocation> LocalReferences(LuaDeclaration declaration, LuaDeclarationTree declarationTree)
    {
        var references = new List<LuaLocation>();
        var parentBlock = declaration.SyntaxElement?.Ancestors.OfType<LuaBlockSyntax>().FirstOrDefault();
        if (parentBlock is not null)
        {
            references.Add(declaration.SyntaxElement!.Location);
            foreach (var node in parentBlock.Descendants
                         .Where(it => it.Position > declaration.Position))
            {
                if (node is LuaNameExprSyntax nameExpr && nameExpr.Name?.RepresentText == declaration.Name)
                {
                    if (declarationTree.FindDeclaration(node, context) == declaration)
                    {
                        references.Add(node.Location);
                    }
                }
            }
        }

        return references;
    }

    private IEnumerable<LuaLocation> GlobalReferences(LuaDeclaration declaration)
    {
        var references = new List<LuaLocation>();
        var globalName = declaration.Name;
        var nameExprs = context.Compilation.ProjectIndex.GetNameExprs(globalName);

        foreach (var nameExpr in nameExprs)
        {
            var declarationTree = context.Compilation.GetDeclarationTree(nameExpr.Tree.Document.Id);
            if (declarationTree?.FindDeclaration(nameExpr, context) == declaration)
            {
                references.Add(nameExpr.Location);
            }
        }

        return references;
    }

    private IEnumerable<LuaLocation> FieldReferences(LuaDeclaration declaration, string fieldName)
    {
        var references = new List<LuaLocation>();
        var indexExprs = context.Compilation.ProjectIndex.GetIndexExprs(fieldName);
        foreach (var indexExpr in indexExprs)
        {
            var declarationTree = context.Compilation.GetDeclarationTree(indexExpr.Tree.Document.Id);
            if (declarationTree?.FindDeclaration(indexExpr, context) == declaration)
            {
                references.Add(indexExpr.KeyElement.Location);
            }
        }

        return references;
    }

    private IEnumerable<LuaLocation> MethodReferences(MethodLuaDeclaration declaration)
    {
        switch (declaration.Feature)
        {
            case DeclarationFeature.Local:
            {
                if (declaration.SyntaxElement is { Tree.Document.Id: { } id })
                {
                    var declarationTree = context.Compilation.GetDeclarationTree(id);
                    if (declarationTree is not null)
                    {
                        return LocalReferences(declaration, declarationTree);
                    }
                }

                break;
            }
            case DeclarationFeature.Global:
            {
                return GlobalReferences(declaration);
            }
            default:
            {
                if (declaration.IndexExpr is { Name: { } name })
                {
                    return FieldReferences(declaration, name);
                }

                break;
            }
        }

        return Enumerable.Empty<LuaLocation>();
    }

    private IEnumerable<LuaLocation> DocFieldReferences(DocFieldLuaDeclaration fieldDeclaration)
    {
        var references = new List<LuaLocation>();
        if (fieldDeclaration is { Name: { } name, FieldDef: { } fieldDef })
        {
            if (fieldDef.FieldElement is { } fieldElement)
            {
                references.Add(fieldElement.Location);
            }

            references.AddRange(FieldReferences(fieldDeclaration, name));
        }

        return references;
    }

    private IEnumerable<LuaLocation> TableFieldReferences(TableFieldLuaDeclaration declaration)
    {
        var references = new List<LuaLocation>();
        if (declaration is { Name: { } name, TableField: { } fieldDef })
        {
            references.Add(fieldDef.Location);
            references.AddRange(FieldReferences(declaration, name));
        }

        return references;
    }

    private IEnumerable<LuaLocation> NamedTypeReferences(NamedTypeLuaDeclaration declaration)
    {
        var references = new List<LuaLocation>();
        if (declaration is { Name: { } name, NameToken: { } nameToken })
        {
            references.Add(nameToken.Location);
            var nameTypes = context.Compilation.ProjectIndex.GetNameTypes(name);
            foreach (var nameType in nameTypes)
            {
                var declarationTree = context.Compilation.GetDeclarationTree(nameType.Tree.Document.Id);
                if (declarationTree?.FindDeclaration(nameType, context) == declaration)
                {
                    references.Add(nameType.Location);
                }
            }
        }

        return references;
    }
}
