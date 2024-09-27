using EmmyLua.CodeAnalysis.Compile.Kind;
using EmmyLua.CodeAnalysis.Compile.Parser;

namespace EmmyLua.CodeAnalysis.Compile.Grammar.Doc;

public static class TypesParser
{
    [Flags]
    public enum TypeParseFeature
    {
        None = 0,
        CompactLuaLs = 0X01,
        AllowContinue = 0X02,
        DisableNullable = 0X04,
    }

    public static void TypeList(LuaDocParser p)
    {
        var cm = Type(p);
        while (cm.IsComplete && p.Current is LuaTokenKind.TkComma)
        {
            p.Bump();
            cm = Type(p, TypeParseFeature.None);
        }
    }

    public static CompleteMarker Type(LuaDocParser p, TypeParseFeature feature = TypeParseFeature.AllowContinue)
    {
        if (feature.HasFlag(TypeParseFeature.AllowContinue) && p.Current is LuaTokenKind.TkDocOr)
        {
            p.Bump();
        }

        return SubType(p, 0, feature);
    }

    private static CompleteMarker SubType(LuaDocParser p, int limit, TypeParseFeature feature)
    {
        CompleteMarker cm;
        var unaryOp = CompileTypeOperatorKind.ToUnaryTypeOperatorKind(p.Current);
        if (unaryOp != CompileTypeOperatorKind.TypeUnaryOperator.None)
        {
            var m = p.Marker();
            p.Bump();
            SimpleType(p, feature);
            var kind = unaryOp switch
            {
                CompileTypeOperatorKind.TypeUnaryOperator.KeyOf => LuaSyntaxKind.TypeKeyOf,
                _ => LuaSyntaxKind.None
            };

            cm = m.Complete(p, kind);
        }
        else
        {
            cm = SimpleType(p, feature);
        }

        var binaryOp = CompileTypeOperatorKind.ToBinaryTypeOperatorKind(p.Current);
        if (binaryOp != CompileTypeOperatorKind.TypeBinaryOperator.None)
        {
            while (binaryOp != CompileTypeOperatorKind.TypeBinaryOperator.None &&
                   CompileTypeOperatorKind.Priority[(int)binaryOp].Left > limit)
            {
                var m = cm.Precede(p);
                p.Bump();

                if (feature.HasFlag(TypeParseFeature.CompactLuaLs) &&
                    binaryOp == CompileTypeOperatorKind.TypeBinaryOperator.Union &&
                    p.Current is LuaTokenKind.TkGt or LuaTokenKind.TkPlus)
                {
                    p.Bump();
                }

                var cm2 = SubType(p, CompileTypeOperatorKind.Priority[(int)binaryOp].Right, feature);
                DescriptionParser.InlineDescription(p);
                var kind = binaryOp switch
                {
                    CompileTypeOperatorKind.TypeBinaryOperator.Union => LuaSyntaxKind.TypeUnion,
                    CompileTypeOperatorKind.TypeBinaryOperator.Intersection => LuaSyntaxKind.TypeIntersection,
                    CompileTypeOperatorKind.TypeBinaryOperator.In => LuaSyntaxKind.TypeIn,
                    _ => LuaSyntaxKind.None
                };
                cm = m.Complete(p, kind);
                if (!cm2.IsComplete)
                {
                    return cm;
                }

                binaryOp = CompileTypeOperatorKind.ToBinaryTypeOperatorKind(p.Current);
            }
        }

        var threeOp = CompileTypeOperatorKind.ToThreeTypeOperatorKind(p.Current);
        if (threeOp != CompileTypeOperatorKind.TypeThreeOperator.None)
        {
            var m = cm.Precede(p);
            try
            {
                p.Bump();
                var cm2 = SubType(p, 0, feature | TypeParseFeature.DisableNullable);
                if (!cm2.IsComplete)
                {
                    return m.Fail(p, LuaSyntaxKind.TypeConditional, "expect type");
                }

                p.Expect(LuaTokenKind.TkNullable);
                var cm3 = SubType(p, 0, feature);
                if (!cm3.IsComplete)
                {
                    return m.Fail(p, LuaSyntaxKind.TypeConditional, "expect type");
                }

                p.Expect(LuaTokenKind.TkColon);
                var cm4 = SubType(p, 0, feature);
                if (!cm4.IsComplete)
                {
                    return m.Fail(p, LuaSyntaxKind.TypeConditional, "expect type");
                }

                return m.Complete(p, LuaSyntaxKind.TypeConditional);
            }
            catch (UnexpectedTokenException e)
            {
                return m.Fail(p, LuaSyntaxKind.TypeConditional, e.Message);
            }
        }

        return cm;
    }

