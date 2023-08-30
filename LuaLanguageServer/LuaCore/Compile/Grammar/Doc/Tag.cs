using System.Diagnostics;
using LuaLanguageServer.LuaCore.Compile.Lexer;
using LuaLanguageServer.LuaCore.Compile.Parser;
using LuaLanguageServer.LuaCore.Kind;

namespace LuaLanguageServer.LuaCore.Compile.Grammar.Doc;

public static class TagParser
{
    public static CompleteMarker Tag(LuaDocParser p)
    {
        p.SetState(LuaDocLexerState.Tag);
        // ReSharper disable once SwitchStatementHandlesSomeKnownEnumValuesWithDefault
        switch (p.Current)
        {
            case LuaTokenKind.TkTagClass:
            {
                return TagClass(p);
            }
            case LuaTokenKind.TkTagEnum:
            {
                return TagEnum(p);
            }
            case LuaTokenKind.TkTagInterface:
            {
                return TagInterface(p);
            }
            case LuaTokenKind.TkTagAlias:
            {
                return TagAlias(p);
            }
            case LuaTokenKind.TkTagField:
            {
                return TagField(p);
            }
            case LuaTokenKind.TkTagType:
            {
                return TagType(p);
            }
            case LuaTokenKind.TkTagParam:
            {
                return TagParam(p);
            }
            case LuaTokenKind.TkTagReturn:
            {
                return TagReturn(p);
            }
            case LuaTokenKind.TkTagGeneric:
            {
                return TagGeneric(p);
            }
            case LuaTokenKind.TkTagSee:
            {
                return TagSee(p);
            }
            case LuaTokenKind.TkTagOverload:
            {
                return TagOverload(p);
            }
            case LuaTokenKind.TkTagAsync:
            {
                return TagAsync(p);
            }
            case LuaTokenKind.TkTagCast:
            {
                return TagCast(p);
            }
            case LuaTokenKind.TkTagDeprecated:
            {
                return TagDeprecated(p);
            }
            case LuaTokenKind.TkTagOther:
            {
                return TagOther(p);
            }
            case LuaTokenKind.TkWhitespace:
            {
                p.Bump();
                p.Events.Add(new MarkEvent.Error("expected <tag> but got whitespace"));
                p.SetState(LuaDocLexerState.Trivia);
                p.Accept(LuaTokenKind.TkDocTrivia);
                return CompleteMarker.Empty;
            }
            default:
            {
                throw new UnreachableException();
            }
        }
    }

    public static CompleteMarker LongDocTag(LuaDocParser p)
    {
        throw new NotImplementedException();
    }

    enum ClassSuffixState
    {
        None,
        Generic,
        Extend,
        TableType,
        Description
    }

    private static CompleteMarker TagClass(LuaDocParser p)
    {
        p.SetState(LuaDocLexerState.Normal);
        var m = p.Marker();
        p.Bump();
        try
        {
            p.Expect(LuaTokenKind.TkName);
            var state = ClassSuffixState.None;
            var rollbackPoint = p.GetRollbackPoint();
            do
            {
                switch (p.Current)
                {
                    // generic
                    case LuaTokenKind.TkLt:
                    {
                        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
                        if (state is not ClassSuffixState.None)
                        {
                            goto default;
                        }

                        if (GenericDeclareList(p).IsComplete)
                        {
                            state = ClassSuffixState.Generic;
                            rollbackPoint = p.GetRollbackPoint();
                        }
                        else
                        {
                            state = ClassSuffixState.Description;
                        }

                        break;
                    }
                    // extends
                    case LuaTokenKind.TkColon:
                    {
                        p.Bump();
                        TypesParser.TypeList(p);
                        break;
                    }
                    // class table define
                    case LuaTokenKind.TkLeftBrace:
                    {
                        TypesParser.TableType(p);
                        break;
                    }
                    case LuaTokenKind.TkDocDescriptionStart:
                    {
                        p.Bump();
                        p.SetState(LuaDocLexerState.Description);
                        p.Accept(LuaTokenKind.TkDocDescription);
                        break;
                    }
                    default:
                    {
                        p.Rollback(rollbackPoint);
                        p.SetState(LuaDocLexerState.Description);
                        p.Accept(LuaTokenKind.TkDocDescription);
                        break;
                    }
                }
            } while (false);

            return m.Complete(p, LuaSyntaxKind.DocClass);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.DocClass, e.Message);
        }
    }

    public static CompleteMarker GenericDeclareList(LuaDocParser p)
    {
        var m = p.Marker();
        p.Bump();
        try
        {
            p.Expect(LuaTokenKind.TkName);
            while (p.Current is LuaTokenKind.TkComma)
            {
                p.Bump();
                p.Expect(LuaTokenKind.TkName);
            }

            p.Expect(LuaTokenKind.TkGt);
            return m.Complete(p, LuaSyntaxKind.DocGenericDeclareList);
        }
        catch (UnexpectedTokenException e)
        {
            return m.Fail(p, LuaSyntaxKind.DocGenericDeclareList, e.Message);
        }
    }

    private static CompleteMarker TagEnum(LuaDocParser p)
    {
    }

    private static CompleteMarker TagInterface(LuaDocParser p)
    {
    }

    private static CompleteMarker TagAlias(LuaDocParser p)
    {
    }

    private static CompleteMarker TagField(LuaDocParser p)
    {
    }

    private static CompleteMarker TagType(LuaDocParser p)
    {
    }

    private static CompleteMarker TagParam(LuaDocParser p)
    {
    }

    private static CompleteMarker TagReturn(LuaDocParser p)
    {
    }

    private static CompleteMarker TagGeneric(LuaDocParser p)
    {
    }

    private static CompleteMarker TagSee(LuaDocParser p)
    {
    }

    private static CompleteMarker TagOverload(LuaDocParser p)
    {
    }

    private static CompleteMarker TagAsync(LuaDocParser p)
    {
    }

    private static CompleteMarker TagCast(LuaDocParser p)
    {
    }

    private static CompleteMarker TagDeprecated(LuaDocParser p)
    {
    }

    private static CompleteMarker TagOther(LuaDocParser p)
    {
    }
}
