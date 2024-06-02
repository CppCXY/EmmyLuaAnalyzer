using System.Globalization;

namespace EmmyLua.CodeAnalysis.Workspace.Module.FilenameConverter;

// ReSharper disable once IdentifierTypo
public static class FilenameConverter
{
    public static string ConvertToIdentifier(string source, FilenameConvention convention)
    {
        return convention switch
        {
            FilenameConvention.CamelCase => ToCamelCase(source),
            FilenameConvention.PascalCase => ToPascalCase(source),
            FilenameConvention.SnakeCase => ToSnakeCase(source),
            _ => source
        };
    }

    private static string ToCamelCase(string input)
    {
        var words = input.Trim().Split(new[] { ' ', '_', '-' }, StringSplitOptions.RemoveEmptyEntries);
        return string.Join("",
            words.Select((word, index) =>
                index == 0 ? word.ToLower() : CultureInfo.CurrentCulture.TextInfo.ToTitleCase(word.ToLower())));
    }

    private static string ToPascalCase(string input)
    {
        var words = input.Trim().Split(new[] { ' ', '_', '-' }, StringSplitOptions.RemoveEmptyEntries);
        return string.Join("", words.Select(word => CultureInfo.CurrentCulture.TextInfo.ToTitleCase(word.ToLower())));
    }

    private static string ToSnakeCase(string source)
    {
        var words = source.Trim().Split(new[] { ' ', '_', '-' }, StringSplitOptions.RemoveEmptyEntries);
        return string.Join("_", words.Select(word => word.ToLower()));
    }
}
