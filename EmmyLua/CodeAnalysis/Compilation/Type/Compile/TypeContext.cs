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

    private RangeCollection IgnoreRanges { get; } = new();

    private Dictionary<SyntaxElementId, Dictionary<string, LuaType?>> GenericNames { get; } = new();

    private Dictionary<SyntaxElementId, SyntaxElementId> GenericEffectRanges { get; } = new();

    public void AddIgnoreRange(SourceRange range)
    {
        IgnoreRanges.AddRange(range);
    }

    public bool IsIgnoreRange(SourceRange range)
    {
        return IgnoreRanges.Contains(range.StartOffset);
    }

    public LuaType? FindType(string name, LuaCommentSyntax comment)
    {
        var id = comment.UniqueId;
        if (!GenericNames.TryGetValue(id, out var names))
        {
            var sourceId = GenericEffectRanges.GetValueOrDefault(id);
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
        Compilation.Diagnostics.AddDiagnostic(document.Id, diagnostic);
    }
}
