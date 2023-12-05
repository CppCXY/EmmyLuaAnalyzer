using LuaLanguageServer.CodeAnalysis.Compilation.Infer;
using LuaLanguageServer.CodeAnalysis.Compilation.Symbol;
using LuaLanguageServer.CodeAnalysis.Syntax.Node;
using LuaLanguageServer.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace LuaLanguageServer.CodeAnalysis.Compilation.Type;

public interface ILuaType
{
    public IEnumerable<ILuaSymbol> GetMembers(SearchContext context);

    public IEnumerable<ILuaSymbol> IndexMember(IndexKey key, SearchContext context);

    public bool SubTypeOf(ILuaType other, SearchContext context);

    public bool AcceptExpr(LuaExprSyntax expr, SearchContext context);

    public TypeKind Kind { get; }
}

public abstract record IndexKey
{
    public record Integer(long Value) : IndexKey;

    public record String(string Value) : IndexKey;

    public record Ty(ILuaType Value) : IndexKey;

    public static IndexKey? FromIndexExpr(LuaIndexExprSyntax expr, SearchContext context)
    {
        switch (expr)
        {
            case { DotOrColonIndexName : { } name }:
            {
                return new String(name.RepresentText);
            }
            case { IndexKeyExpr: LuaLiteralExprSyntax { Literal: { } literal } }:
            {
                switch (literal)
                {
                    case LuaStringToken stringToken:
                        return new String(stringToken.InnerString);
                    case LuaIntegerToken integerToken:
                        return new Integer(integerToken.Value);
                }

                break;
            }
        }

        return null;
    }

    public static IndexKey? FromTableFieldExpr(LuaTableFieldSyntax field, SearchContext context)
    {
        switch (field)
        {
            case { NameKey : { } name }:
            {
                return new String(name.RepresentText);
            }
            case { NumberKey: LuaIntegerToken numberKey }:
            {
                return new Integer(numberKey.Value);
            }
            case { StringKey: { } stringKey }:
            {
                return new String(stringKey.InnerString);
            }
        }

        return null;
    }

    public static IndexKey FromDocTypedField(LuaDocTagTypedFieldSyntax field, SearchContext context)
    {
        // switch (expr)
        // {
        //     case { DotOrColonIndexName : { } name }:
        //     {
        //         return new String(name.RepresentText);
        //     }
        //     case { IndexKeyExpr: LuaLiteralExprSyntax { Literal: { } literal } }:
        //     {
        //         switch (literal)
        //         {
        //             case LuaStringToken stringToken:
        //                 return new String(stringToken.RepresentText);
        //             case LuaIntegerToken integerToken:
        //                 return new Integer(integerToken.Value);
        //         }
        //
        //         break;
        //     }
        // }
        //
        // return new Ty(context.Infer(expr.IndexKeyExpr));
        throw new NotImplementedException();
    }

    public static IndexKey FromDocField(LuaDocTagFieldSyntax tagField, SearchContext context)
    {
        // switch (expr)
        // {
        //     case { DotOrColonIndexName : { } name }:
        //     {
        //         return new String(name.RepresentText);
        //     }
        //     case { IndexKeyExpr: LuaLiteralExprSyntax { Literal: { } literal } }:
        //     {
        //         switch (literal)
        //         {
        //             case LuaStringToken stringToken:
        //                 return new String(stringToken.RepresentText);
        //             case LuaIntegerToken integerToken:
        //                 return new Integer(integerToken.Value);
        //         }
        //
        //         break;
        //     }
        // }
        //
        // return new Ty(context.Infer(expr.IndexKeyExpr));
        throw new NotImplementedException();
    }
}

public interface ILuaNamedType : ILuaType
{
    public string Name { get; }

    public IEnumerable<GenericParam> GetGenericParams(SearchContext context);
}

public interface IGeneric : ILuaType
{
    public ILuaNamedType GetBaseType(SearchContext context);

    public IEnumerable<ILuaType> GetGenericArgs(SearchContext context);
}
