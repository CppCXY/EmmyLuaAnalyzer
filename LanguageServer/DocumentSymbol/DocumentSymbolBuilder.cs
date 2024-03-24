using EmmyLua.CodeAnalysis.Compilation.Semantic;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using LanguageServer.ExtensionUtil;
using OmniSharp.Extensions.LanguageServer.Protocol.Models;
using DocumentSymbolType = OmniSharp.Extensions.LanguageServer.Protocol.Models.DocumentSymbol;

namespace LanguageServer.DocumentSymbol;

// Simple implementation of DocumentSymbolBuilder
public class DocumentSymbolBuilder
{
    public List<DocumentSymbolType> Build(SemanticModel semanticModel)
    {
        var document = semanticModel.Document;
        var symbols = new List<DocumentSymbolType>();
        var source = document.SyntaxTree.SyntaxRoot;
        foreach (var node in source.Descendants)
        {
            switch (node)
            {
                case LuaLocalNameSyntax localName:
                {
                    if (localName is { Name.RepresentText: { } name })
                    {
                        symbols.Add(new DocumentSymbolType()
                        {
                            Name = $"local {name}",
                            Kind = SymbolKind.Variable,
                            Range = localName.Range.ToLspRange(document),
                            SelectionRange = localName.Range.ToLspRange(document)
                        });
                    }

                    break;
                }
                case LuaAssignStatSyntax assignStat:
                {
                    foreach (var expr in assignStat.VarList)
                    {
                        if (expr is LuaNameExprSyntax { Name.RepresentText: {} name }nameExpr)
                        {
                            symbols.Add(new DocumentSymbolType()
                            {
                                Name = name,
                                Kind = SymbolKind.Variable,
                                Range = nameExpr.Range.ToLspRange(document),
                                SelectionRange = nameExpr.Range.ToLspRange(document)
                            });
                        }
                    }
                    break;
                }
                case LuaFuncStatSyntax funcStat:
                {
                    if (funcStat is { IsLocal: true, LocalName.Name.RepresentText: { } name })
                    {
                        symbols.Add(new DocumentSymbolType()
                        {
                            Name = $"local function {name}",
                            Kind = SymbolKind.Function,
                            Range = funcStat.LocalName.Name.Range.ToLspRange(document),
                            SelectionRange = funcStat.LocalName.Name.Range.ToLspRange(document)
                        });
                    }
                    else if (funcStat is { IsLocal: false, NameExpr.Name.RepresentText: { } name2 })
                    {
                        symbols.Add(new DocumentSymbolType()
                        {
                            Name = $"function {name2}",
                            Kind = SymbolKind.Function,
                            Range = funcStat.NameExpr.Name.Range.ToLspRange(document),
                            SelectionRange = funcStat.NameExpr.Name.Range.ToLspRange(document)
                        });
                    }
                    else if (funcStat is {IsMethod: true, IndexExpr.Name: {} name3})
                    {
                        symbols.Add(new DocumentSymbolType()
                        {
                            Name = $"method {name3}",
                            Kind = SymbolKind.Method,
                            Range = funcStat.IndexExpr.Range.ToLspRange(document),
                            SelectionRange = funcStat.IndexExpr.Range.ToLspRange(document)
                        });
                    }
                    break;
                }
                case LuaParamDefSyntax paramDef:
                {
                    if (paramDef is { Name.RepresentText: { } name })
                    {
                        symbols.Add(new DocumentSymbolType()
                        {
                            Name = $"param {name}",
                            Kind = SymbolKind.Variable,
                            Range = paramDef.Name.Range.ToLspRange(document),
                            SelectionRange = paramDef.Name.Range.ToLspRange(document)
                        });
                    }
                    break;
                }
            }
        }


        return symbols;
    }
}