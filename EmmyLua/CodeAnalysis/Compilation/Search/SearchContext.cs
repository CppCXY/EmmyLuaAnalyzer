using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Type;


namespace EmmyLua.CodeAnalysis.Compilation.Search;

public class SearchContext
{
    public LuaCompilation Compilation { get; }

    internal SearchContextFeatures Features { get; set; }

    private Declarations Declarations { get; }

    private Members Members { get; }

    private IndexMembers IndexMembers { get; }

    private References References { get; }

    private Operators Operators { get; }

    private ElementInfer ElementInfer { get; }

    private SubTypeInfer SubTypeInfer { get; }

    private SameTypeInfer SameTypeInfer { get; }

    public SearchContext(LuaCompilation compilation, SearchContextFeatures features)
    {
        Compilation = compilation;
        Declarations = new Declarations(this);
        Members = new Members(this);
        IndexMembers = new IndexMembers(this);
        References = new References(this);
        Operators = new Operators(this);
        ElementInfer = new ElementInfer(this);
        SubTypeInfer = new SubTypeInfer(this);
        SameTypeInfer = new SameTypeInfer(this);
        Features = features;
    }

    public LuaType Infer(LuaSyntaxElement? element)
    {
        return ElementInfer.Infer(element);
    }

    // public void ClearMemberCache(LuaType luaType)
    // {
    //     // Members.ClearMember(luaType);
    // }

    public BinaryOperator? GetBestMatchedBinaryOperator(TypeOperatorKind kind, LuaType left, LuaType right)
    {
        if (left is not LuaNamedType namedType)
        {
            return null;
        }

        var operators = Operators.GetOperators(kind, namedType);

        var bestMatched = operators
            .OfType<BinaryOperator>()
            .FirstOrDefault(it => it.Right.IsSameType(right, this));
        return bestMatched;
    }

    public UnaryOperator? GetBestMatchedUnaryOperator(TypeOperatorKind kind, LuaType type)
    {
        if (type is not LuaNamedType namedType)
        {
            return null;
        }

        var operators = Operators.GetOperators(kind, namedType);
        return operators.OfType<UnaryOperator>().FirstOrDefault();
    }

    public IndexOperator? GetBestMatchedIndexOperator(LuaNamedType namedType, LuaNamedType key)
    {
        var operators = Operators.GetOperators(TypeOperatorKind.Index, namedType);
        var bestMatched = operators
            .OfType<IndexOperator>()
            .FirstOrDefault(it => it.Key.IsSameType(key, this));
        return bestMatched;
    }

    public LuaSymbol? FindDeclaration(LuaSyntaxElement? element)
    {
        return Declarations.FindDeclaration(element);
    }

    public IEnumerable<LuaSymbol> GetDocumentLocalDeclarations(LuaDocumentId documentId)
    {
        return Compilation.Db.QueryDocumentLocalDeclarations(documentId);
    }

    public LuaType InferExprShouldBeType(LuaExprSyntax expr)
    {
        return ExpressionShouldBeInfer.InferExprShouldBe(expr, this);
    }

    public List<LuaMethodType> FindCallableType(LuaType? type)
    {
        var methods = new List<LuaMethodType>();
        var action = new Action<LuaMethodType>(methods.Add);
        switch (type)
        {
            case LuaUnionType unionType:
            {
                foreach (var t in unionType.UnionTypes)
                {
                    InnerFindMethods(t, action, 0);
                }

                break;
            }
            default:
            {
                InnerFindMethods(type, action, 0);
                break;
            }
        }

        return methods;
    }

    private void InnerFindMethods(LuaType? type, Action<LuaMethodType> action, int level)
    {
        if (level > 3)
        {
            return;
        }

        switch (type)
        {
            case LuaMethodType methodType:
            {
                action(methodType);
                break;
            }
            case LuaNamedType namedType:
            {
                var founded = false;
                var typeInfo = Compilation.TypeManager.FindTypeInfo(namedType);
                if (typeInfo?.Overloads is { } overloads)
                {
                    foreach (var stub in overloads)
                    {
                        founded = true;
                        action(stub.MethodType);
                    }
                }

                if (!founded && !Compilation.Project.Features.TypeCallStrict)
                {
                    var luaMethod = new LuaMethodType(namedType, [], false);
                    action(luaMethod);
                }

                break;
            }
            case LuaElementType elementType:
            {
                var baseType = Compilation.TypeManager.GetBaseType(elementType.Id);
                if (baseType is not null)
                {
                    InnerFindMethods(baseType, action, level + 1);
                }

                break;
            }
        }
    }

    public List<LuaSymbol> GetMembers(LuaType type)
    {
        return Members.GetTypeMembers(type);
    }

    public LuaSymbol? FindMember(LuaType type, string name)
    {
        return IndexMembers.FindTypeMember(type, name);
    }

    public LuaSymbol? FindMember(SyntaxElementId id, string name)
    {
        var elementType = new LuaElementType(id);
        return IndexMembers.FindTypeMember(elementType, name);
    }

    public LuaSymbol? FindMember(LuaType type, LuaIndexExprSyntax indexExpr)
    {
        return IndexMembers.FindTypeMember(type, indexExpr);
    }

    public List<LuaSymbol> GetSuperMembers(LuaNamedType type)
    {
        return Members.GetSuperMembers(type);
    }

    public LuaSymbol? FindSuperMember(LuaType type, string name)
    {
        return IndexMembers.FindSuperMember(type, name);
    }

    public IEnumerable<ReferenceResult> FindReferences(LuaSymbol luaSymbol)
    {
        return References.FindReferences(luaSymbol);
    }

    public bool IsUpValue(LuaNameExprSyntax nameExpr, LuaSymbol symbol)
    {
        return Declarations.IsUpValue(nameExpr, symbol);
    }

    public bool IsSubTypeOf(LuaType left, LuaType right)
    {
        return SubTypeInfer.IsSubTypeOf(left, right);
    }

    public bool IsSameType(LuaType left, LuaType right)
    {
        return SameTypeInfer.IsSameType(left, right);
    }

    public LuaSignature FindPerfectMatchSignature(LuaMethodType methodType, LuaCallExprSyntax callExpr,
        List<LuaExprSyntax> args)
    {
        return MethodInfer.FindPerfectMatchSignature(methodType, callExpr, args, this);
    }
}
