using EmmyLua.CodeAnalysis.Compilation.Reference;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Compilation.Type.Compile;
using EmmyLua.CodeAnalysis.Compilation.Type.Types;
using EmmyLua.CodeAnalysis.Syntax.Node;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Syntax.Visitor;

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
                var visitor = new TypeCompilerVisitor(comment, typeContext);
                AddDocGeneric(comment, typeContext);
                comment.VisitSyntaxNode(visitor);
            }
        }
    }

    private class TypeCompilerVisitor(LuaCommentSyntax comment, TypeContext typeContext) : LuaSyntaxNodeVisitor
    {
        protected override void VisitNode(LuaSyntaxNode node)
        {
            if (node is LuaDocTypeSyntax typeSyntax)
            {
                TypeCompiler.Compile(typeSyntax, comment, typeContext);
                SkipChildren();
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
                            var baseType = genericParam.Type is not null ? new LuaTypeRef(LuaTypeId.Create(genericParam.Type)) : null;
                            typeContext.AddGenericName(comment.UniqueId, name, baseType);
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
                            var baseType = param.Type is not null ? new LuaTypeRef(LuaTypeId.Create(param.Type)) : null;
                            typeContext.AddGenericName(comment.UniqueId, name, baseType);
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
                            var baseType = param.Type is not null ? new LuaTypeRef(LuaTypeId.Create(param.Type)) : null;
                            typeContext.AddGenericName(comment.UniqueId, name, baseType);
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
                            var baseType = param.Type is not null ? new LuaTypeRef(LuaTypeId.Create(param.Type)) : null;
                            typeContext.AddGenericName(comment.UniqueId, name, baseType);
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
        if (commentSyntax is { Owner: { } owner })
        {
            switch (owner)
            {
                case LuaLocalStatSyntax { NameList: { } localNames }:
                {
                    if (localNames.FirstOrDefault() is { } firstLocalDecl)
                    {
                        var references = Compilation.ProjectIndex.QueryLocalReferences(firstLocalDecl.UniqueId);
                        AddGenericEffectId(id, references.ToList(), context);
                    }

                    break;
                }
                case LuaAssignStatSyntax { VarList: { } varList }:
                {
                    if (varList.FirstOrDefault() is LuaNameExprSyntax firstVarDecl)
                    {
                        var references = Compilation.ProjectIndex.QueryLocalReferences(firstVarDecl.UniqueId);
                        AddGenericEffectId(id, references.ToList(), context);
                    }

                    break;
                }
                case LuaTableFieldSyntax tableFieldSyntax:
                {
                    foreach (var luaCommentSyntax in tableFieldSyntax.Descendants.OfType<LuaCommentSyntax>())
                    {
                        context.AddGenericEffectId(id, luaCommentSyntax.UniqueId);
                    }
                    break;
                }
            }
        }
    }

    private void AddGenericEffectId(SyntaxElementId id, List<LuaReference> references, TypeContext typeContext)
    {
        foreach (var reference in references)
        {
            if (reference.Ptr.ToNode(typeContext.Document) is { } node)
            {
                if (node.AncestorsAndSelf.OfType<ICommentOwner>().FirstOrDefault() is { Comments: { } comments })
                {
                    foreach (var comment in comments)
                    {
                        typeContext.AddGenericEffectId(id, comment.UniqueId);
                    }
                }
            }
        }
    }
}
