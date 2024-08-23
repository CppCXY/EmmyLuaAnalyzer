using EmmyLua.CodeAnalysis.Compilation.Search;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Document.Version;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Type;
using EmmyLua.CodeAnalysis.Type.Types;

namespace EmmyLua.CodeAnalysis.Compilation.Symbol;

public interface ISymbolInfo
{
    public LuaElementPtr<LuaSyntaxElement> Ptr { get; }
};

[Flags]
public enum SymbolFeature
{
    None = 0,
    Deprecated = 0x01,
    Local = 0x02,
    Global = 0x04,
    NoDiscard = 0x08,
    Async = 0x10,
    Source = 0x20,
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
    ISymbolInfo info,
    SymbolFeature feature = SymbolFeature.None,
    SymbolVisibility visibility = SymbolVisibility.Public
)
{
    public string Name { get; internal set; } = name;

    public LuaType? Type { get; set; } = type;

    public ISymbolInfo Info { get; set; } = info;

    public SymbolFeature Feature { get; internal set; } = feature;

    public bool IsDeprecated => Feature.HasFlag(SymbolFeature.Deprecated);

    public bool IsLocal => Feature.HasFlag(SymbolFeature.Local);

    public bool IsGlobal => Feature.HasFlag(SymbolFeature.Global);

    public bool IsAsync => Feature.HasFlag(SymbolFeature.Async);

    public bool IsNoDiscard => Feature.HasFlag(SymbolFeature.NoDiscard);

    public bool HasSource => Feature.HasFlag(SymbolFeature.Source);

    public SymbolVisibility Visibility { get; internal set; } = visibility;

    public bool IsPublic => Visibility == SymbolVisibility.Public;

    public bool IsProtected => Visibility == SymbolVisibility.Protected;

    public bool IsPrivate => Visibility == SymbolVisibility.Private;

    public bool IsPackage => Visibility == SymbolVisibility.Package;

    public List<RequiredVersion>? RequiredVersions { get; set; }

    public SyntaxElementId UniqueId => Info.Ptr.UniqueId;

    public string RelationInformation => Info.Ptr.Stringify;

    public LuaDocumentId DocumentId => Info.Ptr.DocumentId;

    public LuaSymbol WithInfo(ISymbolInfo otherInfo) =>
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
        if (HasSource)
        {
            var source = context.Compilation.Db.QuerySource(UniqueId);
            if (source is not null)
            {
                var parts = source.Split('#');
                var uri = string.Empty;
                var lineColStr = string.Empty;
                var line = 0;
                var col = 0;
                if (parts.Length == 2)
                {
                    uri = parts[0];
                    lineColStr = parts[1];
                    if (lineColStr.StartsWith('L'))
                    {
                        lineColStr = lineColStr[1..];
                    }
                }

                if (lineColStr.IndexOf(':') > 0)
                {
                    var lineCol = lineColStr.Split(':');
                    if (lineCol.Length == 2 && int.TryParse(lineCol[0], out var l) && int.TryParse(lineCol[1], out var c))
                    {
                        line = l;
                        col = c;
                    }
                }
                else if (int.TryParse(lineColStr, out var l))
                {
                    line = l;
                }

                if (uri.Length > 0)
                {
                    return new LuaLocation(line, col, line, col + 1, uri);
                }
            }
        }

        var document = context.Compilation.Project.GetDocument(Info.Ptr.DocumentId);
        if (document is not null)
        {
            var range = document.SyntaxTree.GetSourceRange(Info.Ptr.ElementId);
            return document.GetLocation(range);
        }

        return null;
    }

    public override int GetHashCode()
    {
        // ReSharper disable once NonReadonlyMemberInGetHashCode
        return Info.Ptr.GetHashCode();
    }

    public bool IsReferenceTo(LuaSymbol other)
    {
        return Info.Ptr.UniqueId == other.Info.Ptr.UniqueId;
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
) : ISymbolInfo
{
    public LuaElementPtr<LuaSyntaxElement> Ptr => LocalNamePtr.UpCast();
}

public record GlobalInfo(
    LuaElementPtr<LuaNameExprSyntax> VarNamePtr
) : ISymbolInfo
{
    public LuaElementPtr<LuaSyntaxElement> Ptr => VarNamePtr.UpCast();
}

