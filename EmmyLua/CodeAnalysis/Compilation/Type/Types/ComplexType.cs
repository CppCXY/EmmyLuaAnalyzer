namespace EmmyLua.CodeAnalysis.Compilation.Type.Types;

public abstract class LuaComplexType : LuaType
{
    public override IEnumerable<LuaType> DescendantTypes
    {
        get
        {
            var stack = new Stack<LuaType>();
            foreach (var child in ChildrenTypes.Reverse())
            {
                stack.Push(child);
            }

            while (stack.Count > 0)
            {
                var luaType = stack.Pop();
                yield return luaType;
                foreach (var child in luaType.ChildrenTypes.Reverse())
                {
                    stack.Push(child);
                }
            }
        }
    }
}

public class LuaUnionType(List<LuaType> typeList)
    : LuaComplexType
{
    public List<LuaType> TypeList { get; } = typeList.ToList();

    public override IEnumerable<LuaType> ChildrenTypes => TypeList;

    public override string ToString() => $"{string.Join(" | ", TypeList)}";
}

public class LuaTupleType(List<LuaType> typeList)
    : LuaComplexType
{
    public List<LuaType> TypeList { get; } = typeList;

    public override IEnumerable<LuaType> ChildrenTypes => TypeList;

    public override string ToString() => $"[{string.Join(", ", TypeList)}]";
}

public class LuaArrayType(LuaType baseType)
    : LuaComplexType
{
    public LuaType BaseType { get; } = baseType;

    public override IEnumerable<LuaType> ChildrenTypes
    {
        get { yield return BaseType; }
    }

    public override string ToString() => $"{BaseType}[]";
}

// just compact luals
public class LuaVariadicType(LuaType baseType)
    : LuaComplexType
{
    public LuaType BaseType { get; } = baseType;

    public override IEnumerable<LuaType> ChildrenTypes
    {
        get { yield return BaseType; }
    }

    public override string ToString() => $"...{BaseType}";
}

public class LuaMultiReturnType : LuaComplexType
{
    private List<LuaType>? RetTypes { get; }

    private LuaType? BaseType { get; }

    public LuaMultiReturnType(LuaType baseType)
    {
        BaseType = baseType;
    }

    public LuaMultiReturnType(List<LuaType> retTypes)
    {
        RetTypes = retTypes;
    }

    public LuaType GetElementType(int id)
    {
        if (RetTypes?.Count > id)
        {
            return RetTypes[id];
        }

        return BaseType ?? Builtin.Nil;
    }

    public int GetElementCount()
    {
        return RetTypes?.Count ?? 0;
    }

    public override IEnumerable<LuaType> ChildrenTypes
    {
        get
        {
            if (RetTypes is not null)
            {
                return RetTypes;
            }

            return BaseType is not null ? new[] { BaseType } : Enumerable.Empty<LuaType>();
        }
    }

    public override string ToString()
    {
        if (RetTypes is not null)
        {
            return $"[{string.Join(", ", RetTypes)}]";
        }

        return $"{BaseType}...";
    }
}

// lua template type like T
public class LuaTplType(string name, LuaType? baseType) : LuaComplexType
{
    public LuaType? BaseType { get; } = baseType;

    public string Name { get; } = name;

    public override IEnumerable<LuaType> ChildrenTypes
    {
        get
        {
            if (BaseType is not null)
            {
                yield return BaseType;
            }
        }
    }

    public override string ToString()
    {
        if (BaseType is not null)
        {
            return $"{Name} : {BaseType}";
        }

        return $"{Name}";
    }
}

public class LuaGenericType(
    LuaType baseType,
    List<LuaType> genericArgs)
    : LuaComplexType
{
    public LuaType BaseType { get; } = baseType;

    public List<LuaType> GenericArgs { get; } = genericArgs;

    public override IEnumerable<LuaType> ChildrenTypes
    {
        get
        {
            yield return BaseType;
            foreach (var arg in GenericArgs)
            {
                yield return arg;
            }
        }
    }

    public override string ToString() => $"{BaseType}<{string.Join(", ", GenericArgs)}>";
}

