using EmmyLua.CodeAnalysis.Compilation.Declaration;

namespace EmmyLua.CodeAnalysis.Compilation.Type.DetailType;

public class InterfaceDetailType(
    string name,
    List<LuaType> supers,
    List<GenericParameterLuaDeclaration> generics,
    NamedTypeLuaDeclaration? declaration) : BasicDetailType(name, NamedTypeKind.Interface)
{
    public List<LuaType> Supers { get; } = supers;

    public List<GenericParameterLuaDeclaration> Generics { get; } = generics;

    public NamedTypeLuaDeclaration? Declaration { get; } = declaration;
}
