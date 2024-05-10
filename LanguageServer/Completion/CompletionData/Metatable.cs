using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace LanguageServer.Completion.CompletionData;

public static class Metatable
{
    public static List<CompletionItem> MetaFields { get; } = new()
    {
        new CompletionItem
        {
            Label = "__index",
            Kind = CompletionItemKind.Property,
            Detail = "Metafield",
            Documentation = "The value of this field will be returned when the table is indexed with a key that is not in the table.",
            InsertText = "__index = ",
        },
        new CompletionItem
        {
            Label = "__newindex",
            Kind = CompletionItemKind.Property,
            Detail = "Metafield",
            Documentation = "The value of this field will be returned when the table is indexed with a key that is not in the table.",
            InsertText = "__newindex = ",
        },
        new CompletionItem
        {
            Label = "__len",
            Kind = CompletionItemKind.Property,
            Detail = "Metafield",
            Documentation = "The length of the table.",
            InsertText = "__len = ",
        },
        new CompletionItem
        {
            Label = "__pairs",
            Kind = CompletionItemKind.Property,
            Detail = "Metafield",
            Documentation = "The pairs function.",
            InsertText = "__pairs = ",
        },
        new CompletionItem
        {
            Label = "__ipairs",
            Kind = CompletionItemKind.Property,
            Detail = "Metafield",
            Documentation = "The ipairs function.",
            InsertText = "__ipairs = ",
        },
        new CompletionItem
        {
            Label = "__mode",
            Kind = CompletionItemKind.Property,
            Detail = "Metafield",
            Documentation = "The mode of the table.",
            InsertText = "__mode = ",
        },
        new CompletionItem
        {
            Label = "__call",
            Kind = CompletionItemKind.Property,
            Detail = "Metafield",
            Documentation = "The call function.",
            InsertText = "__call = ",
        },
        new CompletionItem
        {
            Label = "__tostring",
            Kind = CompletionItemKind.Property,
            Detail = "Metafield",
            Documentation = "The tostring function.",
            InsertText = "__tostring = ",
        },
        new CompletionItem
        {
            Label = "__gc",
            Kind = CompletionItemKind.Property,
            Detail = "Metafield",
            Documentation = "The garbage collection function.",
            InsertText = "__gc = ",
        },
        new CompletionItem
        {
            Label = "__add",
            Kind = CompletionItemKind.Property,
            Detail = "Metafield",
            Documentation = "The addition function.",
            InsertText = "__add = ",
        },
        new CompletionItem
        {
            Label = "__sub",
            Kind = CompletionItemKind.Property,
            Detail = "Metafield",
            Documentation = "The subtraction function.",
            InsertText = "__sub = ",
        },
        new CompletionItem
        {
            Label = "__mul",
            Kind = CompletionItemKind.Property,
            Detail = "Metafield",
            Documentation = "The multiplication function.",
            InsertText = "__mul = ",
        },
        new CompletionItem
        {
            Label = "__div",
            Kind = CompletionItemKind.Property,
            Detail = "Metafield",
            Documentation = "The division function.",
            InsertText = "__div = ",
        },
        new CompletionItem
        {
            Label = "__mod",
            Kind = CompletionItemKind.Property,
            Detail = "Metafield",
            Documentation = "The modulo function.",
            InsertText = "__mod = ",
        },
        new CompletionItem
        {
            Label = "__pow",
            Kind = CompletionItemKind.Property,
            Detail = "Metafield",
            Documentation = "The power function.",
            InsertText = "__pow = ",
        },
        new CompletionItem
        {
            Label = "__unm",
            Kind = CompletionItemKind.Property,
            Detail = "Metafield",
            Documentation = "The unary minus function.",
            InsertText = "__unm = ",
        },
        new CompletionItem
        {
            Label = "__idiv",
            Kind = CompletionItemKind.Property,
            Detail = "Metafield",
            Documentation = "The floor division function.",
            InsertText = "__idiv = ",
        },
        new CompletionItem
        {
            Label = "__band",
            Kind = CompletionItemKind.Property,
            Detail = "Metafield",
            Documentation = "The bitwise AND function.",
            InsertText = "__band = ",
        },
        new CompletionItem
        {
            Label = "__bor",
            Kind = CompletionItemKind.Property,
            Detail = "Metafield",
            Documentation = "The bitwise OR function.",
            InsertText = "__bor = ",
        },
        new CompletionItem
        {
            Label = "__bxor",
            Kind = CompletionItemKind.Property,
            Detail = "Metafield",
            Documentation = "The bitwise XOR function.",
            InsertText = "__bxor = ",
        },
        new CompletionItem
        {
            Label = "__bnot",
            Kind = CompletionItemKind.Property,
            Detail = "Metafield",
            Documentation = "The bitwise NOT function.",
            InsertText = "__bnot = ",
        },
        new CompletionItem
        {
            Label = "__shl",
            Kind = CompletionItemKind.Property,
            Detail = "Metafield",
            Documentation = "The bitwise shift left function.",
            InsertText = "__shl = ",
        },
        new CompletionItem
        {
            Label = "__shr",
            Kind = CompletionItemKind.Property,
            Detail = "Metafield",
            Documentation = "The bitwise shift right function.",
            InsertText = "__shr = ",
        },
        new CompletionItem
        {
            Label = "__concat",
            Kind = CompletionItemKind.Property,
            Detail = "Metafield",
            Documentation = "The concatenation function.",
            InsertText = "__concat = ",
        },
        new CompletionItem
        {
            Label = "__eq",
            Kind = CompletionItemKind.Property,
            Detail = "Metafield",
            Documentation = "The equality function.",
            InsertText = "__eq = ",
        },
        new CompletionItem
        {
            Label = "__lt",
            Kind = CompletionItemKind.Property,
            Detail = "Metafield",
            Documentation = "The less than function.",
            InsertText = "__lt = ",
        },
        new CompletionItem
        {
            Label = "__le",
            Kind = CompletionItemKind.Property,
            Detail = "Metafield",
            Documentation = "The less than or equal function.",
            InsertText = "__le = ",
        },
    };
}