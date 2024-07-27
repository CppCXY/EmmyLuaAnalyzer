using EmmyLua.CodeAnalysis.Compilation.Search;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Document.Version;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Type;

namespace EmmyLua.CodeAnalysis.Compilation.Symbol;

public abstract record SymbolInfo(
    LuaElementPtr<LuaSyntaxElement> Ptr
);

[Flags]
public enum SymbolFeature
{
    None = 0,
    Deprecated = 0x01,
    Local = 0x02,
    Global = 0x04,
    NoDiscard = 0x08,
    Async = 0x10,
}

public enum SymbolVisibility
{
    Public,
    Protected,
    Private,
    Package
}

public class LuaSymbol(
    string name,
    LuaType? type,
    SymbolInfo info,
    SymbolFeature feature = SymbolFeature.None,
    SymbolVisibility visibility = SymbolVisibility.Public
)
{
    public string Name { get; internal set; } = name;

    public LuaType? Type { get; set; } = type;

    public SymbolInfo Info { get; set; } = info;

    public SymbolFeature Feature { get; internal set; } = feature;

    public bool IsDeprecated => Feature.HasFlag(SymbolFeature.Deprecated);

    public bool IsLocal => Feature.HasFlag(SymbolFeature.Local);

    public bool IsGlobal => Feature.HasFlag(SymbolFeature.Global);

    public bool IsAsync => Feature.HasFlag(SymbolFeature.Async);

    public bool IsNoDiscard => Feature.HasFlag(SymbolFeature.NoDiscard);

    public SymbolVisibility Visibility { get; internal set; } = visibility;

    public bool IsPublic => Visibility == SymbolVisibility.Public;

    public bool IsProtected => Visibility == SymbolVisibility.Protected;

    public bool IsPrivate => Visibility == SymbolVisibility.Private;

    public bool IsPackage => Visibility == SymbolVisibility.Package;

    public List<RequiredVersion>? RequiredVersions { get; set; }

    public SyntaxElementId UniqueId => Info.Ptr.UniqueId;

    public string RelationInformation => Info.Ptr.Stringify;

    public LuaDocumentId DocumentId => Info.Ptr.DocumentId;

    public LuaSymbol WithInfo(SymbolInfo otherInfo) =>
        new(Name, Type, otherInfo, Feature, Visibility);

    public LuaSymbol WithType(LuaType? otherType) =>
        new(Name, otherType, Info, Feature, Visibility);

    public LuaSymbol Instantiate(TypeSubstitution substitution) =>
        new(Name, Type?.Instantiate(substitution), Info, Feature, Visibility);

    public bool ValidateLuaVersion(VersionNumber version)
    {
        var canValid = false;
        if (RequiredVersions is { } requiredVersions)
        {
            foreach (var requiredVersion in requiredVersions.Where(requiredVersion => requiredVersion.Name.Length == 0))
            {
                canValid = true;
                if (requiredVersion.IsMatch(version))
                {
                    return true;
                }
            }
        }

        return !canValid;
    }

    public bool ValidateFrameworkVersion(FrameworkVersion version)
    {
        var canValid = false;
        if (RequiredVersions is { } requiredVersions)
        {
            foreach (var requiredVersion in requiredVersions.Where(requiredVersion => requiredVersion.Name.Length != 0))
            {
                canValid = true;
                if (requiredVersion.IsMatch(version))
                {
                    return true;
                }
            }
        }

        return !canValid;
    }

    public bool ValidateFrameworkVersions(List<FrameworkVersion> versions)
    {
        if (versions.Count == 0)
        {
            return true;
        }

        return versions.Any(ValidateFrameworkVersion);
    }

    public virtual LuaLocation? GetLocation(SearchContext context)
    {
        var document = context.Compilation.Project.GetDocument(Info.Ptr.DocumentId);
        if (document is not null)
        {
            var range = document.SyntaxTree.GetSourceRange(Info.Ptr.ElementId);
            return document.GetLocation(range);
        }

        return null;
    }

    public override bool Equals(object? obj)
    {
        return obj is LuaSymbol symbol && Equals(symbol);
    }

    public bool Equals(LuaSymbol? obj)
    {
        return Info.Ptr == obj?.Info.Ptr;
    }

    public override int GetHashCode()
    {
        // ReSharper disable once NonReadonlyMemberInGetHashCode
        return Info.Ptr.GetHashCode();
    }

    public override string ToString()
    {
        return $"{Name}";
    }
}

public record LocalInfo(
    LuaElementPtr<LuaLocalNameSyntax> LocalNamePtr,
    bool IsConst = false,
    bool IsClose = false
) : SymbolInfo(LocalNamePtr.UpCast())
{
    public LuaElementPtr<LuaLocalNameSyntax> LocalNamePtr => Ptr.Cast<LuaLocalNameSyntax>();
}

