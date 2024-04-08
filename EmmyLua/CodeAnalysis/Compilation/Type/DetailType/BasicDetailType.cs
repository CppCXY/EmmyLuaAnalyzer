using EmmyLua.CodeAnalysis.Compilation.Index;
using EmmyLua.CodeAnalysis.Compilation.Infer;

namespace EmmyLua.CodeAnalysis.Compilation.Type.DetailType;

public class BasicDetailType(string name, NamedTypeKind kind, SearchContext context)
{
    public string Name { get; } = name;

    public NamedTypeKind Kind { get; } = kind;

    protected SearchContext Context { get; } = context;

    public bool IsEnum => Kind == NamedTypeKind.Enum;

    public bool IsInterface => Kind == NamedTypeKind.Interface;

    public bool IsClass => Kind == NamedTypeKind.Class;

    public bool IsAlias => Kind == NamedTypeKind.Alias;

    protected bool LazyInit { get; set; }

    protected ProjectIndex Index => Context.Compilation.ProjectIndex;

    protected virtual void DoLazyInit()
    {
        LazyInit = true;
    }
}
