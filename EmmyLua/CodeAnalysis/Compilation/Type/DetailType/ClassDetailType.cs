using EmmyLua.CodeAnalysis.Compilation.Declaration;

namespace EmmyLua.CodeAnalysis.Compilation.Type.DetailType;

public class ClassDetailType(
    string name,
    List<LuaType> supers,
    List<GenericParameterLuaDeclaration> generics,
    NamedTypeLuaDeclaration? declaration) : BasicDetailType(name, NamedTypeKind.Class)
{
    public List<LuaType> Supers { get; } = supers;

    public List<GenericParameterLuaDeclaration> Generics { get; } = generics;

    public NamedTypeLuaDeclaration? Declaration { get; } = declaration;
}