public record GlobalInfo(
    LuaElementPtr<LuaNameExprSyntax> VarNamePtr
) : SymbolInfo(VarNamePtr.UpCast())
{
    public LuaElementPtr<LuaNameExprSyntax> VarNamePtr => Ptr.Cast<LuaNameExprSyntax>();
}

public record ParamInfo(
    LuaElementPtr<LuaSyntaxElement> Ptr,
    bool IsVararg,
    bool Nullable = false
) : SymbolInfo(Ptr)
{
    public LuaElementPtr<LuaParamDefSyntax> ParamDefPtr => Ptr.Cast<LuaParamDefSyntax>();

    public LuaElementPtr<LuaDocTypedParamSyntax> TypedParamPtr => Ptr.Cast<LuaDocTypedParamSyntax>();
}

public record MethodInfo(
    LuaElementPtr<LuaSyntaxElement> Ptr,
    LuaElementPtr<LuaFuncStatSyntax> FuncStatPtr
) : SymbolInfo(Ptr)
{
    public LuaElementPtr<LuaNameExprSyntax> NamePtr => Ptr.Cast<LuaNameExprSyntax>();

    public LuaElementPtr<LuaIndexExprSyntax> IndexPtr => Ptr.Cast<LuaIndexExprSyntax>();

    public LuaElementPtr<LuaLocalNameSyntax> LocalPtr => Ptr.Cast<LuaLocalNameSyntax>();
}

public record NamedTypeInfo(
    LuaElementPtr<LuaDocTagNamedTypeSyntax> TypeDefinePtr,
    NamedTypeKind Kind)
    : SymbolInfo(TypeDefinePtr.UpCast())
{
    public LuaElementPtr<LuaDocTagNamedTypeSyntax> TypeDefinePtr => Ptr.Cast<LuaDocTagNamedTypeSyntax>();
}

public record DocFieldInfo(
    LuaElementPtr<LuaDocFieldSyntax> FieldDefPtr
) : SymbolInfo(FieldDefPtr.UpCast())
{
    public LuaElementPtr<LuaDocFieldSyntax> FieldDefPtr => Ptr.Cast<LuaDocFieldSyntax>();
}

public record TableFieldInfo(
    LuaElementPtr<LuaTableFieldSyntax> TableFieldPtr
) : SymbolInfo(TableFieldPtr.UpCast())
{
    public LuaElementPtr<LuaTableFieldSyntax> TableFieldPtr => Ptr.Cast<LuaTableFieldSyntax>();
}

public record EnumFieldInfo(
    LuaElementPtr<LuaDocTagEnumFieldSyntax> EnumFieldDefPtr
) : SymbolInfo(EnumFieldDefPtr.UpCast())
{
    public LuaElementPtr<LuaDocTagEnumFieldSyntax> EnumFieldDefPtr => Ptr.Cast<LuaDocTagEnumFieldSyntax>();
}

public record GenericParamInfo(
    LuaElementPtr<LuaDocGenericParamSyntax> GenericParamDefPtr
) : SymbolInfo(GenericParamDefPtr.UpCast())
{
    public LuaElementPtr<LuaDocGenericParamSyntax> GenericParamDefPtr => Ptr.Cast<LuaDocGenericParamSyntax>();
}

public record IndexInfo(
    LuaElementPtr<LuaIndexExprSyntax> IndexExprPtr,
    LuaElementPtr<LuaExprSyntax> ValueExprPtr
) : SymbolInfo(IndexExprPtr.UpCast())
{
    public LuaElementPtr<LuaIndexExprSyntax> IndexExprPtr => Ptr.Cast<LuaIndexExprSyntax>();
}

public record TypeIndexInfo(
    LuaType KeyType,
    LuaType ValueType,
    LuaElementPtr<LuaDocFieldSyntax> FieldDefPtr)
    : SymbolInfo(FieldDefPtr.UpCast())
{
    public LuaElementPtr<LuaDocFieldSyntax> FieldDefPtr => Ptr.Cast<LuaDocFieldSyntax>();
}

public record TypeOpInfo(
    LuaElementPtr<LuaDocTagOperatorSyntax> OpFieldPtr
) : SymbolInfo(OpFieldPtr.UpCast())
{
    public LuaElementPtr<LuaDocTagOperatorSyntax> OpFieldPtr => Ptr.Cast<LuaDocTagOperatorSyntax>();
}

public record TupleMemberInfo(
    int Index,
    LuaElementPtr<LuaDocTypeSyntax> TypePtr)
    : SymbolInfo(TypePtr.UpCast())
{
    public LuaElementPtr<LuaDocTypeSyntax> TypePtr => Ptr.Cast<LuaDocTypeSyntax>();
}

public record AggregateMemberInfo(
    LuaElementPtr<LuaDocTypeSyntax> TypePtr
) : SymbolInfo(TypePtr.UpCast())
{
    public LuaElementPtr<LuaDocTypeSyntax> TypePtr => Ptr.Cast<LuaDocTypeSyntax>();
}

public record VirtualInfo() : SymbolInfo(LuaElementPtr<LuaSyntaxElement>.Empty);
