using EmmyLua.CodeAnalysis.Compilation.Reference;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Type;
using EmmyLua.CodeAnalysis.Type.Manager.TypeInfo;
using EmmyLua.CodeAnalysis.Type.Types;

namespace EmmyLua.CodeAnalysis.Compilation.Search;

public record ReferenceResult(
    LuaLocation Location,
    LuaSyntaxElement Element,
    ReferenceKind Kind = ReferenceKind.Unknown);

public class References(SearchContext context)
{
    private HashSet<TypeInfo> TypeInfoGuard { get; } = new();

    public IEnumerable<ReferenceResult> FindReferences(LuaSymbol luaSymbol)
    {
        var referencesSet = new ReferencesSet();

        var results = luaSymbol.Info switch
        {
            LocalInfo => LocalReferences(luaSymbol),
            GlobalInfo => GlobalReferences(luaSymbol),
            ParamInfo paramInfo => ParameterReferences(luaSymbol, paramInfo),
            MethodInfo methodInfo => MethodReferences(luaSymbol, methodInfo),
            DocFieldInfo docFieldInfo => DocFieldReferences(luaSymbol, docFieldInfo),
            EnumFieldInfo enumFieldInfo => EnumFieldReferences(luaSymbol, enumFieldInfo),
            TableFieldInfo tableFieldInfo => TableFieldReferences(luaSymbol, tableFieldInfo),

            // TupleMemberInfo tupleMemberInfo => TupleMemberReferences(luaSymbol, tupleMemberInfo),
            NamedTypeInfo namedTypeInfo => NamedTypeReferences(luaSymbol, namedTypeInfo),
            IndexInfo indexInfo => IndexExprReferences(luaSymbol, indexInfo),
            _ => []
        };

        foreach (var result in results)
        {
            referencesSet.AddReference(result);
        }

        return referencesSet.GetReferences();
    }

    private IEnumerable<ReferenceResult> LocalReferences(LuaSymbol symbol)
    {
        var references = new List<ReferenceResult>();
        var luaReferences = context.Compilation.Db.QueryLocalReferences(symbol);
        foreach (var luaReference in luaReferences)
        {
            if (luaReference.Ptr.ToNode(context) is { } element)
            {
                references.Add(new ReferenceResult(element.Location, element, luaReference.Kind));
            }
        }

        return references;
    }

    private IEnumerable<ReferenceResult> GlobalReferences(LuaSymbol symbol)
    {
        var references = new List<ReferenceResult>();
        var globalName = symbol.Name;
        var nameExprs = context.Compilation.Db.QueryNameExprReferences(globalName, context);

        foreach (var nameExpr in nameExprs)
        {
            if (IsReferenceTo(nameExpr, symbol))
            {
                references.Add(new ReferenceResult(nameExpr.Location, nameExpr));
            }
        }

        return references;
    }

    private IEnumerable<ReferenceResult> FieldReferences(LuaSymbol symbol, string fieldName)
    {
        var references = new List<ReferenceResult>();
        var indexExprs = context.Compilation.Db.QueryIndexExprReferences(fieldName, context);
        foreach (var indexExpr in indexExprs)
        {
            if (IsReferenceTo(indexExpr, symbol) && indexExpr.KeyElement is { } keyElement)
            {
                references.Add(new ReferenceResult(keyElement.Location, keyElement));
            }
        }

        var tableFields = context.Compilation.Db.QueryTableFieldReferences(fieldName, context);
        foreach (var tableField in tableFields)
        {
            if (IsReferenceTo(tableField, symbol) && tableField.KeyElement is { } keyElement)
            {
                references.Add(new ReferenceResult(keyElement.Location, keyElement));
            }
        }

        return references;
    }

    private IEnumerable<ReferenceResult> MethodReferences(LuaSymbol symbol, MethodInfo methodInfo)
    {
        switch (symbol)
        {
            case { IsLocal: true }:
            {
                return LocalReferences(symbol);
            }
            case { IsGlobal: true }:
            {
                return GlobalReferences(symbol);
            }
            default:
            {
                var results = new List<ReferenceResult>();
                var mappingName = context.Compilation.Db.QueryMapping(methodInfo.IndexPtr.UniqueId);
                if (mappingName is not null)
                {
                    results.AddRange(FieldReferences(symbol, mappingName));
                }

                if (methodInfo.IndexPtr.ToNode(context) is { Name: { } name })
                {
                    results.AddRange(FieldReferences(symbol, name));
                }

                return results;
            }
        }
    }

    private IEnumerable<ReferenceResult> DocFieldReferences(LuaSymbol fieldSymbol, DocFieldInfo info)
    {
        var name = fieldSymbol.Name;
        var references = new List<ReferenceResult>();
        if (info.FieldDefPtr.ToNode(context) is { } fieldDef)
        {
            if (fieldDef.FieldElement is { } fieldElement)
            {
                references.Add(new ReferenceResult(fieldElement.Location, fieldElement));
            }

            references.AddRange(FieldReferences(fieldSymbol, name));
        }

        return references;
    }

