using EmmyLua.CodeAnalysis.Compilation.Reference;
using EmmyLua.CodeAnalysis.Compilation.Type.Compile;
using EmmyLua.CodeAnalysis.Compilation.Type.Types;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;

namespace EmmyLua.CodeAnalysis.Compilation.Analyzer.TypeAnalyzer;

public class TypeAnalyzer(LuaCompilation compilation) : LuaAnalyzer(compilation, "Type")
{
    public override void Analyze(AnalyzeContext analyzeContext)
    {
        foreach (var document in analyzeContext.LuaDocuments)
        {
            var typeContext = new TypeContext(Compilation, document);
            var comments = document.SyntaxTree.SyntaxRoot.Descendants.OfType<LuaCommentSyntax>();
            foreach (var comment in comments)
            {
                AddDocGeneric(comment, typeContext);
                foreach (var typeSyntax in comment.Descendants.OfType<LuaDocTypeSyntax>())
                {
                    TypeCompiler.Compile(typeSyntax, typeContext);
                }
            }
        }
    }

    private void AddDocGeneric(LuaCommentSyntax comment, TypeContext typeContext)
    {
        foreach (var docTag in comment.DocList)
        {
            switch (docTag)
            {
                case LuaDocTagGenericSyntax genericSyntax:
                {
                    foreach (var genericParam in genericSyntax.Params)
                    {
                        if (genericParam.Name is { RepresentText: { } name })
                        {
                            typeContext.AddGenericName(comment.UniqueId, name);
                        }
                    }

                    break;
                }
                case LuaDocTagClassSyntax { GenericDeclareList.Params: { } classGenericParams }:
                {
                    var genericParams = classGenericParams.ToList();
                    foreach (var param in genericParams)
                    {
                        if (param.Name is { RepresentText: { } name })
                        {
                            typeContext.AddGenericName(comment.UniqueId, name);
                        }
                    }

                    AddGenericParamRange(comment, comment.UniqueId, typeContext);
                    break;
                }
                case LuaDocTagInterfaceSyntax { GenericDeclareList.Params: { } interfaceGenericParams }:
                {
                    var genericParams = interfaceGenericParams.ToList();
                    foreach (var param in genericParams)
                    {
                        if (param.Name is { RepresentText: { } name })
                        {
                            typeContext.AddGenericName(comment.UniqueId, name);
                        }
                    }

                    AddGenericParamRange(comment, comment.UniqueId, typeContext);
                    break;
                }
                case LuaDocTagAliasSyntax { GenericDeclareList.Params: { } aliasGenericParams }:
                {
                    var genericParams = aliasGenericParams.ToList();
                    foreach (var param in genericParams)
                    {
                        if (param.Name is { RepresentText: { } name })
                        {
                            typeContext.AddGenericName(comment.UniqueId, name);
                        }
                    }

                    break;
                }
            }
        }
    }

    private void AddGenericParamRange(LuaCommentSyntax commentSyntax, SyntaxElementId id,
        TypeContext context)
    {
        if (commentSyntax is { Owner: { } owner, DocumentId: { } documentId })
        {
            switch (owner)
            {
                case LuaLocalStatSyntax { NameList: { } localNames }:
                {
                    if (localNames.FirstOrDefault() is { } firstLocalDecl)
                    {
                        var references = Compilation.ProjectIndex.QueryLocalReferences(firstLocalDecl.UniqueId);
                        AddGenericEffectRange(id, references.ToList(), context);
                    }

                    break;
                }
                case LuaAssignStatSyntax { VarList: { } varList }:
                {
                    if (varList.FirstOrDefault() is LuaNameExprSyntax firstVarDecl)
                    {
                        var references = Compilation.ProjectIndex.QueryLocalReferences(firstVarDecl.UniqueId);
                        AddGenericEffectRange(id, references.ToList(), context);
                    }

                    break;
                }
                case LuaTableFieldSyntax tableFieldSyntax:
                {
                    context.AddGenericEffectRange(id, tableFieldSyntax.Range);
                    break;
                }
            }
        }
    }

    private void AddGenericEffectRange(SyntaxElementId id, List<LuaReference> references, TypeContext typeContext)
    {
        foreach (var reference in references)
        {
            if (reference.Ptr.ToNode(typeContext.Document) is { } node)
            {
                if (node.AncestorsAndSelf.OfType<ICommentOwner>().FirstOrDefault() is { Comments: { } comments })
                {
                    foreach (var comment in comments)
                    {
                        typeContext.AddGenericEffectRange(id, comment.Range);
                    }
                }
            }
        }
    }
}
