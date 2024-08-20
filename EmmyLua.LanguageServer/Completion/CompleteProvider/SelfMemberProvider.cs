﻿using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.CodeAnalysis.Type;
using EmmyLua.CodeAnalysis.Type.Types;

namespace EmmyLua.LanguageServer.Completion.CompleteProvider;

public class SelfMemberProvider : ICompleteProviderBase
{
    public void AddCompletion(CompleteContext context)
    {
        if (context.TriggerToken?.Parent is not LuaNameExprSyntax)
        {
            return;
        }

        var luaFuncStat = context.TriggerToken.Ancestors.OfType<LuaFuncStatSyntax>().FirstOrDefault();

        if (luaFuncStat is { IsColonFunc: true, IndexExpr.PrefixExpr: { } selfExpr })
        {
            var selfType = context.SemanticModel.Context.Infer(selfExpr);
            var members = context.SemanticModel.Context.GetMembers(selfType);
            foreach (var member in members)
            {
                if (member.Type is LuaMethodType { ColonDefine: true })
                {
                    context.CreateCompletion($"self:{member.Name}", member.Type)
                        .WithData(member.RelationInformation)
                        .WithCheckDeclaration(member)
                        .AddToContext();
                }
                else
                {
                    context.CreateCompletion($"self.{member.Name}", member.Type)
                        .WithData(member.RelationInformation)
                        .WithCheckDeclaration(member)
                        .AddToContext();
                }
            }
        }
    }
}