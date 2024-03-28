using EmmyLua.CodeAnalysis.Compilation.Semantic;
using EmmyLua.CodeAnalysis.Compilation.Semantic.Reference;
using EmmyLua.CodeAnalysis.Compile.Lexer;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using LanguageServer.Util;
using OmniSharp.Extensions.LanguageServer.Protocol;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;

namespace LanguageServer.Rename;

public class RenameBuilder
{
    public Dictionary<DocumentUri, IEnumerable<TextEdit>> Build(SemanticModel semanticModel, LuaSyntaxElement element,
        string newName)
    {
        if (newName.Length == 0)
        {
            return new();
        }

        newName = newName.Trim();
        var references = semanticModel.FindReferences(element);
        var notSymbolChar = !LuaLexer.IsNameStart(newName.First());
        if (!notSymbolChar)
        {
            notSymbolChar = newName.Skip(1).Any(it => !LuaLexer.IsNameContinue(it));
        }

        var changes = new Dictionary<DocumentUri, IEnumerable<TextEdit>>();
        foreach (var reference in references)
        {
            switch (reference.Element)
            {
                case LuaStringToken stringToken:
                {
                    ChangeStringToken(changes, stringToken, reference, newName);
                    break;
                }
                case LuaNameToken nameToken:
                {
                    ChangeNameToken(changes, nameToken, reference, newName, notSymbolChar);
                    break;
                }
                case LuaLocalNameSyntax localNameSyntax:
                {
                    ChangeLocalName(changes, localNameSyntax, reference, newName, notSymbolChar);
                    break;
                }
                case LuaNameExprSyntax nameExprSyntax:
                {
                    if (nameExprSyntax.Name is not null)
                    {
                        ChangeNameToken(changes, nameExprSyntax.Name, reference, newName, notSymbolChar);
                    }

                    break;
                }
                case LuaParamDefSyntax paramDefSyntax:
                {
                    if (paramDefSyntax.Name is not null)
                    {
                        ChangeNameToken(changes, paramDefSyntax.Name, reference, newName, notSymbolChar);
                    }
                    break;
                }
            }
        }

        return changes;
    }

    private void AddChange(Dictionary<DocumentUri, IEnumerable<TextEdit>> changes, LuaLocation location, string newName)
    {
        var uri = location.Document.Uri;
        if (!changes.TryGetValue(uri, out var edits))
        {
            edits = new List<TextEdit>();
            changes[uri] = edits;
        }

        var edit = new TextEdit()
        {
            Range = location.Range.ToLspRange(location.Document),
            NewText = newName
        };
        ((List<TextEdit>)edits).Add(edit);
    }

    private void ChangeStringToken(Dictionary<DocumentUri, IEnumerable<TextEdit>> changes, LuaStringToken stringToken,
        LuaReference luaReference, string newName)
    {
        var range = stringToken.Range;
        if (range.Length < 2)
        {
            return;
        }

        range = range with { StartOffset = range.StartOffset + 1, Length = range.Length - 2 };
        AddChange(changes, new LuaLocation(luaReference.Location.Document, range), newName);
    }

    private void ChangeNameToken(Dictionary<DocumentUri, IEnumerable<TextEdit>> changes, LuaNameToken nameToken,
        LuaReference luaReference, string newName, bool notSymbolChar)
    {
        // give up rename
        if (notSymbolChar)
        {
            return;
        }

        AddChange(changes, luaReference.Location, newName);
    }
    
    private void ChangeLocalName(Dictionary<DocumentUri, IEnumerable<TextEdit>> changes, LuaLocalNameSyntax localNameSyntax,
        LuaReference luaReference, string newName, bool notSymbolChar)
    {
        // give up rename
        if (notSymbolChar)
        {
            return;
        }
        
        AddChange(changes, luaReference.Location, newName);
    }
}