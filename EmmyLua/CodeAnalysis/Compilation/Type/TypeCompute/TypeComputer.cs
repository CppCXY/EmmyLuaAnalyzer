using EmmyLua.CodeAnalysis.Compilation.Type.Types;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Type.TypeCompute;

public class TypeComputer
{
    private List<string> Params { get; }

    private Dictionary<string, int> ParamIndex { get; }

    private List<TypeInstruction> Instructions { get; }

    private List<LuaNamedType> Refs { get; }

    private List<string> IdRefs { get; }

    TypeComputer(List<string> @params)
    {
        Params = @params;
        ParamIndex = new();
        for (var i = 0; i < @params.Count; i++)
        {
            ParamIndex[@params[i]] = i;
        }

        Instructions = new();
        Refs = new List<LuaNamedType>();
        IdRefs = new List<string>();
    }

    public static TypeComputer Compile(List<string> templateParams, LuaDocTypeSyntax typeSyntax)
    {
        var typeComposer = new TypeComputer(templateParams);
        typeComposer.Compile(typeSyntax);
        return typeComposer;
    }

    private void Compile(LuaDocTypeSyntax typeSyntax)
    {
        switch (typeSyntax)
        {
            case LuaDocGenericTypeSyntax genericTypeSyntax:
            {
                CompileGenericType(genericTypeSyntax);
                break;
            }
            case LuaDocNameTypeSyntax nameType:
            {
                CompileNameType(nameType);
                break;
            }
            case LuaDocUnionTypeSyntax unionTypeSyntax:
            {
                CompileUnionType(unionTypeSyntax);
                break;
            }
            case LuaDocArrayTypeSyntax arrayTypeSyntax:
            {
                CompileArrayType(arrayTypeSyntax);
                break;
            }
            case LuaDocParenTypeSyntax parenTypeSyntax:
            {
                if (parenTypeSyntax is { Type: { } type })
                {
                    Compile(type);
                }
                break;
            }
            case LuaDocTableTypeSyntax tableTypeSyntax:
            {
                CompileTableType(tableTypeSyntax);
                break;
            }
            case LuaDocKeyOfTypeSyntax keyOfTypeSyntax:
            {
                CompileKeyOfType(keyOfTypeSyntax);
                break;
            }
            case LuaDocTypeOfTypeSyntax typeOfTypeSyntax:
            {
                CompileTypeOfType(typeOfTypeSyntax);
                break;
            }
            case LuaDocMappedTypeSyntax mappedTypeSyntax:
            {
                CompileMappedType(mappedTypeSyntax);
                break;
            }
        }
    }

    private void CompileGenericType(LuaDocGenericTypeSyntax genericTypeSyntax)
    {
        if (genericTypeSyntax is { Name.RepresentText: { } name, GenericArgs: { } genericArgs })
        {
            if (ParamIndex.TryGetValue(name, out var index))
            {
                Instructions.Add(new TypeInstruction(TypeComputeOpCode.Load, index));
            }
            else
            {
                index = Refs.Count;
                Refs.Add(new LuaNamedType(genericTypeSyntax.DocumentId, name));
                Instructions.Add(new TypeInstruction(TypeComputeOpCode.Ref, index));
            }

            var genericArgsList = genericArgs.ToList();
            foreach (var typeSyntax in genericArgsList)
            {
                Compile(typeSyntax);
            }

            Instructions.Add(new TypeInstruction(TypeComputeOpCode.Call, genericArgsList.Count));
        }
    }

    private void CompileNameType(LuaDocNameTypeSyntax nameType)
    {
        if (nameType is { Name.RepresentText: { } name })
        {
            if (ParamIndex.TryGetValue(name, out var index))
            {
                Instructions.Add(new TypeInstruction(TypeComputeOpCode.Load, index));
            }
            else
            {
                index = Refs.Count;
                Refs.Add(new LuaNamedType(nameType.DocumentId, name));
                Instructions.Add(new TypeInstruction(TypeComputeOpCode.Ref, index));
            }
        }
    }

    private void CompileUnionType(LuaDocUnionTypeSyntax unionTypeSyntax)
    {
        var types = unionTypeSyntax.UnionTypes.ToList();
        foreach (var typeSyntax in types)
        {
            Compile(typeSyntax);
        }

        Instructions.Add(new TypeInstruction(TypeComputeOpCode.Union, types.Count));
    }

    private void CompileArrayType(LuaDocArrayTypeSyntax arrayTypeSyntax)
    {
        if (arrayTypeSyntax is { BaseType: { } baseType })
        {
            Compile(baseType);
            Instructions.Add(new TypeInstruction(TypeComputeOpCode.Array, 0));
        }
    }

    private void CompileTableType(LuaDocTableTypeSyntax tableTypeSyntax)
    {
        if (tableTypeSyntax is { Body.FieldList: { } fields })
        {
            foreach (var field in fields)
            {
                // if (field is { Key: { } key, Value: { } value })
                // {
                //     Compile(key);
                //     Compile(value);
                // }
            }
        }
    }

    private void CompileKeyOfType(LuaDocKeyOfTypeSyntax keyOfTypeSyntax)
    {
        if (keyOfTypeSyntax is { Type: {} type })
        {
            Compile(type);
            Instructions.Add(new TypeInstruction(TypeComputeOpCode.KeyOf, 0));
        }
    }

    private void CompileTypeOfType(LuaDocTypeOfTypeSyntax typeOfTypeSyntax)
    {
        if (typeOfTypeSyntax is { Type: LuaDocNameTypeSyntax {Name.RepresentText: {} name} })
        {
            var pos = IdRefs.Count;
            IdRefs.Add(name);
            Instructions.Add(new TypeInstruction(TypeComputeOpCode.TypeOf, pos));
        }
    }

    private void CompileMappedType(LuaDocMappedTypeSyntax mappedTypeSyntax)
    {
        if (mappedTypeSyntax is { KeyType: LuaDocNameTypeSyntax key, ValueType: { } value })
        {
            // Compile(key);
            // Compile(value);
        }
    }

    public static LuaType Compute(List<LuaType> args, LuaTypeManager typeManager)
    {
        var stack = new Stack<LuaType>();

        return Builtin.Unknown;
    }

}
