using EmmyLua.CodeAnalysis.Common;
using EmmyLua.CodeAnalysis.Compilation.Search;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Document.Version;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Declaration;

public abstract record DeclarationInfo(
    LuaElementPtr<LuaSyntaxElement> Ptr,
    LuaType? DeclarationType
);

[Flags]
public enum DeclarationFeature
{
    None = 0,
    Deprecated = 0x01,
    Local = 0x02,
    Global = 0x04,
    NoDiscard = 0x08,
    Async = 0x10,
}

public enum DeclarationVisibility
{
    Public,
    Protected,
    Private,
    Package
}

public class LuaDeclaration(
    string name,
    DeclarationInfo info,
    DeclarationFeature feature = DeclarationFeature.None,
    DeclarationVisibility visibility = DeclarationVisibility.Public
)
    : IDeclaration
{
    public string Name { get; internal set; } = name;

    public LuaType Type => Info.DeclarationType ?? Builtin.Unknown;

    public DeclarationInfo Info { get; internal set; } = info;

    public DeclarationFeature Feature { get; internal set; } = feature;

    public bool IsDeprecated => Feature.HasFlag(DeclarationFeature.Deprecated);

    public bool IsLocal => Feature.HasFlag(DeclarationFeature.Local);

    public bool IsGlobal => Feature.HasFlag(DeclarationFeature.Global);

    public bool IsAsync => Feature.HasFlag(DeclarationFeature.Async);

    public bool IsNoDiscard => Feature.HasFlag(DeclarationFeature.NoDiscard);

    public DeclarationVisibility Visibility { get; internal set; } = visibility;

    public bool IsPublic => Visibility == DeclarationVisibility.Public;

    public bool IsProtected => Visibility == DeclarationVisibility.Protected;

    public bool IsPrivate => Visibility == DeclarationVisibility.Private;

    public bool IsPackage => Visibility == DeclarationVisibility.Package;

    public List<RequiredVersion>? RequiredVersions { get; set; }

    public LuaDeclaration WithInfo(DeclarationInfo otherInfo) =>
        new(Name, otherInfo, Feature, Visibility);

    public IDeclaration Instantiate(TypeSubstitution substitution)
    {
        if (Info.DeclarationType is { } type)
        {
            return WithInfo(Info with { DeclarationType = type.Instantiate(substitution) });
        }

        return this;
    }

    public SyntaxElementId UniqueId => Info.Ptr.UniqueId;

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

    public ILocation? GetLocation(SearchContext context)
    {
        var document = context.Compilation.Workspace.GetDocument(Info.Ptr.DocumentId);
        if (document is not null)
        {
            var range = document.SyntaxTree.GetSourceRange(Info.Ptr.ElementId);
            return new LuaLocation(document, range);
        }

        return null;
    }

    public string RelationInformation => Info.Ptr.Stringify;

    public LuaDocumentId DocumentId => Info.Ptr.DocumentId;

    public override string ToString()
    {
        return $"{Name}";
    }
}

public record LocalInfo(
    LuaElementPtr<LuaLocalNameSyntax> LocalNamePtr,
    LuaType? DeclarationType,
    bool IsConst = false,
    bool IsClose = false
) : DeclarationInfo(LocalNamePtr.UpCast(), DeclarationType)
{
    public LuaElementPtr<LuaLocalNameSyntax> LocalNamePtr => Ptr.Cast<LuaLocalNameSyntax>();
}

public record GlobalInfo(
    LuaElementPtr<LuaNameExprSyntax> VarNamePtr,
    LuaType? DeclarationType,
    bool TypeDecl = false
) : DeclarationInfo(VarNamePtr.UpCast(), DeclarationType)
{
    public LuaElementPtr<LuaNameExprSyntax> VarNamePtr => Ptr.Cast<LuaNameExprSyntax>();
}

public record ParamInfo(
    LuaElementPtr<LuaSyntaxElement> Ptr,
    LuaType? DeclarationType,
    bool IsVararg,
    bool Nullable = false
) : DeclarationInfo(Ptr, DeclarationType)
{
    public LuaElementPtr<LuaParamDefSyntax> ParamDefPtr => Ptr.Cast<LuaParamDefSyntax>();

    public LuaElementPtr<LuaDocTypedParamSyntax> TypedParamPtr => Ptr.Cast<LuaDocTypedParamSyntax>();
}

