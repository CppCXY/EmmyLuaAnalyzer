namespace EmmyLua.CodeAnalysis.Compilation.Type.TypeCompute;

public enum TypeComputeOpCode
{
    // load a type from the args
    Load,

    // ref a type
    Ref,

    Union,

    Intersection,

    Call,

    KeyOf,

    TypeOf,

    Index,

    Array,
}
