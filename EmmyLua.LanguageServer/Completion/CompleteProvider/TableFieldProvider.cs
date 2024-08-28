﻿using EmmyLua.CodeAnalysis.Compilation.Type.Types;
using EmmyLua.CodeAnalysis.Syntax.Node.SyntaxNodes;
using EmmyLua.LanguageServer.Completion.CompletionData;
using EmmyLua.LanguageServer.Framework.Protocol.Message.Completion;

namespace EmmyLua.LanguageServer.Completion.CompleteProvider;

public class TableFieldProvider : ICompleteProviderBase
{
    public void AddCompletion(CompleteContext context)
    {
        if (context.TriggerToken?.Parent?.Parent is not LuaTableFieldSyntax tableFieldSyntax)
        {
            return;
        }

        if (!tableFieldSyntax.IsValue || context.TriggerToken?.Parent is not LuaNameExprSyntax)
        {
            return;
        }

        if (tableFieldSyntax.ParentTable is { } expr)
        {
            var exprType = context.SemanticModel.Context.InferExprShouldBeType(expr);
            AddTypeMemberCompletion(exprType, context);    
        }
        
        AddMetaFieldCompletion(context);
    }
    
    private void AddTypeMemberCompletion(LuaType type, CompleteContext context)
    {
        var members = context.SemanticModel.Context.GetMembers(type);
        var nameSet = new HashSet<string>();
        foreach (var member in members)
        {
            if (nameSet.Add(member.Name))
            {
                context.CreateCompletion($"{member.Name} = ", member.Type)
                    .WithKind(CompletionItemKind.Property)
                    .WithData(member.RelationInformation)
                    .WithCheckDeclaration(member)
                    .AddToContext();
            }
        }
    }

    private void AddMetaFieldCompletion(CompleteContext context)
    {
        context.AddRange(Metatable.MetaFields);
    }
}