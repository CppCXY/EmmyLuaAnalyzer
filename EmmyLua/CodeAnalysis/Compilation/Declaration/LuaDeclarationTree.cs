using EmmyLua.CodeAnalysis.Common;
using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Syntax.Tree;
using EmmyLua.CodeAnalysis.Type;

namespace EmmyLua.CodeAnalysis.Compilation.Declaration;

public class LuaDeclarationTree(LuaSyntaxTree tree, IReadOnlyDictionary<long, DeclarationScope> scopeOwners)
{
    public LuaSyntaxTree SyntaxTree { get; } = tree;

    public DeclarationScope? RootScope { get; internal set; }

    internal LuaDeclaration? FindDeclaration(LuaSyntaxElement? element, SearchContext context)
    {
        switch (element)
        {
            case LuaNameExprSyntax nameExpr:
            {
                return FindNameDeclaration(nameExpr, context);
            }
            case LuaIndexExprSyntax indexExpr:
            {
                return FindIndexDeclaration(indexExpr, context);
            }
            case LuaTableFieldSyntax tableField:
            {
                return FindTableFieldDeclaration(tableField, context);
            }
            case LuaDocNameTypeSyntax docNameType:
            {
                return FindTypeDeclaration(docNameType.Name?.RepresentText, context);
            }
            case LuaDocGenericTypeSyntax docGenericType:
            {
                return FindTypeDeclaration(docGenericType.Name?.RepresentText, context);
            }
            case LuaDocTagClassSyntax docTagClass:
            {
                return FindTypeDeclaration(docTagClass.Name?.RepresentText, context);
            }
            case LuaDocTagInterfaceSyntax docTagInterface:
            {
                return FindTypeDeclaration(docTagInterface.Name?.RepresentText, context);
            }
            case LuaDocTagAliasSyntax docTagAlias:
            {
                return FindTypeDeclaration(docTagAlias.Name?.RepresentText, context);
            }
            case LuaDocTagEnumSyntax docTagEnum:
            {
                return FindTypeDeclaration(docTagEnum.Name?.RepresentText, context);
            }
            case LuaDocFieldSyntax docField:
            {
                return FindDocFieldDeclaration(docField, context);
            }
            case LuaDocTagNamedTypeSyntax docTagNamedType:
            {
                return FindTypeDeclaration(docTagNamedType.Name?.RepresentText, context);
            }
            case LuaParamDefSyntax or LuaLocalNameSyntax:
            {
                var scope = FindScope(element);
                return scope?.FindDeclaration(element);
            }
        }

        return null;
    }

    private LuaDeclaration? FindNameDeclaration(LuaNameExprSyntax nameExpr, SearchContext context)
    {
        if (nameExpr.Name is { Text: "self" })
        {
            var closures = nameExpr.Ancestors.OfType<LuaClosureExprSyntax>();
            foreach (var closure in closures)
            {
                var stat = closure.Parent;
                if (stat is LuaFuncStatSyntax { IndexExpr.PrefixExpr: { } expr })
                {
                    return FindDeclaration(expr, context);
                }
            }
        }

        if (nameExpr.Name is { } name)
        {
            var scope = FindScope(nameExpr);
            var declaration = scope?.FindNameDeclaration(nameExpr);
            if (declaration is not null)
            {
                return declaration;
            }

            return context.Compilation.Db.QueryGlobals(name.RepresentText).FirstOrDefault();
        }

        return null;
    }

    private LuaDeclaration? FindTableFieldDeclaration(LuaTableFieldSyntax tableField, SearchContext context)
    {
        if (tableField is { ParentTable: { } parentTable, Name: { } name })
        {
            var relatedType = context.Compilation.Db.QueryTypeFromId(parentTable.UniqueId).FirstOrDefault() ??
                              new LuaTableLiteralType(parentTable);
            return context.FindMember(relatedType, name).FirstOrDefault();
        }

        return null;
    }

    private LuaDeclaration? FindIndexDeclaration(LuaIndexExprSyntax indexExpr, SearchContext context)
    {
        if (indexExpr.PrefixExpr is { } prefixExpr)
        {
            var prefixType = context.Infer(prefixExpr);
            return context.FindMember(prefixType, indexExpr).FirstOrDefault();
        }

        return null;
    }

    private IDeclaration? FindTypeDeclaration(string? name, SearchContext context)
    {
        if (name is not null)
        {
            if (context.Compilation.Db.IsDefinedType(name))
            {
                return context.Compilation.Db.QueryNamedTypeDefinitions(name).FirstOrDefault();
            }
        }

        return null;
    }

    private LuaDeclaration? FindDocFieldDeclaration(LuaDocFieldSyntax docField, SearchContext context)
    {
        var parentType = context.Compilation.Db.QueryParentType(docField);
        if (parentType is not null && docField.Name is { } name)
        {
            return context.FindMember(parentType, name).FirstOrDefault();
        }

        return null;
    }

    public DeclarationScope? FindScope(LuaSyntaxElement element)
    {
        var cur = element;
        while (cur != null)
        {
            if (scopeOwners.TryGetValue(cur.UniqueId, out var scope))
            {
                return scope;
            }

            cur = cur.Parent;
        }

        return null;
    }

    public IEnumerable<LuaDeclaration> GetDeclarationsBefore(LuaSyntaxElement beforeToken)
    {
        var token = SyntaxTree.SyntaxRoot.TokenAt(beforeToken.Position);
        if (token is not null)
        {
            var result = new List<LuaDeclaration>();
            var scope = FindScope(token);
            scope?.WalkUp(beforeToken.Position, 0, declaration =>
            {
                result.Add(declaration);
                return ScopeFoundState.NotFounded;
            });
            return result;
        }

        return [];
    }

    public bool IsUpValue(LuaNameExprSyntax nameExpr, LuaDeclaration declaration)
    {
        var scope = FindScope(nameExpr);
        if (scope is null)
        {
            return false;
        }

        var closure = nameExpr.Ancestors.OfType<LuaClosureExprSyntax>().FirstOrDefault();
        if (closure is not null)
        {
            return closure.Position > declaration.Position;
        }

        return false;
    }
}
