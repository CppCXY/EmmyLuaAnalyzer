using EmmyLua.CodeAnalysis.Common;
using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Reference;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Search;

public record ReferenceResult(ILocation Location, LuaSyntaxElement Element, ReferenceKind Kind = ReferenceKind.Unknown);

public class References(SearchContext context)
{
    public IEnumerable<ReferenceResult> FindReferences(IDeclaration declaration)
    {
        var referencesSet = new ReferencesSet();
        if (declaration is LuaDeclaration luaDeclaration)
        {
            var results = luaDeclaration.Info switch
            {
                LocalInfo localInfo => LocalReferences(luaDeclaration, localInfo),
                GlobalInfo => GlobalReferences(luaDeclaration),
                MethodInfo methodInfo => MethodReferences(luaDeclaration, methodInfo),
                DocFieldInfo docFieldInfo => DocFieldReferences(luaDeclaration, docFieldInfo),
                EnumFieldInfo enumFieldInfo => EnumFieldReferences(luaDeclaration, enumFieldInfo),
                TableFieldInfo tableFieldInfo => TableFieldReferences(luaDeclaration, tableFieldInfo),
                TupleMemberInfo tupleMemberInfo => TupleMemberReferences(luaDeclaration, tupleMemberInfo),
                NamedTypeInfo namedTypeInfo => NamedTypeReferences(luaDeclaration, namedTypeInfo),
                ParamInfo paramInfo => ParameterReferences(luaDeclaration, paramInfo),
                IndexInfo indexInfo => IndexExprReferences(luaDeclaration, indexInfo),
                _ => []
            };

            foreach (var result in results)
            {
                referencesSet.AddReference(result);
            }
        }

        return referencesSet.GetReferences();
    }

    private IEnumerable<ReferenceResult> LocalReferences(LuaDeclaration declaration, DeclarationInfo info)
    {
        var references = new List<ReferenceResult>();
        var luaReferences = context.Compilation.Db.QueryLocalReferences(declaration);
        foreach (var luaReference in luaReferences)
        {
            if (luaReference.Ptr.ToNode(context) is {} element)
            {
                references.Add(new ReferenceResult(element.Location, element, luaReference.Kind));
            }
        }

        return references;
    }

    private IEnumerable<ReferenceResult> GlobalReferences(LuaDeclaration declaration)
    {
        var references = new List<ReferenceResult>();
        var globalName = declaration.Name;
        var nameExprs = context.Compilation.Db.QueryNameExprReferences(globalName, context);

        foreach (var nameExpr in nameExprs)
        {
            if (context.FindDeclaration(nameExpr) == declaration)
            {
                references.Add(new ReferenceResult(nameExpr.Location, nameExpr));
            }
        }

        return references;
    }

    private IEnumerable<ReferenceResult> FieldReferences(LuaDeclaration declaration, string fieldName)
    {
        var references = new List<ReferenceResult>();
        var indexExprs = context.Compilation.Db.QueryIndexExprReferences(fieldName, context);
        foreach (var indexExpr in indexExprs)
        {
            if (context.FindDeclaration(indexExpr) == declaration && indexExpr.KeyElement is {} keyElement)
            {
                references.Add(new ReferenceResult(keyElement.Location, keyElement));
            }
        }

        return references;
    }

    private IEnumerable<ReferenceResult> MethodReferences(LuaDeclaration declaration, MethodInfo methodInfo)
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

