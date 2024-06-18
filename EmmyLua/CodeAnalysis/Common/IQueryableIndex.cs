using EmmyLua.CodeAnalysis.Compilation.Type;

namespace EmmyLua.CodeAnalysis.Common;

public interface IQueryableIndex
{
    IEnumerable<IDeclaration> QueryAllGlobal();

    IEnumerable<IDeclaration> QueryMembers(LuaType type);

    IDeclaration? QueryGlobals(string name);

    IEnumerable<LuaType> QuerySupers(string name);

    IEnumerable<string> QuerySubTypes(string name);

    IEnumerable<IDeclaration> QueryNamedTypeDefinitions(string name);

    IEnumerable<IDeclaration> QueryGenericParams(string name);

    NamedTypeKind QueryNamedTypeKind(string name);

    IEnumerable<IDeclaration> QueryAllNamedTypeDefinitions();

    IEnumerable<TypeOperator> QueryTypeOperators(string name);

    IEnumerable<LuaMethodType> QueryTypeOverloads(string name);
}
