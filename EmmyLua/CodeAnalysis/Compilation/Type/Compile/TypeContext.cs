using EmmyLua.CodeAnalysis.Compilation.Type.Types;
using EmmyLua.CodeAnalysis.Diagnostics;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Type.Compile;

public class TypeContext(LuaCompilation compilation, LuaDocument document)
{
    public LuaCompilation Compilation { get; } = compilation;

    public LuaTypeManager TypeManager => Compilation.TypeManager;

    public LuaDocument Document { get; } = document;

    private Dictionary<SyntaxElementId, Dictionary<string, LuaType?>> GenericNames { get; } = new();

    private Dictionary<SyntaxElementId, SyntaxElementId> GenericEffectRanges { get; } = new();

    public LuaType? FindType(string name, SyntaxElementId commentId)
    {
        if (!GenericNames.TryGetValue(commentId, out var names))
        {
            var sourceId = GenericEffectRanges.GetValueOrDefault(commentId);
            if (!GenericNames.TryGetValue(sourceId, out names))
            {
                return FindDefinedType(name);
            }
        }

        if (names.TryGetValue(name, out var baseType))
        {
            return new LuaTplType(name, baseType);
        }

        return FindDefinedType(name);
    }

    private LuaType? FindDefinedType(string name)
    {
        var luaNamedType = new LuaNamedType(Document.Id, name);
        if (TypeManager.FindTypeInfo(luaNamedType) is not null)
        {
            return luaNamedType;
        }

        return null;
    }

    public void AddGenericName(SyntaxElementId id, string name, LuaType? baseType)
    {
        if (!GenericNames.TryGetValue(id, out var names))
        {
            names = new();
            GenericNames[id] = names;
        }

        names[name] = baseType;
    }

    public void AddGenericEffectId(SyntaxElementId sourceId, SyntaxElementId effectId)
    {
        GenericEffectRanges[effectId] = sourceId;
    }

    public void AddDiagnostic(Diagnostic diagnostic)
    {
        Compilation.Diagnostics.AddDiagnostic(Document.Id, diagnostic);
    }

    public void AddRealType(SyntaxElementId id, LuaType type)
    {
        Compilation.TypeManager.AddRealType(id, type);
    }
}
