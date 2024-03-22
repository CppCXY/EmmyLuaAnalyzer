using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Compilation.Type.DetailType;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using Microsoft.VisualBasic.CompilerServices;

namespace EmmyLua.CodeAnalysis.Compilation.Infer;

public class SearchContext(LuaCompilation compilation, bool allowCache = true)
{
    public LuaCompilation Compilation { get; } = compilation;

    private Dictionary<LuaSyntaxElement, LuaType> Caches { get; } = new();

    private Dictionary<LuaType, List<LuaDeclaration>> MemberCaches { get; } = new();

    private HashSet<LuaSyntaxElement> InferGuard { get; } = new();

    private const int MaxDepth = 1000;

    // 推断深度
    private int _currentDepth = 0;

    public LuaType Infer(LuaSyntaxElement? element)
    {
        if (element is null)
        {
            return Builtin.Unknown;
        }

        if (allowCache)
        {
            return Caches.TryGetValue(element, out var luaType) ? luaType : Caches[element] = InferCore(element);
        }
        else
        {
            return InferCore(element);
        }
    }

    public void ClearCache()
    {
        Caches.Clear();
        MemberCaches.Clear();
    }

    private LuaType InferCore(LuaSyntaxElement element)
    {
        if (_currentDepth > MaxDepth)
        {
            return Builtin.Unknown;
        }

        if (!InferGuard.Add(element))
        {
            return Builtin.Unknown;
        }

        try
        {
            _currentDepth++;
            return element switch
            {
                LuaExprSyntax expr => ExpressionInfer.InferExpr(expr, this),
                LuaLocalNameSyntax localName => DeclarationInfer.InferLocalName(localName, this),
                LuaParamDefSyntax paramDef => DeclarationInfer.InferParam(paramDef, this),
                LuaSourceSyntax source => DeclarationInfer.InferSource(source, this),
                LuaDocTypeSyntax ty => TypeInfer.InferType(ty, this),
                _ => Builtin.Unknown
            };
        }
        finally
        {
            _currentDepth--;
            InferGuard.Remove(element);
        }
    }

    private IEnumerable<LuaDeclaration> GetMembers(string name)
    {
        if (name is "_G" or "_ENV")
        {
            return Compilation.ProjectIndex.GetGlobals();
        }

        return Compilation.ProjectIndex.GetMembers(name);
    }

    public IEnumerable<LuaDeclaration> GetMembers(LuaType luaType)
    {
        if (luaType is LuaGenericType genericType)
        {
            return GetGenericMembers(genericType);
        }
        else if (luaType is LuaNamedType namedType)
        {
            return GetMembers(namedType.Name);
        }
        else if (luaType is LuaUnionType unionType)
        {
            return unionType.UnionTypes.SelectMany(GetMembers);
        }

        return Enumerable.Empty<LuaDeclaration>();
    }

    private IEnumerable<LuaDeclaration> GetGenericMembers(LuaGenericType genericType)
    {
        if (MemberCaches.TryGetValue(genericType, out var instanceMembers))
        {
            return instanceMembers;
        }

        var members = Compilation.ProjectIndex.GetMembers(genericType.Name);
        var genericParams = Compilation.ProjectIndex.GetGenericParams(genericType.Name).ToList();
        var genericArgs = genericType.GenericArgs;

        var genericMap = new Dictionary<string, LuaType>();
        for (var i = 0; i < genericParams.Count && i < genericArgs.Count; i++)
        {
            genericMap[genericParams[i].Name] = genericArgs[i];
        }

        instanceMembers = new List<LuaDeclaration>();
        foreach (var member in members)
        {
            var declarationType = member.DeclarationType;
            if (declarationType is not null)
            {
                declarationType = declarationType.Instantiate(genericMap);
                instanceMembers.Add(member.WithType(declarationType));
            }
            else
            {
                instanceMembers.Add(member);
            }
        }

        MemberCaches.TryAdd(genericType, instanceMembers);
        return instanceMembers;
    }

    public IEnumerable<LuaDeclaration> FindMember(LuaType luaType, string memberName)
    {
        if (luaType is LuaNamedType namedType)
        {
            var fieldDeclarations = GetMembers(namedType)
                .Where(it => string.Equals(it.Name, memberName, StringComparison.CurrentCulture));
            // TODO generic

            return fieldDeclarations;
        }
        else if (luaType is LuaUnionType unionType)
        {
            return unionType.UnionTypes.SelectMany(it => FindMember(it, memberName));
        }

        return Enumerable.Empty<LuaDeclaration>();
    }

    public IEnumerable<LuaDeclaration> FindMember(LuaType luaType, LuaIndexExprSyntax indexExpr)
    {
        var declarations = new List<LuaDeclaration>();
        if (indexExpr is { Name: { } name })
        {
            declarations.AddRange(FindMember(luaType, name));
        }

        if (declarations.Count == 0)
        {
            LuaType keyType = Builtin.Unknown;
            if (indexExpr.DotOrColonIndexName != null)
            {
                keyType = Builtin.String;
            }
            else if (indexExpr.IndexKeyExpr is LuaLiteralExprSyntax literal)
            {
                if (literal.Literal is LuaStringToken)
                {
                    keyType = Builtin.String;
                }
                else if (literal.Literal is LuaIntegerToken luaIntegerToken)
                {
                    keyType = Builtin.Integer;
                }
            }
            else
            {
                keyType = Infer(indexExpr.KeyElement);
            }

            var op = Compilation.ProjectIndex.TypeOperatorStorage.GetBestMatchedIndexOperator(luaType, keyType);
            if (op != null)
            {
                declarations.Add(new TypeIndexDeclaration(op.Ret, op.DefineElement as LuaDocTagFieldSyntax));
            }
        }

        return declarations;
    }
}
