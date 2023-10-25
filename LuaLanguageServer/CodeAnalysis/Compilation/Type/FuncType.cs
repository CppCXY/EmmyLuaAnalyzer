using LuaLanguageServer.CodeAnalysis.Compilation.Infer;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public class FuncType : LuaType
{
    public IFuncSignature MainSignature { get; private set; }

    public List<IFuncSignature> Signatures { get; } = new();

    public bool IsColonCall { get; private set; }

    public FuncType(IFuncSignature mainSignature) : base(TypeKind.Func)
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

    public IFuncSignature FindPerfectSignature(IEnumerable<ILuaType> arguments, SearchContext context)
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

    public override IEnumerable<InterfaceMember?> GetMembers(SearchContext context)
    {
        throw new NotImplementedException();
    }
}

public interface IFuncSignature
{
    public bool ColonCall { get; }

    public ILuaType? ReturnType { get; }

    public IEnumerable<string> Parameters { get; }

    public string DisplayName { get; }

    public string ParamSignature { get; }

    public ILuaType? Variadic { get; }

    public int Match(IEnumerable<ILuaType> arguments, SearchContext context);
}
