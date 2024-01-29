using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Document;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.Members;

public class MembersAnalyzer(LuaCompilation compilation) : LuaAnalyzer(compilation)
{
    public override void Analyze(DocumentId documentId)
    {
        // if (Compilation.GetSymbolTree(documentId) is { } tree)
        // {
        //     foreach (var symbol in tree.Symbols)
        //     {
        //         switch (symbol)
        //         {
        //             case { IsLocalDeclaration: true, DeclarationType: ILuaNamedType namedType, RelatedExpr: LuaTableExprSyntax relatedExpr }:
        //             {
        //                 NamedTypeMerge(namedType, relatedExpr, documentId);
        //                 break;
        //             }
        //             case { IsGlobalDeclaration: true, DeclarationType: ILuaNamedType namedType, RelatedExpr: LuaTableExprSyntax relatedExpr }:
        //             {
        //                 NamedTypeMerge(namedType, relatedExpr, documentId);
        //                 break;
        //             }
        //             case { IsClassMember: true, DeclarationType: ILuaNamedType namedType, RelatedExpr: LuaTableExprSyntax relatedExpr }:
        //             {
        //                 NamedTypeMerge(namedType, relatedExpr, documentId);
        //                 break;
        //             }
        //         }
        //     }
        // }
    }

    private void NamedTypeMerge(ILuaNamedType namedType, LuaTableExprSyntax tableExpr, DocumentId documentId)
    {
        var name = namedType.Name;
        var members = Compilation.Stub.Members.Get(Compilation.SearchContext.GetUniqueId(tableExpr));
        foreach (var symbol in members)
        {
            Compilation.Stub.Members.AddStub(documentId, name, symbol);
        }
    }
}
