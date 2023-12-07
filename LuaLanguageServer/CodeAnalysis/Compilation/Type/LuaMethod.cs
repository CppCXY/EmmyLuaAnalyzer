using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Infer;
using LuaLanguageServer.CodeAnalysis.Compilation.Symbol;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public class LuaMethod : LuaType
{
    public IFuncSignature MainSignature { get; private set; }

    public List<IFuncSignature> Signatures { get; } = new();

    public LuaMethod(IFuncSignature mainSignature) : base(TypeKind.Method)
    {
        MainSignature = mainSignature;
    }

    public void ProcessSignature(Func<IFuncSignature, bool> process)
    {
        if (process(MainSignature))
        {
            foreach (var signature in Signatures)
            {
                if (!process(signature))
                {
                    break;
                }
            }
        }
    }

    public IFuncSignature FindPerfectSignature(IEnumerable<LuaExprSyntax> arguments, SearchContext context)
    {
        var perfectSignature = MainSignature;
        var perfectCount = 0;
        var argumentsList = arguments.ToList();
        ProcessSignature(signature =>
        {
            var count = signature.Match(argumentsList, context);

            if (count > perfectCount)
            {
                perfectSignature = signature;
                perfectCount = count;
            }

            return true;
        });

        return perfectSignature;
    }

    public override IEnumerable<ILuaSymbol> GetMembers(SearchContext context) => Enumerable.Empty<LuaSymbol>();
}

public class FuncTypedParam
{
    public string Name { get; }

    public ILuaType? Type { get; }

    public FuncTypedParam(string name, ILuaType? type)
    {
        Name = name;
        Type = type;
    }
}

public interface IFuncSignature
{
    public bool ColonCall { get; }

    public ILuaType? ReturnType { get; }

    public List<FuncTypedParam> Parameters { get; }

    public ILuaType? Variadic { get; }

    public int Match(List<LuaExprSyntax> arguments, SearchContext context);
}

public class FuncSignature : IFuncSignature
{
    public bool ColonCall { get; }

    public ILuaType? ReturnType { get; }

    public List<FuncTypedParam> Parameters { get; }

    public ILuaType? Variadic { get; }

    public FuncSignature(
        bool colonCall,
        List<FuncTypedParam> parameters,
        ILuaType? variadic,
        ILuaType? returnType)
    {
        ColonCall = colonCall;
        ReturnType = returnType;
        Parameters = parameters;
        Variadic = variadic;
    }

    public int Match(List<LuaExprSyntax> arguments, SearchContext context)
    {
        var matched = 0;
        for(; matched < Parameters.Count; matched++)
        {
            if (arguments.Count <= matched)
            {
                return matched;
            }

            var arg = arguments[matched];
            var param = Parameters[matched];
            if (param.Type is { } type)
            {
                if (!type.AcceptExpr(arg, context))
                {
                    return matched;
                }
            }
        }

        if (arguments.Count > matched && Variadic != null)
        {
            for (; matched < arguments.Count; matched++)
            {
                var arg = arguments[matched];
                if (!Variadic.AcceptExpr(arg, context))
                {
                    return matched;
                }
            }
        }

        return matched;
    }
}
