namespace EmmyLua.CodeAnalysis.Compilation.Symbol;

[Flags]
public enum SymbolFlag : ushort
{
    Local = 0x0001,
    Method = 0x0002,
    ClassMember = 0x0004,
    Global = 0x0008,
    TypeDeclaration = 0x0010,
    Parameter = 0x0020,
    DocField = 0x0040,
    EnumMember = 0x0080,
    Virtual = 0x0100,
    GenericParameter = 0x0200,
}