    private static CompleteMarker SimpleType(LuaDocParser p, TypeParseFeature feature)
    {
        var cm = PrimaryType(p);
        if (!cm.IsComplete)
        {
            return cm;
        }

        // suffix
        SuffixType(p, feature, ref cm);
        return cm;
    }

    public static void ReturnTypeList(LuaDocParser p)
    {
        if (p.Current is LuaTokenKind.TkDocMatch)
        {
            var cm = ReturnMatchType(p);
            while (cm.IsComplete && p.Current is LuaTokenKind.TkDocMatch)
            {
                p.Bump();
                cm = ReturnMatchType(p);
            }

            return;
        }

        TypeList(p);
    }

    private static CompleteMarker ReturnMatchType(LuaDocParser p)
    {
        var m = p.Marker();
        try
        {
            p.Bump();
            p.Expect(LuaTokenKind.TkLeftParen);
            TypeList(p);
            p.Expect(LuaTokenKind.TkRightParen);
            return m.Complete(p, LuaSyntaxKind.TypeMatch);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.TypeMatch, e.Message);
        }
    }

    private static void SuffixType(LuaDocParser p, TypeParseFeature feature, ref CompleteMarker pcm)
    {
        var continueArray = false;
        while (true)
        {
            switch (p.Current)
            {
                // array or index access
                case LuaTokenKind.TkLeftBracket:
                {
                    var kind = LuaSyntaxKind.TypeArray;
                    var m = pcm.Precede(p);
                    p.Bump();
                    if (p.Current is LuaTokenKind.TkString or LuaTokenKind.TkInt or LuaTokenKind.TkName)
                    {
                        p.Bump();
                        kind = LuaSyntaxKind.TypeIndexAccess;
                    }

                    p.Expect(LuaTokenKind.TkRightBracket);
                    pcm = m.Complete(p, kind);
                    continueArray = true;
                    break;
                }
                // generic
                case LuaTokenKind.TkLt:
                {
                    if (continueArray)
                    {
                        return;
                    }

                    if (pcm.Kind != LuaSyntaxKind.TypeName)
                    {
                        return;
                    }

                    var m = pcm.Reset(p);
                    p.Bump();
                    TypeList(p);
                    p.Expect(LuaTokenKind.TkGt);
                    pcm = m.Complete(p, LuaSyntaxKind.TypeGeneric);
                    return;
                }
                // '?'
                case LuaTokenKind.TkNullable:
                {
                    if (feature.HasFlag(TypeParseFeature.DisableNullable))
                    {
                        return;
                    }

                    p.Bump();
                    break;
                }
                case LuaTokenKind.TkDots:
                {
                    if (pcm.Kind != LuaSyntaxKind.TypeName)
                    {
                        return;
                    }

                    var m = pcm.Reset(p);
                    p.Bump();
                    pcm = m.Complete(p, LuaSyntaxKind.TypeExpand);
                    return;
                }
                case LuaTokenKind.TkStringTemplateType:
                {
                    if (pcm.Kind != LuaSyntaxKind.TypeName)
                    {
                        return;
                    }

                    var m = pcm.Reset(p);
                    p.Bump();
                    pcm = m.Complete(p, LuaSyntaxKind.TypeTemplate);
                    return;
                }
                default:
                {
                    return;
                }
            }
        }
    }

    private static CompleteMarker PrimaryType(LuaDocParser p)
    {
        return p.Current switch
        {
            LuaTokenKind.TkLeftBrace => ObjectOrMappedType(p),
            LuaTokenKind.TkLeftParen => ParenType(p),
            LuaTokenKind.TkLeftBracket => TupleType(p),
            LuaTokenKind.TkString or LuaTokenKind.TkInt or LuaTokenKind.TkDocBoolean => LiteralType(p),
            LuaTokenKind.TkName => FuncOrNameType(p),
            LuaTokenKind.TkStringTemplateType => TemplateType(p),
            LuaTokenKind.TkDots => VariadicType(p),
            _ => CompleteMarker.Empty
        };
    }

    public static CompleteMarker ObjectOrMappedType(LuaDocParser p)
    {
        var m = p.Marker();

        try
        {
            if (p.Current is LuaTokenKind.TkLeftBracket)
            {
                var rollbackPoint = p.GetRollbackPoint();
                var cm = MappedParser.TryMappedType(p);
                if (cm.IsComplete)
                {
                    return cm;
                }
                p.Rollback(rollbackPoint);
            }

            FieldsParser.DefineBody(p);
            return m.Complete(p, LuaSyntaxKind.TypeObject);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.TypeObject, e.Message);
        }
    }

    private static CompleteMarker ParenType(LuaDocParser p)
    {
        var m = p.Marker();
        p.Bump();

        try
        {
            Type(p);

            p.Expect(LuaTokenKind.TkRightParen);

            return m.Complete(p, LuaSyntaxKind.TypeParen);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.TypeParen, e.Message);
        }
    }

    private static CompleteMarker TupleType(LuaDocParser p)
    {
        var m = p.Marker();
        p.Bump();

        try
        {
            if (p.Current is LuaTokenKind.TkRightBracket)
            {
                p.Bump();
                return m.Complete(p, LuaSyntaxKind.TypeTuple);
            }

            TypeList(p);

            p.Expect(LuaTokenKind.TkRightBracket);

            return m.Complete(p, LuaSyntaxKind.TypeTuple);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.TypeTuple, e.Message);
        }
    }

    private static CompleteMarker LiteralType(LuaDocParser p)
    {
        var m = p.Marker();

        p.Bump();

        return m.Complete(p, LuaSyntaxKind.TypeLiteral);
    }

    private static CompleteMarker FuncOrNameType(LuaDocParser p)
    {
        if (p.CurrentNameText is "fun" or "async")
        {
            return FunType(p);
        }

        var m = p.Marker();
        p.Bump();
        return m.Complete(p, LuaSyntaxKind.TypeName);
    }

    private static CompleteMarker TemplateType(LuaDocParser p)
    {
        var m = p.Marker();
        p.Bump();
        return m.Complete(p, LuaSyntaxKind.TypeTemplate);
    }

    private static CompleteMarker VariadicType(LuaDocParser p)
    {
        var m = p.Marker();
        try
        {
            p.Bump();
            // for any
            p.Accept(LuaTokenKind.TkName);
            return m.Complete(p, LuaSyntaxKind.TypeVariadic);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.TypeVariadic, e.Message);
        }
    }

    public static CompleteMarker FunType(LuaDocParser p)
    {
        var m = p.Marker();
        try
        {
            if (p.CurrentNameText is "async")
            {
                p.Bump();
                if (p.CurrentNameText is "fun")
                {
                    p.Bump();
                }
                else
                {
                    return m.Complete(p, LuaSyntaxKind.TypeName);
                }
            }
            else
            {
                p.Expect(LuaTokenKind.TkName);
            }

            p.Expect(LuaTokenKind.TkLeftParen);
            if (p.Current != LuaTokenKind.TkRightParen)
            {
                var cm = TypedParameter(p);
                while (cm.IsComplete && p.Current is LuaTokenKind.TkComma)
                {
                    p.Bump();
                    cm = TypedParameter(p);
                }

                if (p.Current is LuaTokenKind.TkDots)
                {
                    VariadicTypedParameter(p);
                }
            }

            p.Expect(LuaTokenKind.TkRightParen);
            // ReSharper disable once InvertIf
            if (p.Current is LuaTokenKind.TkColon)
            {
                p.Bump();
                TypeList(p);
            }

            return m.Complete(p, LuaSyntaxKind.TypeFun);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.TypeFun, e.Message);
        }
    }

    private static CompleteMarker TypedParameter(LuaDocParser p)
    {
        var m = p.Marker();
        try
        {
            if (p.Current is LuaTokenKind.TkName)
            {
                p.Bump();
                p.Accept(LuaTokenKind.TkNullable);
            }
            else if (p.Current is LuaTokenKind.TkDots)
            {
                return CompleteMarker.Empty;
            }
            else
            {
                return m.Fail(p, LuaSyntaxKind.TypedParameter, "expect <name> or '...'");
            }

            if (p.Current is LuaTokenKind.TkColon)
            {
                p.Bump();
                Type(p);
            }

            return m.Complete(p, LuaSyntaxKind.TypedParameter);
        }
        catch (UnexpectedTokenException)
        {
            return m.Fail(p, LuaSyntaxKind.TypedParameter, "expect typed parameter");
        }
    }

    private static CompleteMarker VariadicTypedParameter(LuaDocParser p)
    {
        var m = p.Marker();
        try
        {
            if (p.Current is LuaTokenKind.TkDots)
            {
                p.Bump();
            }
            else
            {
                return m.Fail(p, LuaSyntaxKind.TypedParameter, "expect '...'");
            }

            if (p.Current is LuaTokenKind.TkColon)
            {
                p.Bump();
                Type(p);
            }

            return m.Complete(p, LuaSyntaxKind.TypedParameter);
        }
        catch (UnexpectedTokenException)
        {
            return m.Fail(p, LuaSyntaxKind.TypedParameter, "expect typed parameter");
        }
    }
}
