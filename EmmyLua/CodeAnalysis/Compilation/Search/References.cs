using EmmyLua.CodeAnalysis.Compilation.Reference;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Search;

public record ReferenceResult(LuaLocation Location, LuaSyntaxElement Element, ReferenceKind Kind = ReferenceKind.Unknown);

public class References(SearchContext context)
{
    public IEnumerable<ReferenceResult> FindReferences(LuaSymbol luaSymbol)
    {
        var referencesSet = new ReferencesSet();

        var results = luaSymbol.Info switch
        {
            LocalInfo localInfo => LocalReferences(luaSymbol, localInfo),
            GlobalInfo => GlobalReferences(luaSymbol),
            MethodInfo methodInfo => MethodReferences(luaSymbol, methodInfo),
            DocFieldInfo docFieldInfo => DocFieldReferences(luaSymbol, docFieldInfo),
            EnumFieldInfo enumFieldInfo => EnumFieldReferences(luaSymbol, enumFieldInfo),
            TableFieldInfo tableFieldInfo => TableFieldReferences(luaSymbol, tableFieldInfo),
            TupleMemberInfo tupleMemberInfo => TupleMemberReferences(luaSymbol, tupleMemberInfo),
            NamedTypeInfo namedTypeInfo => NamedTypeReferences(luaSymbol, namedTypeInfo),
            ParamInfo paramInfo => ParameterReferences(luaSymbol, paramInfo),
            IndexInfo indexInfo => IndexExprReferences(luaSymbol, indexInfo),
            _ => []
        };

        foreach (var result in results)
        {
            referencesSet.AddReference(result);
        }


        return referencesSet.GetReferences();
    }

    private IEnumerable<ReferenceResult> LocalReferences(LuaSymbol symbol, ISymbolInfo info)
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
            if (context.FindDeclaration(nameExpr) == symbol)
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
            if (context.FindDeclaration(indexExpr)?.IsReferenceTo(symbol) == true && indexExpr.KeyElement is { } keyElement)
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
                return LocalReferences(symbol, methodInfo);
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

    private IEnumerable<ReferenceResult> TupleMemberReferences(LuaSymbol symbol, TupleMemberInfo info)
    {
        var name = symbol.Name;
        var references = new List<ReferenceResult>();
        if (info.TypePtr.ToNode(context) is { } tupleMember)
        {
            references.Add(new ReferenceResult(tupleMember.Location, tupleMember));
            references.AddRange(FieldReferences(symbol, name));
        }

        return references;
    }

    private IEnumerable<ReferenceResult> NamedTypeReferences(LuaSymbol symbol, NamedTypeInfo info)
    {
        var name = symbol.Name;
        var references = new List<ReferenceResult>();
        if (info.TypeDefinePtr.ToNode(context) is { Name: { } typeName })
        {
            references.Add(new ReferenceResult(typeName.Location, typeName));
            var nameTypes = context.Compilation.Db.QueryNamedTypeReferences(name, context);
            foreach (var nameType in nameTypes)
            {
                if (context.FindDeclaration(nameType) == symbol && nameType.Name is { } name2)
                {
                    references.Add(new ReferenceResult(name2.Location, name2));
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

            references.AddRange(LocalReferences(symbol, info));
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
