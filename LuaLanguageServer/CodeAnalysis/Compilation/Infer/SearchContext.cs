using LuaLanguageServer.CodeAnalysis.Compilation.StubIndex;
using LuaLanguageServer.CodeAnalysis.Compilation.Type;
using LuaLanguageServer.CodeAnalysis.Kind;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Infer;

public class SearchContext
{
    public LuaCompilation Compilation { get; }

    private Dictionary<LuaSyntaxElement, ILuaType> _caches = new();

    private Dictionary<LuaSyntaxElement, LuaTypeMember> _memberCaches = new();

    private Stack<Dictionary<string, ILuaType>> _envStack = new();

    public CallExprInfer CallExprInfer { get; } = new();

    public SearchContext(LuaCompilation compilation)
    {
        Compilation = compilation;
    }

    public ILuaType Infer(LuaSyntaxElement? element)
    {
        if (element is null)
        {
            return Compilation.Builtin.Unknown;
        }

        return _caches.TryGetValue(element, out var symbol) ? symbol : _caches[element] = InferCore(element);
    }

    private ILuaType InferCore(LuaSyntaxElement element)
    {
        return element switch
        {
            LuaExprSyntax expr => ExpressionInfer.InferExpr(expr, this),
            LuaLocalNameSyntax localName => DeclarationInfer.InferLocalName(localName, this),
            LuaParamDefSyntax paramDef => DeclarationInfer.InferParam(paramDef, this),
            LuaFuncStatSyntax funcStat => throw new NotImplementedException(),
            LuaSourceSyntax source => DeclarationInfer.InferSource(source, this),
            _ => Compilation.Builtin.Unknown
        };
    }

    public TMember? InferMember<TMember>(LuaSyntaxElement element, Func<TMember?> factory)
        where TMember : LuaTypeMember
    {
        if (_memberCaches.TryGetValue(element, out var member))
        {
            return member as TMember;
        }

        var result = factory();
        if (result is not null)
        {
            _memberCaches[element] = result;
        }

        return result;
    }

    public ILuaType InferTypeName(string name)
    {
        foreach (var env in _envStack)
        {
            if (env.TryGetValue(name, out var ty))
            {
                return ty;
            }
        }

        var elements = Compilation.StubIndexImpl.ShortNameIndex.Get(name);
        foreach (var luaShortName in elements)
        {
            switch(luaShortName)
            {
                case LuaShortName.Alias alias:
                    return Infer(alias.AliasSyntax);
                case LuaShortName.Class clazz:
                    return Infer(clazz.ClassSyntax);
                case LuaShortName.Enum enumType:
                    return Infer(enumType.EnumSyntax);
                case LuaShortName.Interface interfaceType:
                    return Infer(interfaceType.InterfaceSyntax);
            }
        }

        return Compilation.Builtin.Unknown;
    }

    public void PushEnv(Dictionary<string, ILuaType> env)
    {
        _envStack.Push(env);
    }

    public void PopEnv()
    {
        _envStack.Pop();
    }
}