    private IEnumerable<ReferenceResult> EnumFieldReferences(LuaSymbol symbol, EnumFieldInfo info)
    {
        var name = symbol.Name;
        var references = new List<ReferenceResult>();
        if (info.EnumFieldDefPtr.ToNode(context) is { } enumFieldDef)
        {
            if (enumFieldDef.Name is { } enumFieldName)
            {
                references.Add(new ReferenceResult(enumFieldName.Location, enumFieldName));
            }

            references.AddRange(FieldReferences(symbol, name));
        }

        return references;
    }

    private IEnumerable<ReferenceResult> TableFieldReferences(LuaSymbol symbol, TableFieldInfo info)
    {
        var name = symbol.Name;
        var references = new List<ReferenceResult>();
        if (info.TableFieldPtr.ToNode(context) is { } fieldDef)
        {
            if (fieldDef.KeyElement is { } keyElement)
            {
                references.Add(new ReferenceResult(keyElement.Location, keyElement));
            }

            references.AddRange(FieldReferences(symbol, name));
        }

        return references;
    }

    // private IEnumerable<ReferenceResult> TupleMemberReferences(LuaSymbol symbol, TupleMemberInfo info)
    // {
    //     var name = symbol.Name;
    //     var references = new List<ReferenceResult>();
    //     if (info.TypePtr.ToNode(context) is { } tupleMember)
    //     {
    //         references.Add(new ReferenceResult(tupleMember.Location, tupleMember));
    //         references.AddRange(FieldReferences(symbol, name));
    //     }
    //
    //     return references;
    // }

    private IEnumerable<ReferenceResult> NamedTypeReferences(LuaSymbol symbol, NamedTypeInfo info)
    {
        if (symbol.Type is not LuaNamedType namedType)
        {
            return [];
        }

        var references = new List<ReferenceResult>();
        if (info.TypeDefinePtr.ToNode(context) is { Name: { } typeName })
        {
            references.Add(new ReferenceResult(typeName.Location, typeName));
            var nameTypePtrList = context.Compilation.Db.QueryAllNamedType();
            foreach (var nameTypePtr in nameTypePtrList)
            {
                if (nameTypePtr.ToNode(context) is { Name.RepresentText: { } name, DocumentId: { } documentId } element)
                {
                    var namedType2 = new LuaNamedType(documentId, name);
                    if (namedType.IsSameType(namedType2, context))
                    {
                        references.Add(new ReferenceResult(element.Location, element));
                    }
                }
            }
        }

        return references;
    }

    private IEnumerable<ReferenceResult> ParameterReferences(LuaSymbol symbol, ParamInfo info)
    {
        var references = new List<ReferenceResult>();

        if (info.ParamDefPtr.ToNode(context) is { } paramDef)
        {
            if (DocParameterReferences(paramDef) is { } luaReference)
            {
                references.Add(luaReference);
            }

            references.AddRange(LocalReferences(symbol));
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

    private IEnumerable<ReferenceResult> IndexExprReferences(LuaSymbol symbol, IndexInfo info)
    {
        var references = new List<ReferenceResult>();
        if (info.IndexExprPtr.ToNode(context) is { Name: { } name })
        {
            references.AddRange(FieldReferences(symbol, name));
        }

        return references;
    }

    public bool IsReferenceTo(LuaSyntaxElement element, LuaSymbol symbol)
    {
        var declarationSymbol = context.FindDeclaration(element);
        if (declarationSymbol is null)
        {
            return false;
        }

        if (declarationSymbol.IsReferenceTo(symbol))
        {
            return true;
        }

        if (element is LuaIndexExprSyntax { PrefixExpr: { } prefixExpr })
        {
            var parentSymbol = context.FindDeclaration(prefixExpr);
            if (parentSymbol is null)
            {
                return false;
            }

            if (parentSymbol.Type is LuaNamedType namedParentType)
            {
                var parentTypeInfo = context.Compilation.TypeManager.FindTypeInfo(namedParentType);
                if (parentTypeInfo is not null)
                {
                    return IsTypeInfoContainSymbol(parentTypeInfo, symbol);
                }
            }
        }

        return false;
    }

    private bool IsTypeInfoContainSymbol(TypeInfo typeInfo, LuaSymbol symbol)
    {
        if (!TypeInfoGuard.Add(typeInfo))
        {
            return false;
        }

        try
        {
            if (typeInfo.Declarations is not null &&
                typeInfo.Declarations.TryGetValue(symbol.Name, out var childSymbol))
            {
                if (childSymbol.IsReferenceTo(symbol))
                {
                    return true;
                }
            }

            if (typeInfo is { Supers: { } supers })
            {
                foreach (var super in supers)
                {
                    var superTypeInfo = context.Compilation.TypeManager.FindTypeInfo(super);
                    if (superTypeInfo is not null && IsTypeInfoContainSymbol(superTypeInfo, symbol))
                    {
                        return true;
                    }
                }
            }

            return false;
        }
        finally
        {
            TypeInfoGuard.Remove(typeInfo);
        }
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