public class LuaKeyOfType(LuaType baseType)
    : LuaComplexType
{
    public LuaType BaseType { get; } = baseType;

    public override IEnumerable<LuaType> ChildrenTypes
    {
        get { yield return BaseType; }
    }

    public override string ToString() => $"keyof {BaseType}";
}

// a & b
public class LuaIntersectionType(List<LuaType> typeList)
    : LuaComplexType
{
    public List<LuaType> TypeList { get; } = typeList;

    public override IEnumerable<LuaType> ChildrenTypes => TypeList;

    public override string ToString() => $"{string.Join(" & ", TypeList)}";
}

// a extends b
public class LuaExtendType(LuaType baseType, LuaType extendType)
    : LuaComplexType
{
    public LuaType BaseType { get; } = baseType;

    public LuaType ExtendType { get; } = extendType;

    public override IEnumerable<LuaType> ChildrenTypes
    {
        get
        {
            yield return BaseType;
            yield return ExtendType;
        }
    }

    public override string ToString() => $"{BaseType} extends {ExtendType}";
}

// true ? a : b
public class LuaTernaryType(LuaType conditionType, LuaType trueType, LuaType falseType)
    : LuaComplexType
{
    public LuaType ConditionType { get; } = conditionType;

    public LuaType TrueType { get; } = trueType;

    public LuaType FalseType { get; } = falseType;

    public override IEnumerable<LuaType> ChildrenTypes
    {
        get
        {
            yield return ConditionType;
            yield return TrueType;
            yield return FalseType;
        }
    }

    public override string ToString() => $"{ConditionType} ? {TrueType} : {FalseType}";
}

// P in K
public class LuaInType(string name, LuaType baseType)
    : LuaComplexType
{
    public LuaType BaseType { get; } = baseType;

    public string Name { get; } = name;

    public override IEnumerable<LuaType> ChildrenTypes
    {
        get { yield return BaseType; }
    }

    public override string ToString() => $"{Name} in {BaseType}";
}

// { [P in keyof T]: T[P] }
public class LuaMapppedType(string name, LuaType baseType, LuaType valueType) : LuaComplexType
{
    public LuaType BaseType { get; } = baseType;

    public LuaType ValueType { get; } = valueType;

    public string Name { get; } = name;

    public override IEnumerable<LuaType> ChildrenTypes
    {
        get
        {
            yield return BaseType;
            yield return ValueType;
        }
    }

    public override string ToString() => $"{{ [ {Name} in {BaseType} ]: {ValueType} }}";
}

// T[P]
public class LuaIndexedAccessType(LuaType baseType, LuaType indexType)
    : LuaComplexType
{
    public LuaType BaseType { get; } = baseType;

    public LuaType IndexType { get; } = indexType;

    public override IEnumerable<LuaType> ChildrenTypes
    {
        get
        {
            yield return BaseType;
            yield return IndexType;
        }
    }

    public override string ToString() => $"{BaseType}[{IndexType}]";
}

// { a: T, b: U }
public class LuaRecordType(Dictionary<string, LuaType> fields)
    : LuaComplexType
{
    public Dictionary<string, LuaType> Fields { get; } = fields;

    public override IEnumerable<LuaType> ChildrenTypes => Fields.Values;

    public override string ToString() => $"{{ {string.Join(", ", Fields.Select(i => $"{i.Key}: {i.Value}"))} }}";
}

// fun(a: T, b: U): R
public class LuaDocFunctionType(List<(string, LuaType?)> argTypes, LuaType retType)
    : LuaComplexType
{
    public List<(string, LuaType?)> ArgTypes { get; } = argTypes;

    public LuaType RetType { get; } = retType;

    public override IEnumerable<LuaType> ChildrenTypes
    {
        get
        {
            foreach (var (_, arg) in ArgTypes)
            {
                if (arg is not null)
                {
                    yield return arg;
                }
            }

            yield return RetType;
        }
    }

    public override string ToString() => $"fun({string.Join(", ", ArgTypes.Select(i => $"{i.Item1}: {i.Item2}"))}): {RetType}";
}
