namespace LuaLanguageServer.CodeAnalysis.Compilation.Symbol.Impl;

public class Builtin
{
    public readonly UnknownSymbol Unknown = new UnknownSymbol();
    public readonly VoidSymbol Void = new VoidSymbol();
    public readonly NilSymbol Nil = new NilSymbol();
    public readonly PrimitiveSymbol Number = new PrimitiveSymbol("number", PrimitiveTypeKind.Number);
    public readonly PrimitiveSymbol Integer = new PrimitiveSymbol("integer", PrimitiveTypeKind.Integer);
    public readonly PrimitiveSymbol String = new PrimitiveSymbol("string", PrimitiveTypeKind.String);
    public readonly PrimitiveSymbol Boolean = new PrimitiveSymbol("boolean", PrimitiveTypeKind.Boolean);
}