public record ParamInfo(
    LuaElementPtr<LuaSyntaxElement> Ptr,
    bool IsVararg,
    bool Nullable = false
) : ISymbolInfo
{
    public LuaElementPtr<LuaParamDefSyntax> ParamDefPtr => Ptr.Cast<LuaParamDefSyntax>();

    public LuaElementPtr<LuaDocTypedParamSyntax> TypedParamPtr => Ptr.Cast<LuaDocTypedParamSyntax>();
}

public record MethodInfo(
    LuaElementPtr<LuaSyntaxElement> Ptr,
    LuaElementPtr<LuaFuncStatSyntax> FuncStatPtr
) : ISymbolInfo
{
    public LuaElementPtr<LuaNameExprSyntax> NamePtr => Ptr.Cast<LuaNameExprSyntax>();

    public LuaElementPtr<LuaIndexExprSyntax> IndexPtr => Ptr.Cast<LuaIndexExprSyntax>();

    public LuaElementPtr<LuaLocalNameSyntax> LocalPtr => Ptr.Cast<LuaLocalNameSyntax>();
}

public record NamedTypeInfo(
    LuaElementPtr<LuaDocTagNamedTypeSyntax> TypeDefinePtr,
    NamedTypeKind Kind)
    : ISymbolInfo
{
    public LuaElementPtr<LuaSyntaxElement> Ptr => TypeDefinePtr.UpCast();
}

public record DocFieldInfo(
    LuaElementPtr<LuaDocFieldSyntax> FieldDefPtr
) : ISymbolInfo
{
    public LuaElementPtr<LuaSyntaxElement> Ptr => FieldDefPtr.UpCast();
}

public record TableFieldInfo(
    LuaElementPtr<LuaTableFieldSyntax> TableFieldPtr
) : ISymbolInfo
{
    public LuaElementPtr<LuaSyntaxElement> Ptr => TableFieldPtr.UpCast();
}

public record EnumFieldInfo(
    LuaElementPtr<LuaDocTagEnumFieldSyntax> EnumFieldDefPtr
) : ISymbolInfo
{
    public LuaElementPtr<LuaSyntaxElement> Ptr => EnumFieldDefPtr.UpCast();
}

public record GenericParamInfo(
    LuaElementPtr<LuaDocGenericParamSyntax> GenericParamDefPtr
) : ISymbolInfo
{
    public LuaElementPtr<LuaSyntaxElement> Ptr => GenericParamDefPtr.UpCast();
}

public record IndexInfo(
    LuaElementPtr<LuaIndexExprSyntax> IndexExprPtr,
    LuaElementPtr<LuaExprSyntax> ValueExprPtr
) : ISymbolInfo
{
    public LuaElementPtr<LuaSyntaxElement> Ptr => IndexExprPtr.UpCast();
}

public record TypeIndexInfo(
    LuaType KeyType,
    LuaType ValueType,
    LuaElementPtr<LuaDocFieldSyntax> FieldDefPtr)
    : ISymbolInfo
{
    public LuaElementPtr<LuaSyntaxElement> Ptr => FieldDefPtr.UpCast();
}

public record TypeOpInfo(
    LuaElementPtr<LuaDocTagOperatorSyntax> OpFieldPtr
) : ISymbolInfo
{
    public LuaElementPtr<LuaSyntaxElement> Ptr => OpFieldPtr.UpCast();
}

// public record TupleMemberInfo(
//     int Index,
//     LuaElementPtr<LuaDocTypeSyntax> TypePtr)
//     : ISymbolInfo
// {
//     public LuaElementPtr<LuaSyntaxElement> Ptr => TypePtr.UpCast();
// }
//
// public record AggregateMemberInfo(
//     LuaElementPtr<LuaDocTypeSyntax> TypePtr
// ) : ISymbolInfo
// {
//     public LuaElementPtr<LuaSyntaxElement> Ptr => TypePtr.UpCast();
// }

public record VirtualInfo : ISymbolInfo
{
    private static VirtualInfo Default { get; } = new();

    public LuaElementPtr<LuaSyntaxElement> Ptr => LuaElementPtr<LuaSyntaxElement>.Empty;

    public static LuaSymbol CreateVirtualSymbol(string name, LuaType? type, SymbolFeature feature = SymbolFeature.None,
        SymbolVisibility visibility = SymbolVisibility.Public)
    {
        return new LuaSymbol(name, type, Default, feature, visibility);
    }

    public static LuaSymbol CreateTypeSymbol(LuaType? type)
    {
        return new LuaSymbol(string.Empty, type, Default);
    }
}

public record NamespaceInfo : VirtualInfo;
