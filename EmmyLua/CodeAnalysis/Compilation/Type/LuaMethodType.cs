using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Type;

public class LuaMultiReturnType(List<LuaType> retTypes) : LuaType(TypeKind.Return), IEquatable<LuaMultiReturnType>
{
    public List<LuaType> RetTypes { get; } = retTypes;

    public override bool Equals(object? obj)
    {
        return Equals(obj as LuaMultiReturnType);
    }

    public bool Equals(LuaMultiReturnType? other)
    {
        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return other is not null ? RetTypes.SequenceEqual(other.RetTypes) : base.Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), RetTypes);
    }
}

public class LuaSignature(LuaType returnType, List<ParameterDeclaration> parameters) : IEquatable<LuaSignature>
{
    public LuaType ReturnType { get; set; } = returnType;

    public List<ParameterDeclaration> Parameters { get; } = parameters;

    public bool Equals(LuaSignature? other)
    {
        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (other is not null)
        {
            return ReturnType.Equals(other.ReturnType) && Parameters.SequenceEqual(other.Parameters);
        }

        return false;
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as LuaSignature);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Parameters);
    }
}

public class LuaMethodType(LuaSignature mainSignature, List<LuaSignature>? overloads, bool colon)
    : LuaType(TypeKind.Method), IEquatable<LuaMethodType>
{
    public LuaSignature MainSignature { get; } = mainSignature;

    public List<LuaSignature>? Overloads { get; } = overloads;

    public bool Colon { get; } = colon;

    public LuaMethodType(LuaType returnType, List<ParameterDeclaration> parameters, bool colon)
        : this(new LuaSignature(returnType, parameters), null, colon)
    {
    }

    public LuaSignature FindPerfectMatchSignature(
        LuaCallExprSyntax callExpr,
        List<LuaExprSyntax> args,
        SearchContext context)
    {
        if (Overloads is null)
        {
            return MainSignature;
        }


        throw new NotImplementedException();
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as LuaMethodType);
    }

    public bool Equals(LuaMethodType? other)
    {
        if (ReferenceEquals(this, other))
        {
            return true;
        }

        if (other is not null)
        {
            return MainSignature.Equals(other.MainSignature) && Colon == other.Colon;
        }

        return base.Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(base.GetHashCode(), MainSignature, Colon);
    }
}
