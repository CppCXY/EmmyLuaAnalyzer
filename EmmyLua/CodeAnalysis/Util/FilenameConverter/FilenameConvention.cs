using System.Runtime.Serialization;

namespace EmmyLua.CodeAnalysis.Util.FilenameConverter;

public enum FilenameConvention
{
    [EnumMember(Value = "none")] None,
    [EnumMember(Value = "camelCase")] CamelCase,
    [EnumMember(Value = "pascalCase")] PascalCase,
    [EnumMember(Value = "snakeCase")] SnakeCase
}
