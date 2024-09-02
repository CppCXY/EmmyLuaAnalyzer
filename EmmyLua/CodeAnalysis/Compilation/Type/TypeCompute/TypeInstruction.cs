using EmmyLua.CodeAnalysis.Compilation.Type.Types;

namespace EmmyLua.CodeAnalysis.Compilation.Type.TypeCompute;

public record struct TypeInstruction(TypeComputeOpCode OpCode, int Operand);
