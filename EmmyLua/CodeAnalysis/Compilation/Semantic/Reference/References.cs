using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Semantic.Reference;

public class References(SearchContext context)
{
    public IEnumerable<LuaReference> FindReferences(LuaSyntaxElement element)
    {
        var declaration = context.FindDeclaration(element);
        return declaration?.Info switch
        {
            LocalInfo localInfo => LocalReferences(declaration, localInfo),
            GlobalInfo => GlobalReferences(declaration),
            MethodInfo methodInfo => MethodReferences(declaration, methodInfo),
            DocFieldInfo docFieldInfo => DocFieldReferences(declaration, docFieldInfo),
            EnumFieldInfo enumFieldInfo => EnumFieldReferences(declaration, enumFieldInfo),
            TableFieldInfo tableFieldInfo => TableFieldReferences(declaration, tableFieldInfo),
            TupleMemberInfo tupleMemberInfo => TupleMemberReferences(declaration, tupleMemberInfo),
            NamedTypeInfo namedTypeInfo => NamedTypeReferences(declaration, namedTypeInfo),
            ParamInfo paramInfo => ParameterReferences(declaration, paramInfo),
            IndexInfo indexInfo => IndexExprReferences(declaration, indexInfo),
            _ => Enumerable.Empty<LuaReference>()
        };
    }

    private IEnumerable<LuaReference> LocalReferences(LuaDeclaration declaration, DeclarationInfo info)
    {
        var references = new List<LuaReference>();
        var localNameSyntax = info.Ptr.ToNode(context);
        var parentBlock = localNameSyntax?.Ancestors.OfType<LuaBlockSyntax>().FirstOrDefault();
        if (parentBlock is not null && localNameSyntax is not null)
        {
            references.Add(new LuaReference(localNameSyntax.Location, localNameSyntax));
            foreach (var node in parentBlock.Descendants
                         .Where(it => it.Position > declaration.Position))
            {
                if (node is LuaNameExprSyntax nameExpr && nameExpr.Name?.RepresentText == declaration.Name)
                {
                    if (context.FindDeclaration(nameExpr) == declaration)
                    {
                        references.Add(new LuaReference(nameExpr.Location, nameExpr));
                    }
                }
            }
        }

        return references;
    }

    private IEnumerable<LuaReference> GlobalReferences(LuaDeclaration declaration)
    {
        var references = new List<LuaReference>();
        var globalName = declaration.Name;
        var nameExprs = context.Compilation.Db.GetNameExprs(globalName);

        foreach (var nameExpr in nameExprs)
        {
            if (context.FindDeclaration(nameExpr) == declaration)
            {
                references.Add(new LuaReference(nameExpr.Location, nameExpr));
            }
        }

        return references;
    }

    private IEnumerable<LuaReference> FieldReferences(LuaDeclaration declaration, string fieldName)
    {
        var references = new List<LuaReference>();
        var indexExprs = context.Compilation.Db.GetIndexExprs(fieldName);
        foreach (var indexExpr in indexExprs)
        {
            if (context.FindDeclaration(indexExpr) == declaration)
            {
                references.Add(new LuaReference(indexExpr.KeyElement.Location, indexExpr.KeyElement));
            }
        }

        return references;
    }

    private IEnumerable<LuaReference> MethodReferences(LuaDeclaration declaration, MethodInfo methodInfo)
    {
        switch (declaration)
        {
            case { IsLocal: true }:
            {
                return LocalReferences(declaration, methodInfo);
            }
            case { IsGlobal: true }:
            {
                return GlobalReferences(declaration);
            }
            default:
            {
                if (methodInfo.IndexPtr.ToNode(context) is { Name: { } name })
                {
                    return FieldReferences(declaration, name);
                }

                break;
            }
        }

        return Enumerable.Empty<LuaReference>();
    }

    private IEnumerable<LuaReference> DocFieldReferences(LuaDeclaration fieldDeclaration, DocFieldInfo info)
    {
        var name = fieldDeclaration.Name;
        var references = new List<LuaReference>();
        if (info.FieldDefPtr.ToNode(context) is { } fieldDef)
        {
            if (fieldDef.FieldElement is { } fieldElement)
            {
                references.Add(new LuaReference(fieldElement.Location, fieldElement));
            }

            var parentType = context.Compilation.Db.GetParentType(fieldDef);
            if (parentType is not null)
            {
                var members = context.FindMember(parentType, name);
                foreach (var member in members)
                {
                    if (member is
                        {
                            Name: { } name2, Info: TableFieldInfo { TableFieldPtr: { } tableFieldPtr }
                        }
                        && tableFieldPtr.ToNode(context) is { KeyElement: { } keyElement }
                       )
                    {
                        references.Add(new LuaReference(keyElement.Location, keyElement));
                        break;
                    }
                }
            }

            references.AddRange(FieldReferences(fieldDeclaration, name));
        }

        return references;
    }

