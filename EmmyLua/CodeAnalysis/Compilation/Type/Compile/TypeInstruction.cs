namespace EmmyLua.CodeAnalysis.Compilation.Type.Compile;

public record struct TypeInstruction(TypeComputeOpCode OpCode, int Operand);
