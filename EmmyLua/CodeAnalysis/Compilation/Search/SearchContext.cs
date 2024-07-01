using EmmyLua.CodeAnalysis.Common;
using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Search;

public class SearchContext
{
    public LuaCompilation Compilation { get; }

    internal SearchContextFeatures Features { get; set; }

    private Declarations Declarations { get; }

    private Members Members { get; }

    private References References { get; }

    private Operators Operators { get; }

    private ElementInfer ElementInfer { get; }

    private SubTypeInfer SubTypeInfer { get; }

    public SearchContext(LuaCompilation compilation, SearchContextFeatures features)
    {
        Compilation = compilation;
        Declarations = new Declarations(this);
        Members = new Members(this);
        References = new References(this);
        Operators = new Operators(this);
        ElementInfer = new ElementInfer(this);
        SubTypeInfer = new SubTypeInfer(this);
        Features = features;
    }

    public LuaType Infer(LuaSyntaxElement? element)
    {
        return ElementInfer.Infer(element);
    }

    public LuaType InferAndUnwrap(LuaSyntaxElement? element)
    {
        return Infer(element).UnwrapType(this);
    }

    public void ClearMemberCache(LuaType luaType)
    {
        Members.ClearMember(luaType);
    }

    public BinaryOperator? GetBestMatchedBinaryOperator(TypeOperatorKind kind, LuaType left, LuaType right)
    {
        if (left is not LuaNamedType namedType)
        {
            return null;
        }

        var operators = Operators.GetOperators(kind, namedType);

        var bestMatched = operators
            .OfType<BinaryOperator>()
            .FirstOrDefault(it => it.Right.Equals(right));
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

    public IndexOperator? GetBestMatchedIndexOperator(LuaType type, LuaType key)
    {
        if (type is not LuaNamedType namedType)
        {
            return null;
        }

        var operators = Operators.GetOperators(TypeOperatorKind.Index, namedType);
        var bestMatched = operators
            .OfType<IndexOperator>()
            .FirstOrDefault(it => it.Key.Equals(key));
        return bestMatched;
    }

    public IDeclaration? FindDeclaration(LuaSyntaxElement? element)
    {
        return Declarations.FindDeclaration(element);
    }

    public IEnumerable<LuaDeclaration> GetDocumentLocalDeclarations(LuaDocumentId documentId)
    {
        return Compilation.Db.QueryDocumentLocalDeclarations(documentId);
    }

    public LuaType InferExprShouldBeType(LuaExprSyntax expr)
    {
        return ExpressionShouldBeInfer.InferExprShouldBe(expr, this);
    }

    public void FindMethodsForType(LuaType type, Action<LuaMethodType> action)
    {
        type = type.UnwrapType(this);
        switch (type)
        {
            case LuaUnionType unionType:
            {
                foreach (var t in unionType.UnionTypes)
                {
                    InnerFindMethods(t, action);
                }

                break;
            }
            default:
            {
                InnerFindMethods(type, action);
                break;
            }
        }
    }

    private void InnerFindMethods(LuaType type, Action<LuaMethodType> action)
    {
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
                var overloads = Compilation.Db.QueryTypeOverloads(namedType.Name);
                foreach (var methodType in overloads)
                {
                    founded = true;
                    action(methodType);
                }

                if (!founded && !Compilation.Workspace.Features.TypeCallStrict)
                {
                    var luaMethod = new LuaMethodType(namedType, [], false);
                    action(luaMethod);
                }
                break;
            }
        }
    }

    public IEnumerable<IDeclaration> GetMembers(LuaType type)
    {
        return Members.GetMembers(type).GroupBy(m => m.Name).Select(g => g.First());
    }

    public IEnumerable<IDeclaration> GetSuperMembers(LuaType type)
    {
        return Members.GetSupersMembers(type).GroupBy(m => m.Name).Select(g => g.First());
    }

    public IEnumerable<IDeclaration> FindMember(LuaType type, string name)
    {
        return Members.FindMember(type, name);
    }

    public IEnumerable<IDeclaration> FindMember(LuaType type, LuaIndexExprSyntax indexExpr)
    {
        return Members.FindMember(type, indexExpr);
    }

    public IEnumerable<IDeclaration> FindSuperMember(LuaType type, string name)
    {
        return Members.FindSuperMember(type, name);
    }

    public IEnumerable<ReferenceResult> FindReferences(IDeclaration declaration)
    {
        return References.FindReferences(declaration);
    }

    public bool IsUpValue(LuaNameExprSyntax nameExpr, LuaDeclaration declaration)
    {
        return Declarations.IsUpValue(nameExpr, declaration);
    }

    public bool IsSubTypeOf(LuaType left, LuaType right)
    {
        return SubTypeInfer.IsSubTypeOf(left, right);
    }

    public LuaSignature FindPerfectMatchSignature(LuaMethodType methodType, LuaCallExprSyntax callExpr, List<LuaExprSyntax> args)
    {
        return MethodInfer.FindPerfectMatchSignature(methodType, callExpr, args, this);
    }
}
