using LuaLanguageServer.CodeAnalysis.Compilation.Infer;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public abstract class Field : LuaType, ILuaNamedType
{
    public string Name { get; }

    public ILuaType? ContainingType { get; }

    public Field(string name, ILuaType? containingType) : base(TypeKind.Field)
    {
        Name = name;
        ContainingType = containingType;
    }

    public override IEnumerable<ILuaType> GetMembers(SearchContext context)
    {
        return GetType(context)?.GetMembers(context) ?? Enumerable.Empty<ILuaType>();
    }

    public abstract ILuaType? GetType(SearchContext context);
}

public class EnumField : Field
{
    public EnumField(string name, Enum containingType) : base(name, containingType)
    {
    }

    public override ILuaType GetType(SearchContext context)
    {
        return (ContainingType as Enum)?.GetBaseType(context) ?? context.Compilation.Builtin.Unknown;
    }
}

// public class InterfaceField : Field
// {
//     public InterfaceField(string name, Interface containingType) : base(name, containingType)
//     {
//     }
//
//     public override ILuaType GetType(SearchContext context)
//     {
//         return (ContainingType as Interface)?.GetBaseType(context) ?? context.Compilation.Builtin.Unknown;
//     }
// }