        return [];
    }

    private IEnumerable<ReferenceResult> DocFieldReferences(LuaDeclaration fieldDeclaration, DocFieldInfo info)
    {
        var name = fieldDeclaration.Name;
        var references = new List<ReferenceResult>();
        if (info.FieldDefPtr.ToNode(context) is { } fieldDef)
        {
            if (fieldDef.FieldElement is { } fieldElement)
            {
                references.Add(new ReferenceResult(fieldElement.Location, fieldElement));
            }

            var parentType = context.Compilation.Db.QueryParentType(fieldDef);
            if (parentType is not null)
            {
                var members = context.FindMember(parentType, name);
                foreach (var member in members.OfType<LuaDeclaration>())
                {
                    if (member is
                        {
                            Name: { } name2, Info: TableFieldInfo { TableFieldPtr: { } tableFieldPtr }
                        }
                        && tableFieldPtr.ToNode(context) is { KeyElement: { } keyElement }
                       )
                    {
                        references.Add(new ReferenceResult(keyElement.Location, keyElement));
                        break;
                    }
                }
            }

            references.AddRange(FieldReferences(fieldDeclaration, name));
        }

        return references;
    }

    private IEnumerable<ReferenceResult> EnumFieldReferences(LuaDeclaration declaration, EnumFieldInfo info)
    {
        var name = declaration.Name;
        var references = new List<ReferenceResult>();
        if (info.EnumFieldDefPtr.ToNode(context) is { } enumFieldDef)
        {
            if (enumFieldDef.Name is { } enumFieldName)
            {
                references.Add(new ReferenceResult(enumFieldName.Location, enumFieldName));
            }

            var parentType = context.Compilation.Db.QueryParentType(enumFieldDef);
            if (parentType is not null)
            {
                var members = context.FindMember(parentType, name);
                foreach (var member in members.OfType<LuaDeclaration>())
                {
                    if (member is
                        {
                            Name: { } name2, Info: TableFieldInfo { TableFieldPtr: { } tableFieldPtr }
                        } && tableFieldPtr.ToNode(context) is { KeyElement: { } keyElement }
                       )
                    {
                        references.Add(new ReferenceResult(keyElement.Location, keyElement));
                    }
                }
            }

            references.AddRange(FieldReferences(declaration, name));
        }

        return references;
    }

    private IEnumerable<ReferenceResult> TableFieldReferences(LuaDeclaration declaration, TableFieldInfo info)
    {
        var name = declaration.Name;
        var references = new List<ReferenceResult>();
        if (info.TableFieldPtr.ToNode(context) is { } fieldDef)
        {
            if (fieldDef.KeyElement is { } keyElement)
            {
                references.Add(new ReferenceResult(keyElement.Location, keyElement));
            }

            var parentType = context.Compilation.Db.QueryParentType(fieldDef);
            if (parentType is not null)
            {
                var members = context.FindMember(parentType, name);
                foreach (var member in members.OfType<LuaDeclaration>())
                {
                    if (member is
                        {
                            Name: { } name2, Info: DocFieldInfo { FieldDefPtr: { } fieldDefPtr }
                        } && fieldDefPtr.ToNode(context) is { FieldElement: { } fieldElement }
                       )
                    {
                        references.Add(new ReferenceResult(fieldElement.Location, fieldElement));
                    }
                }
            }

            references.AddRange(FieldReferences(declaration, name));
        }

        return references;
    }

    private IEnumerable<ReferenceResult> TupleMemberReferences(LuaDeclaration declaration, TupleMemberInfo info)
    {
        var name = declaration.Name;
        var references = new List<ReferenceResult>();
        if (info.TypePtr.ToNode(context) is { } tupleMember)
        {
            references.Add(new ReferenceResult(tupleMember.Location, tupleMember));

            var parentType = context.Compilation.Db.QueryParentType(tupleMember);
            if (parentType is not null)
            {
                var members = context.FindMember(parentType, name);
                foreach (var member in members.OfType<LuaDeclaration>())
                {
                    if (member is
                        {
                            Name: { } name2, Info: TableFieldInfo { TableFieldPtr: { } tableFieldPtr }
                        } && tableFieldPtr.ToNode(context) is { KeyElement: { } keyElement }
                       )
                    {
                        references.Add(new ReferenceResult(keyElement.Location, keyElement));
                    }
                }
            }

            references.AddRange(FieldReferences(declaration, name));
        }

        return references;
    }

    private IEnumerable<ReferenceResult> NamedTypeReferences(LuaDeclaration declaration, NamedTypeInfo info)
    {
        var name = declaration.Name;
        var references = new List<ReferenceResult>();
        if (info.TypeDefinePtr.ToNode(context) is { Name: { } typeName })
        {
            references.Add(new ReferenceResult(typeName.Location, typeName));
            var nameTypes = context.Compilation.Db.QueryNamedTypeReferences(name, context);
            foreach (var nameType in nameTypes)
            {
                if (context.FindDeclaration(nameType) == declaration && nameType.Name is { } name2)
                {
                    references.Add(new ReferenceResult(name2.Location, name2));
                }
            }
        }

        return references;
    }

    private IEnumerable<ReferenceResult> ParameterReferences(LuaDeclaration declaration, ParamInfo info)
    {
        var references = new List<ReferenceResult>();

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
            references.Add(new ReferenceResult(typedParamName.Location, typedParamName));
        }

        return references;
    }

    private ReferenceResult? DocParameterReferences(LuaParamDefSyntax paramDefSyntax)
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
                    return new ReferenceResult(tagParamSyntax.Name.Location, tagParamSyntax.Name);
                }
            }
        }

        return null;
    }

    private IEnumerable<ReferenceResult> IndexExprReferences(LuaDeclaration declaration, IndexInfo info)
    {
        var references = new List<ReferenceResult>();
        if (info.IndexExprPtr.ToNode(context) is { Name: { } name })
        {
            references.AddRange(FieldReferences(declaration, name));
        }

        return references;
    }
}

class ReferencesSet
{
    readonly record struct Position(int Line, int Character) : IComparable<Position>
    {
        public int CompareTo(Position other)
        {
            var lineComparison = Line.CompareTo(other.Line);
            return lineComparison != 0 ? lineComparison : Character.CompareTo(other.Character);
        }
    }

    private SortedDictionary<Position, ReferenceResult> References { get; } = new();

    public void AddReference(ReferenceResult reference)
    {
        var position = new Position(reference.Location.StartLine, reference.Location.StartCol);
        References.TryAdd(position, reference);
    }

    public IEnumerable<ReferenceResult> GetReferences()
    {
        return References.Values;
    }
}
