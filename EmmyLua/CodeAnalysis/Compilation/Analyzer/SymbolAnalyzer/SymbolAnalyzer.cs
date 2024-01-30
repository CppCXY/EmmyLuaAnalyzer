using EmmyLua.CodeAnalysis.Compilation.Symbol;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.SymbolAnalyzer;

public class SymbolAnalyzer(LuaCompilation compilation) : LuaAnalyzer(compilation)
{
    public override void Analyze(DocumentId documentId)
    {
        if (Compilation.GetSymbolTree(documentId) is { } tree)
        {
            foreach (var symbol in tree.Symbols)
            {
                switch (symbol)
                {
                    case LocalDeclaration localDeclaration:
                    {
                        LocalDeclarationAnalyze(localDeclaration);
                        break;
                    }
                }
            }
        }
    }

    private void LocalDeclarationAnalyze(LocalDeclaration localDeclaration)
    {

    }

    // private void NamedTypeMerge(ILuaNamedType namedType, LuaTableExprSyntax tableExpr, DocumentId documentId)
    // {
    //     var name = namedType.Name;
    //     var members = Compilation.Stub.Members.Get(Compilation.SearchContext.GetUniqueId(tableExpr));
    //     foreach (var symbol in members)
    //     {
    //         Compilation.Stub.Members.AddStub(documentId, name, symbol);
    //     }
    // }
}
