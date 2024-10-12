using EmmyLua.CodeAnalysis.Compilation.Search;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Compilation.Type.Types;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Document.Version;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Symbol;

public interface ISymbolInfo
{
    public LuaPtr<LuaSyntaxElement> Ptr { get; }
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
    Readonly = 0x40,
}

public enum SymbolVisibility
{
    Public,
    Protected,
    Private,
    Package,
    Internal
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

    public LuaSymbol Instantiate(TypeSubstitution substitution)
    {
        throw new NotImplementedException();
    }

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
            var source = context.Compilation.ProjectIndex.QuerySource(UniqueId);
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
    LuaPtr<LuaLocalNameSyntax> LocalNamePtr,
    bool IsConst = false,
    bool IsClose = false
) : ISymbolInfo
{
    public LuaPtr<LuaSyntaxElement> Ptr => LocalNamePtr.UpCast();
}

public record GlobalInfo(
    LuaPtr<LuaNameExprSyntax> VarNamePtr
) : ISymbolInfo
{
    public LuaPtr<LuaSyntaxElement> Ptr => VarNamePtr.UpCast();
}

public record ParamInfo(
    LuaPtr<LuaSyntaxElement> Ptr,
    bool IsVararg,
    bool Nullable = false
) : ISymbolInfo
{
    public LuaPtr<LuaParamDefSyntax> ParamDefPtr => Ptr.Cast<LuaParamDefSyntax>();

    public LuaPtr<LuaDocTypedParamSyntax> TypedParamPtr => Ptr.Cast<LuaDocTypedParamSyntax>();
}

public record MethodInfo(
    LuaPtr<LuaSyntaxElement> Ptr,
    LuaPtr<LuaFuncStatSyntax> FuncStatPtr
) : ISymbolInfo
{
    public LuaPtr<LuaNameExprSyntax> NamePtr => Ptr.Cast<LuaNameExprSyntax>();

    public LuaPtr<LuaIndexExprSyntax> IndexPtr => Ptr.Cast<LuaIndexExprSyntax>();

    public LuaPtr<LuaLocalNameSyntax> LocalPtr => Ptr.Cast<LuaLocalNameSyntax>();
}

public record NamedTypeInfo(
    LuaPtr<LuaDocTagNamedTypeSyntax> TypeDefinePtr,
    NamedTypeKind Kind)
    : ISymbolInfo
{
    public LuaPtr<LuaSyntaxElement> Ptr => TypeDefinePtr.UpCast();
}

public record DocFieldInfo(
    LuaPtr<LuaDocFieldSyntax> FieldDefPtr
) : ISymbolInfo
{
    public LuaPtr<LuaSyntaxElement> Ptr => FieldDefPtr.UpCast();
}

public record TableFieldInfo(
    LuaPtr<LuaTableFieldSyntax> TableFieldPtr
) : ISymbolInfo
{
    public LuaPtr<LuaSyntaxElement> Ptr => TableFieldPtr.UpCast();
}

public record EnumFieldInfo(
    LuaPtr<LuaDocTagEnumFieldSyntax> EnumFieldDefPtr
) : ISymbolInfo
{
    public LuaPtr<LuaSyntaxElement> Ptr => EnumFieldDefPtr.UpCast();
}

public record GenericParamInfo(
    LuaPtr<LuaDocGenericParamSyntax> GenericParamDefPtr
) : ISymbolInfo
{
    public LuaPtr<LuaSyntaxElement> Ptr => GenericParamDefPtr.UpCast();
}

public record IndexInfo(
    LuaPtr<LuaIndexExprSyntax> IndexExprPtr,
    LuaPtr<LuaExprSyntax> ValueExprPtr
) : ISymbolInfo
{
    public LuaPtr<LuaSyntaxElement> Ptr => IndexExprPtr.UpCast();
}

public record TypeIndexInfo(
    LuaType KeyType,
    LuaType ValueType,
    LuaPtr<LuaDocFieldSyntax> FieldDefPtr)
    : ISymbolInfo
{
    public LuaPtr<LuaSyntaxElement> Ptr => FieldDefPtr.UpCast();
}

public record TypeOpInfo(
    LuaPtr<LuaDocTagOperatorSyntax> OpFieldPtr
) : ISymbolInfo
{
    public LuaPtr<LuaSyntaxElement> Ptr => OpFieldPtr.UpCast();
}

// public record TupleMemberInfo(
//     int Index,
//     LuaPtr<LuaDocTypeSyntax> TypePtr)
//     : ISymbolInfo
// {
//     public LuaPtr<LuaSyntaxElement> Ptr => TypePtr.UpCast();
// }
//
// public record AggregateMemberInfo(
//     LuaPtr<LuaDocTypeSyntax> TypePtr
// ) : ISymbolInfo
// {
//     public LuaPtr<LuaSyntaxElement> Ptr => TypePtr.UpCast();
// }

public record VirtualInfo : ISymbolInfo
{
    private static VirtualInfo Default { get; } = new();

    public LuaPtr<LuaSyntaxElement> Ptr => LuaPtr<LuaSyntaxElement>.Empty;

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
