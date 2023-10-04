using LuaLanguageServer.CodeAnalysis.Compilation.Infer;
using LuaLanguageServer.CodeAnalysis.Syntax.Location;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Symbol.Impl;

public class FuncSymbol : LuaSymbol
{
    public IFuncSignature MainSignature { get; private set; }

    public List<IFuncSignature> Signatures { get; } = new();

    public bool IsColonCall { get; private set; }

    public FuncSymbol(IFuncSignature mainSignature) : base(SymbolKind.Func)
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

    public IFuncSignature FindPerfectSignature(IEnumerable<ILuaSymbol> arguments, SearchContext context)
    {
        var perfectSignature = MainSignature;
        var perfectCount = 0;
        ProcessSignature(signature =>
        {
            var count = signature.Match(arguments, context);

            if (count > perfectCount)
            {
                perfectSignature = signature;
                perfectCount = count;
            }

            return true;
        });

        return perfectSignature;
    }
}

public interface IFuncSignature
{
    public bool ColonCall { get; }

    public ILuaSymbol? ReturnType { get; }

    public IEnumerable<ILuaSymbol> Parameters { get; }

    public string DisplayName { get; }

    public string ParamSignature { get; }

    public ILuaSymbol? Variadic { get; }

    public int Match(IEnumerable<ILuaSymbol> arguments, SearchContext context);
}
