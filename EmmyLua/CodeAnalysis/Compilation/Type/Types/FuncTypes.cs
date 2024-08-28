using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Compilation.Search;
using EmmyLua.CodeAnalysis.Compilation.Signature;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Type.Types;


public class LuaMultiReturnType : LuaType
{
    private List<LuaType>? RetTypes { get; }

    private LuaType? BaseType { get; }

    public LuaMultiReturnType(LuaType baseType)
    {
        BaseType = baseType;
    }

    public LuaMultiReturnType(List<LuaType> retTypes)
    {
        RetTypes = retTypes;
    }

    public LuaType GetElementType(int id)
    {
        if (RetTypes?.Count > id)
        {
            return RetTypes[id];
        }

        return BaseType ?? Builtin.Nil;
    }

    public int GetElementCount()
    {
        return RetTypes?.Count ?? 0;
    }

    public override LuaType Instantiate(TypeSubstitution substitution)
    {
        if (RetTypes is not null)
        {
            var returnTypes = new List<LuaType>();
            foreach (var retType in RetTypes)
            {
                var substituteType = retType.Instantiate(substitution);
                if (substituteType is LuaMultiReturnType multiReturnType)
                {
                    if (multiReturnType.RetTypes is { } retTypes)
                    {
                        returnTypes.AddRange(retTypes);
                    }
                    else if (multiReturnType.BaseType is { } baseType)
                    {
                        returnTypes.Add(baseType);
                    }
                }
                else
                {
                    returnTypes.Add(substituteType);
                }
            }

            return new LuaMultiReturnType(returnTypes);
        }
        else
        {
            return new LuaMultiReturnType(BaseType!.Instantiate(substitution));
        }
    }
}

public class LuaMethodType(LuaSignatureId id)
    : LuaType
{
    public LuaSignatureId SignatureId { get; } = id;
}

