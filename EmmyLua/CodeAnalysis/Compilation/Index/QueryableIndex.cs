using EmmyLua.CodeAnalysis.Compilation.Declaration;
using EmmyLua.CodeAnalysis.Compilation.Type;

namespace EmmyLua.CodeAnalysis.Compilation.Index;

public interface QueryableIndex
{
    IEnumerable<LuaDeclaration> QueryAllGlobal();

    IEnumerable<LuaDeclaration> QueryMembers(string name);

    IEnumerable<LuaDeclaration> QueryGlobals(string name);

    IEnumerable<LuaType> QuerySupers(string name);

    IEnumerable<string> QuerySubTypes(string name);

    IEnumerable<LuaDeclaration> QueryNamedTypeDefinitions(string name);

    IEnumerable<LuaType> QueryAliasOriginTypes(string name);

    IEnumerable<LuaDeclaration> QueryGenericParams(string name);

    NamedTypeKind QueryNamedTypeKind(string name);

    IEnumerable<LuaDeclaration> QueryAllNamedTypeDefinitions();

    IEnumerable<TypeOperator> QueryTypeOperators(string name);

    IEnumerable<LuaMethodType> QueryTypeOverloads(string name);
}
