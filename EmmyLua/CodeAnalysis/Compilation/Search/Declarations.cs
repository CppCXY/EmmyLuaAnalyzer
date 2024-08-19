using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Type;

namespace EmmyLua.CodeAnalysis.Compilation.Search;

public class Declarations(SearchContext context)
{
    private Dictionary<SyntaxElementId, LuaSymbol?> DeclarationCaches { get; } = new();

    public LuaSymbol? FindDeclaration(LuaSyntaxElement? element)
    {
        if (element is null)
        {
            return null;
        }

        if (context.Features.Cache && DeclarationCaches.TryGetValue(element.UniqueId, out var declaration))
        {
            return declaration;
        }

        declaration = InnerDeclaration(element);

        if (context.Features.Cache && declaration is not null)
        {
            DeclarationCaches[element.UniqueId] = declaration;
        }

        return declaration;
    }

    private LuaSymbol? InnerDeclaration(LuaSyntaxElement? element)
    {
        switch (element)
        {
            case LuaNameExprSyntax nameExpr:
            {
                return FindNameDeclaration(nameExpr);
            }
            case LuaIndexExprSyntax indexExpr:
            {
                return FindIndexDeclaration(indexExpr);
            }
            case LuaCallExprSyntax callExpr:
            {
                return FindCallDeclaration(callExpr);
            }
            case LuaTableFieldSyntax tableField:
            {
                return FindTableFieldDeclaration(tableField);
            }
            case LuaDocNameTypeSyntax docNameType:
            {
                return FindTypeDeclaration(docNameType.Name?.RepresentText, element.DocumentId);
            }
            case LuaDocGenericTypeSyntax docGenericType:
            {
                return FindTypeDeclaration(docGenericType.Name?.RepresentText, element.DocumentId);
            }
            case LuaDocTagClassSyntax docTagClass:
            {
                return FindTypeDeclaration(docTagClass.Name?.RepresentText, element.DocumentId);
            }
            case LuaDocTagInterfaceSyntax docTagInterface:
            {
                return FindTypeDeclaration(docTagInterface.Name?.RepresentText, element.DocumentId);
            }
            case LuaDocTagAliasSyntax docTagAlias:
            {
                return FindTypeDeclaration(docTagAlias.Name?.RepresentText, element.DocumentId);
            }
            case LuaDocTagEnumSyntax docTagEnum:
            {
                return FindTypeDeclaration(docTagEnum.Name?.RepresentText, element.DocumentId);
            }
            case LuaDocFieldSyntax docField:
            {
                return FindDocFieldDeclaration(docField);
            }
            case LuaDocTagNamedTypeSyntax docTagNamedType:
            {
                return FindTypeDeclaration(docTagNamedType.Name?.RepresentText, element.DocumentId);
            }
            case LuaParamDefSyntax or LuaLocalNameSyntax:
            {
                return context.Compilation.Db.QueryLocalDeclaration(element);
            }
            case LuaDocTagParamSyntax docTagParam:
            {
                return FindDocParamDeclaration(docTagParam);
            }
        }

        return null;
    }

    private LuaSymbol? FindNameDeclaration(LuaNameExprSyntax nameExpr)
    {
        if (nameExpr.Name is { Text: "self" })
        {
            var closures = nameExpr.Ancestors.OfType<LuaClosureExprSyntax>();
            foreach (var closure in closures)
            {
                var stat = closure.Parent;
                if (stat is LuaFuncStatSyntax { IndexExpr.PrefixExpr: { } expr })
                {
                    return FindDeclaration(expr);
                }
            }
        }

        var luaDeclaration = context.Compilation.Db.QueryLocalDeclaration(nameExpr);
        if (luaDeclaration is not null)
        {
            return luaDeclaration;
        }

        if (nameExpr.Name is { } name)
        {
            return context.Compilation.TypeManager.GetGlobalSymbol(name.RepresentText);
        }

        return null;
    }

    private LuaSymbol? FindTableFieldDeclaration(LuaTableFieldSyntax tableField)
    {
        if (tableField is { ParentTable: { } parentTable, Name: { } name })
        {
            var parentType = context.Infer(parentTable);
            return context.FindMember(parentType, name);
        }

        return null;
    }

    private LuaSymbol? FindIndexDeclaration(LuaIndexExprSyntax indexExpr)
    {
        if (indexExpr.PrefixExpr is { } prefixExpr)
        {
            var prefixType = context.Infer(prefixExpr);
            return context.FindMember(prefixType, indexExpr);
        }

        return null;
    }

    private LuaSymbol? FindTypeDeclaration(string? name, LuaDocumentId documentId)
    {
        if (name is not null)
        {
            var namedType = new LuaNamedType(documentId, name);
            return context.Compilation.TypeManager.GetTypeDefinedSymbol(namedType);
        }

        return null;
    }

    private LuaSymbol? FindDocFieldDeclaration(LuaDocFieldSyntax docField)
    {
        if (docField.Name is null)
        {
            return null;
        }

        if (docField.Parent is LuaDocTagFieldSyntax tagFieldSyntax)
        {
            var firstNamedTag = tagFieldSyntax.PrevOfType<LuaDocTagNamedTypeSyntax>()
                .FirstOrDefault();
            if (firstNamedTag is { Name: { } name })
            {
                var namedType = new LuaNamedType(name.DocumentId, name.RepresentText);
                return context.FindMember(namedType, docField.Name);
            }
        }
        else if (docField.Parent is LuaDocTableTypeSyntax tableTypeSyntax)
        {
            if (tableTypeSyntax is { Parent: LuaDocTagNamedTypeSyntax tagNamedType })
            {
                if (tagNamedType is LuaDocTagAliasSyntax)
                {
                    return context.FindMember(tableTypeSyntax.UniqueId, docField.Name);
                }

                if (tagNamedType.Name is not null)
                {
                    var namedType = new LuaNamedType(tagNamedType.DocumentId, tagNamedType.Name.RepresentText);
                    return context.FindMember(namedType, docField.Name);
                }
            }
            else
            {
                return context.FindMember(tableTypeSyntax.UniqueId, docField.Name);
            }
        }

        return null;
    }

    public bool IsUpValue(LuaNameExprSyntax nameExpr, LuaSymbol symbol)
    {
        if (nameExpr.Name is { Text: "self" })
        {
            return false;
        }

        if (symbol.DocumentId != nameExpr.DocumentId)
        {
            return false;
        }

        var closure = nameExpr.Ancestors.OfType<LuaClosureExprSyntax>().FirstOrDefault();
        var element = symbol.Info.Ptr.ToNode(context);
        if (closure is not null && element is not null)
        {
            return closure.Position > element.Position;
        }

        return false;
    }

    private LuaSymbol? FindDocParamDeclaration(LuaDocTagParamSyntax docTagParam)
    {
        if (docTagParam is
            {
                Name.RepresentText: { } docName, Parent: LuaCommentSyntax
                {
                    Owner: LuaFuncStatSyntax
                    {
                        ClosureExpr.ParamList.Params: { } paramList
                    }
                }
            })
        {
            foreach (var paramElement in paramList)
            {
                if (paramElement.Name?.RepresentText == docName)
                {
                    return FindDeclaration(paramElement);
                }
            }
        }

        return null;
    }

    private LuaSymbol? FindCallDeclaration(LuaCallExprSyntax callExpr)
    {
        if (callExpr.PrefixExpr is { } prefixExpr)
        {
            var prefixType = context.Infer(prefixExpr);
            return VirtualInfo.CreateTypeSymbol(prefixType);
        }

        return null;
    }
}