    private IEnumerable<LuaReference> EnumFieldReferences(LuaDeclaration declaration, EnumFieldInfo info)
    {
        var name = declaration.Name;
        var references = new List<LuaReference>();
        if (info.EnumFieldDefPtr.ToNode(context) is { } enumFieldDef)
        {
            if (enumFieldDef.Name is { } enumFieldName)
            {
                references.Add(new LuaReference(enumFieldName.Location, enumFieldName));
            }

            var parentType = context.Compilation.Db.GetParentType(enumFieldDef);
            if (parentType is not null)
            {
                var members = context.FindMember(parentType, name);
                foreach (var member in members)
                {
                    if (member is
                        {
                            Name: { } name2, Info: TableFieldInfo { TableFieldPtr: { } tableFieldPtr }
                        } && tableFieldPtr.ToNode(context) is { KeyElement: { } keyElement }
                       )
                    {
                        references.Add(new LuaReference(keyElement.Location, keyElement));
                    }
                }
            }

            references.AddRange(FieldReferences(declaration, name));
        }

        return references;
    }

    private IEnumerable<LuaReference> TableFieldReferences(LuaDeclaration declaration, TableFieldInfo info)
    {
        var name = declaration.Name;
        var references = new List<LuaReference>();
        if (info.TableFieldPtr.ToNode(context) is { } fieldDef)
        {
            if (fieldDef.KeyElement is { } keyElement)
            {
                references.Add(new LuaReference(keyElement.Location, keyElement));
            }

            var parentType = context.Compilation.Db.GetParentType(fieldDef);
            if (parentType is not null)
            {
                var members = context.FindMember(parentType, name);
                foreach (var member in members)
                {
                    if (member is
                        {
                            Name: { } name2, Info: DocFieldInfo { FieldDefPtr: { } fieldDefPtr }
                        } && fieldDefPtr.ToNode(context) is { FieldElement: { } fieldElement }
                       )
                    {
                        references.Add(new LuaReference(fieldElement.Location, fieldElement));
                    }
                }
            }

            references.AddRange(FieldReferences(declaration, name));
        }

        return references;
    }

    private IEnumerable<LuaReference> TupleMemberReferences(LuaDeclaration declaration, TupleMemberInfo info)
    {
        var name = declaration.Name;
        var references = new List<LuaReference>();
        if (info.TypePtr.ToNode(context) is { } tupleMember)
        {
            references.Add(new LuaReference(tupleMember.Location, tupleMember));

            var parentType = context.Compilation.Db.GetParentType(tupleMember);
            if (parentType is not null)
            {
                var members = context.FindMember(parentType, name);
                foreach (var member in members)
                {
                    if (member is
                        {
                            Name: { } name2, Info: TableFieldInfo { TableFieldPtr: { } tableFieldPtr }
                        } && tableFieldPtr.ToNode(context) is { KeyElement: { } keyElement }
                       )
                    {
                        references.Add(new LuaReference(keyElement.Location, keyElement));
                    }
                }
            }

            references.AddRange(FieldReferences(declaration, name));
        }

        return references;
    }

    private IEnumerable<LuaReference> NamedTypeReferences(LuaDeclaration declaration, NamedTypeInfo info)
    {
        var name = declaration.Name;
        var references = new List<LuaReference>();
        if (info.TypeDefinePtr.ToNode(context) is { Name: { } typeName })
        {
            references.Add(new LuaReference(typeName.Location, typeName));
            var nameTypes = context.Compilation.Db.GetNameTypes(name);
            foreach (var nameType in nameTypes)
            {
                if (context.FindDeclaration(nameType) == declaration && nameType.Name is { } name2)
                {
                    references.Add(new LuaReference(name2.Location, name2));
                }
            }
        }

        return references;
    }

    private IEnumerable<LuaReference> ParameterReferences(LuaDeclaration declaration, ParamInfo info)
    {
        var references = new List<LuaReference>();

        if (info.ParamDefPtr.ToNode(context) is { } paramDef)
        {
            if (DocParameterReferences(paramDef) is { } luaReference)
            {
                references.Add(luaReference);
            }

            references.AddRange(LocalReferences(declaration, info));
        }
        else if (info.TypedParamPtr.ToNode(context) is { Name: { } typedParamName })
        {
            references.Add(new LuaReference(typedParamName.Location, typedParamName));
        }

        return references;
    }

    private LuaReference? DocParameterReferences(LuaParamDefSyntax paramDefSyntax)
    {
        var paramDefName = paramDefSyntax.Name?.RepresentText;
        var stat = paramDefSyntax.Ancestors.OfType<LuaStatSyntax>().FirstOrDefault();
        if (stat is null || paramDefName is null)
        {
            return null;
        }

        foreach (var comment in stat.Comments)
        {
            foreach (var tagParamSyntax in comment.DocList.OfType<LuaDocTagParamSyntax>())
            {
                if (tagParamSyntax.Name is { RepresentText: { } name } && name == paramDefName)
                {
                    return new LuaReference(tagParamSyntax.Name.Location, tagParamSyntax.Name);
                }
            }
        }

        return null;
    }

    private IEnumerable<LuaReference> IndexExprReferences(LuaDeclaration declaration, IndexInfo info)
    {
        var references = new List<LuaReference>();
        if (info.IndexExprPtr.ToNode(context) is { Name: { } name })
        {
            references.AddRange(FieldReferences(declaration, name));
        }

        return references;
    }
}
