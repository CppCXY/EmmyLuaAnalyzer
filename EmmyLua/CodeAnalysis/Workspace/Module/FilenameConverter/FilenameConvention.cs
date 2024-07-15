using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace EmmyLua.CodeAnalysis.Workspace.Module.FilenameConverter;

public enum FilenameConvention
{
    [EnumMember(Value = "none")] None,
    [EnumMember(Value = "camelCase")] CamelCase,
    [EnumMember(Value = "pascalCase")] PascalCase,
    [EnumMember(Value = "snakeCase")] SnakeCase
}
