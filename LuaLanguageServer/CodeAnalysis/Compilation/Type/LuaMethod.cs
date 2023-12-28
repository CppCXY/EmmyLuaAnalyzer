using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Declaration;
using LuaLanguageServer.CodeAnalysis.Compilation.Analyzer.Infer;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public class LuaMethod(IFuncSignature mainSignature) : LuaType(TypeKind.Method)
{
    public IFuncSignature MainSignature { get; private set; } = mainSignature;

    public List<IFuncSignature> Signatures { get; } = new();

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

    public override IEnumerable<Declaration> GetMembers(SearchContext context) => Enumerable.Empty<Declaration>();

}

public class FuncTypedParam(string name, ILuaType? type)
{
    public string Name { get; } = name;

    public ILuaType? Type { get; } = type;
}

public interface IFuncSignature
{
    public bool ColonCall { get; }

    public ILuaType? ReturnType { get; }

    public List<FuncTypedParam> Parameters { get; }

    public ILuaType? Variadic { get; }

    public int Match(List<LuaExprSyntax> arguments, SearchContext context);
}

public class FuncSignature(
    bool colonCall,
    List<FuncTypedParam> parameters,
    ILuaType? variadic,
    ILuaType? returnType)
    : IFuncSignature
{
    public bool ColonCall { get; } = colonCall;

    public ILuaType? ReturnType { get; } = returnType;

    public List<FuncTypedParam> Parameters { get; } = parameters;

    public ILuaType? Variadic { get; } = variadic;

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