public record MethodInfo(
    LuaElementPtr<LuaSyntaxElement> Ptr,
    LuaMethodType? Method,
    LuaElementPtr<LuaFuncStatSyntax> FuncStatPtr
) : DeclarationInfo(Ptr, Method)
{
    public LuaElementPtr<LuaNameExprSyntax> NamePtr => Ptr.Cast<LuaNameExprSyntax>();

    public LuaElementPtr<LuaIndexExprSyntax> IndexPtr => Ptr.Cast<LuaIndexExprSyntax>();

    public LuaElementPtr<LuaLocalNameSyntax> LocalPtr => Ptr.Cast<LuaLocalNameSyntax>();

    public LuaMethodType? Method => DeclarationType as LuaMethodType;
}

public record NamedTypeInfo(
    LuaElementPtr<LuaDocTagNamedTypeSyntax> TypeDefinePtr,
    LuaType DeclarationType,
    NamedTypeKind Kind)
    : DeclarationInfo(TypeDefinePtr.UpCast(), DeclarationType)
{
    public LuaElementPtr<LuaDocTagNamedTypeSyntax> TypeDefinePtr => Ptr.Cast<LuaDocTagNamedTypeSyntax>();
}

public record DocFieldInfo(
    LuaElementPtr<LuaDocFieldSyntax> FieldDefPtr,
    LuaType? DeclarationType
) : DeclarationInfo(FieldDefPtr.UpCast(), DeclarationType)
{
    public LuaElementPtr<LuaDocFieldSyntax> FieldDefPtr => Ptr.Cast<LuaDocFieldSyntax>();
}

public record TableFieldInfo(
    LuaElementPtr<LuaTableFieldSyntax> TableFieldPtr,
    LuaType? DeclarationType
) : DeclarationInfo(TableFieldPtr.UpCast(), DeclarationType)
{
    public LuaElementPtr<LuaTableFieldSyntax> TableFieldPtr => Ptr.Cast<LuaTableFieldSyntax>();
}

public record EnumFieldInfo(
    LuaElementPtr<LuaDocTagEnumFieldSyntax> EnumFieldDefPtr,
    LuaType? DeclarationType
) : DeclarationInfo(EnumFieldDefPtr.UpCast(), DeclarationType)
{
    public LuaElementPtr<LuaDocTagEnumFieldSyntax> EnumFieldDefPtr => Ptr.Cast<LuaDocTagEnumFieldSyntax>();
}

public record GenericParamInfo(
    LuaElementPtr<LuaDocGenericParamSyntax> GenericParamDefPtr,
    LuaType? DeclarationType
) : DeclarationInfo(GenericParamDefPtr.UpCast(), DeclarationType)
{
    public LuaElementPtr<LuaDocGenericParamSyntax> GenericParamDefPtr => Ptr.Cast<LuaDocGenericParamSyntax>();
}

public record IndexInfo(
    LuaElementPtr<LuaIndexExprSyntax> IndexExprPtr,
    LuaElementPtr<LuaExprSyntax> ValueExprPtr,
    LuaType? DeclarationType
) : DeclarationInfo(IndexExprPtr.UpCast(), DeclarationType)
{
    public LuaElementPtr<LuaIndexExprSyntax> IndexExprPtr => Ptr.Cast<LuaIndexExprSyntax>();
}

public record TypeIndexInfo(
    LuaType KeyType,
    LuaType ValueType,
    LuaElementPtr<LuaDocFieldSyntax> FieldDefPtr)
    : DeclarationInfo(FieldDefPtr.UpCast(), ValueType)
{
    public LuaElementPtr<LuaDocFieldSyntax> FieldDefPtr => Ptr.Cast<LuaDocFieldSyntax>();

    public LuaType ValueType => DeclarationType!;
}

public record TypeOpInfo(
    LuaElementPtr<LuaDocTagOperatorSyntax> OpFieldPtr,
    LuaType? DeclarationType
) : DeclarationInfo(OpFieldPtr.UpCast(), DeclarationType)
{
    public LuaElementPtr<LuaDocTagOperatorSyntax> OpFieldPtr => Ptr.Cast<LuaDocTagOperatorSyntax>();
}

public record TupleMemberInfo(
    int Index,
    LuaType? DeclarationType,
    LuaElementPtr<LuaDocTypeSyntax> TypePtr)
    : DeclarationInfo(TypePtr.UpCast(), DeclarationType)
{
    public LuaElementPtr<LuaDocTypeSyntax> TypePtr => Ptr.Cast<LuaDocTypeSyntax>();
}

public record AggregateMemberInfo(
    LuaElementPtr<LuaDocTypeSyntax> TypePtr,
    LuaType? DeclarationType
) : DeclarationInfo(TypePtr.UpCast(), DeclarationType)
{
    public LuaElementPtr<LuaDocTypeSyntax> TypePtr => Ptr.Cast<LuaDocTypeSyntax>();
}

public record VirtualInfo(
    LuaType? DeclarationType
) : DeclarationInfo(LuaElementPtr<LuaSyntaxElement>.Empty, DeclarationType);
