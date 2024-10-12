using EmmyLua.CodeAnalysis.Compilation.Reference;
using EmmyLua.CodeAnalysis.Compilation.Type;
using EmmyLua.CodeAnalysis.Compilation.Type.Compile;
using EmmyLua.CodeAnalysis.Compilation.Type.Types;
using EmmyLua.CodeAnalysis.Compile.Kind;
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
            var comments = document.SyntaxTree.SyntaxRoot.Iter.DescendantsOfKind(LuaSyntaxKind.Comment);
            foreach (var commentIt in comments)
            {
                AddDocGeneric(commentIt, typeContext);
                var stack = new Stack<SyntaxIterator>();
                stack.Push(commentIt);
                while (stack.Count > 0)
                {
                    var it = stack.Pop();
                    if (LuaDocTypeSyntax.CanCast(it.Kind))
                    {
                        var typeSyntax = it.ToNode<LuaDocTypeSyntax>();
                        if (typeSyntax is not null)
                        {
                            TypeCompiler.Compile(typeSyntax, commentIt.UniqueId, typeContext);
                        }
                    }
                    else
                    {
                        foreach (var child in it.Children.Reverse())
                        {
                            if (child.Kind != LuaSyntaxKind.None)
                            {
                                stack.Push(child);
                            }
                        }
                    }
                }
            }
        }
    }

    private void AddDocGeneric(SyntaxIterator commentIt, TypeContext typeContext)
    {
        foreach (var docTagIt in commentIt.NextOf(it => LuaDocTagSyntax.CanCast(it.Kind)))
        {
            var docTag = docTagIt.ToNode<LuaDocTagSyntax>();
            switch (docTag)
            {
                case LuaDocTagGenericSyntax genericSyntax:
                {
                    foreach (var genericParam in genericSyntax.Params)
                    {
                        if (genericParam.Name is { RepresentText: { } name })
                        {
                            var baseType = genericParam.Type is not null
                                ? new LuaTypeRef(LuaTypeId.Create(genericParam.Type))
                                : null;
                            typeContext.AddGenericName(commentIt.UniqueId, name, baseType);
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
                            typeContext.AddGenericName(commentIt.UniqueId, name, baseType);
                        }
                    }

                    AddGenericParamRange(commentIt, commentIt.UniqueId, typeContext);
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
                            typeContext.AddGenericName(commentIt.UniqueId, name, baseType);
                        }
                    }

                    AddGenericParamRange(commentIt, commentIt.UniqueId, typeContext);
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
                            typeContext.AddGenericName(commentIt.UniqueId, name, baseType);
                        }
                    }

                    break;
                }
            }
        }
    }

    private void AddGenericParamRange(SyntaxIterator commentIt, SyntaxElementId id,
        TypeContext context)
    {
        var commentSyntax = commentIt.ToNode<LuaCommentSyntax>();
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
                    foreach (var it in tableFieldSyntax.Iter.DescendantsOfKind(LuaSyntaxKind.Comment))
                    {
                        context.AddGenericEffectId(id, it.UniqueId);
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
            var it = reference.Ptr.ToIter(typeContext.Document);
            var commentOwnerIt = it.AncestorsAndSelf.FirstOrDefault(it2 => LuaCommentSyntax.CanOwner(it2.Kind));
            if (commentOwnerIt.IsValid && commentOwnerIt.ToElement() is ICommentOwner { Comments: { } comments })
            {
                foreach (var comment in comments)
                {
                    typeContext.AddGenericEffectId(id, comment.UniqueId);
                }
            }
        }
    }
}
