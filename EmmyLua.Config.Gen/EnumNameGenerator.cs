using System.Text.RegularExpressions;
using NJsonSchema;
using NJsonSchema.CodeGeneration;

namespace EmmyLua.Config.Gen;

public class EnumNameGenerator : IEnumNameGenerator
{
    private static readonly Regex InvalidNameCharactersPattern =
        new Regex(@"[^\p{Lu}\p{Ll}\p{Lt}\p{Lm}\p{Lo}\p{Nl}\p{Mn}\p{Mc}\p{Nd}\p{Pc}\p{Cf}]");

    private const string DefaultReplacementCharacter = "_";

    public string Generate(int index, string? name, object? value, JsonSchema schema)
    {
        if (string.IsNullOrEmpty(name))
        {
            return "Empty";
        }

        name = name switch
        {
            "//" => "comment",
            "/**/" => "long_comment",
            "`" => "backtick",
            "+=" => "plus_equals",
            "-=" => "minus_equals",
            "*=" => "multiply_equals",
            "/=" => "divide_equals",
            "%=" => "modulus_equals",
            "^=" => "bitwise_xor_equals",
            "//=" => "floor_divide_equals",
            "|=" => "bitwise_or_equals",
            "&=" => "bitwise_and_equals",
            "<<=" => "left_shift_equals",
            ">>=" => "right_shift_equals",
            "||" => "logical_or",
            "&&" => "logical_and",
            "!" => "logical_not",
            "!=" => "not_equals",
            "continue" => "continue_keyword",
            _ => name
        };
        if (name!.StartsWith("-"))
        {
            name = "Minus" + name.Substring(1);
        }

        if (name.StartsWith("+"))
        {
            name = "Plus" + name.Substring(1);
        }

        if (name.StartsWith("_-"))
        {
            name = "__" + name.Substring(2);
        }

        return InvalidNameCharactersPattern.Replace(ConversionUtilities.ConvertToUpperCamelCase(name
            .Replace(":", "-").Replace(@"""", @""), true), "_");
    }
}