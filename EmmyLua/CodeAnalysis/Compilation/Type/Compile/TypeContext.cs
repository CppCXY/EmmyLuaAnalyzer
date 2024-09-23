using EmmyLua.CodeAnalysis.Compilation.Type.Types;
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

    private Dictionary<SyntaxElementId, List<HashSet<string>>> GenericNames { get; } = new();

    private Dictionary<SyntaxElementId, RangeCollection> GenericEffectRanges { get; } = new();

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
        var commentRange = comment.Range;
        foreach (var (id, rangeCollection) in GenericEffectRanges)
        {
            if (rangeCollection.Contains(commentRange.StartOffset))
            {
                if (GenericNames.TryGetValue(id, out var names))
                {
                    foreach (var nameSet in names)
                    {
                        if (nameSet.Contains(name))
                        {
                            return new LuaTypeTemplate(name, null);
                        }
                    }
                }
            }
        }

        var luaNamedType = new LuaNamedType(comment.DocumentId, name);
        if (TypeManager.FindTypeInfo(luaNamedType) is not null)
        {
            return luaNamedType;
        }

        return null;
    }

    public void AddGenericName(SyntaxElementId id, string name)
    {
        if (!GenericNames.TryGetValue(id, out var names))
        {
            names = new List<HashSet<string>>();
            GenericNames[id] = names;
        }

        if (names.Count == 0)
        {
            names.Add(new HashSet<string>());
        }

        names[^1].Add(name);
    }

    public void AddGenericEffectRange(SyntaxElementId id, SourceRange range)
    {
        if (!GenericEffectRanges.TryGetValue(id, out var ranges))
        {
            ranges = new RangeCollection();
            GenericEffectRanges[id] = ranges;
        }

        ranges.AddRange(range);
    }
}
