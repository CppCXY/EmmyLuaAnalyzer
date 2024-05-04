﻿using System.Text;
using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Compilation.Semantic.Render.Renderer;
using EmmyLua.CodeAnalysis.Compilation.Type;

namespace EmmyLua.CodeAnalysis.Compilation.Semantic.Render;

public class LuaRenderContext(SearchContext searchContext, LuaRenderFeature feature)
{
    public SearchContext SearchContext { get; } = searchContext;

    public LuaRenderFeature Feature { get; } = feature;

    private StringBuilder _sb = new StringBuilder();

    private HashSet<LuaNamedType> _typeLinks = [];

    private HashSet<LuaNamedType> _aliasExpand = [];

    private bool _allowExpandAlias = false;

    public void Append(string text)
    {
        _sb.Append(text);
    }

    public void AppendLine(string text)
    {
        _sb.AppendLine(text);
    }

    public void AppendLine()
    {
        _sb.AppendLine();
    }

    public void Append(char ch)
    {
        _sb.Append(ch);
    }

    public void AddSeparator()
    {
        _sb.Append("\n___\n");
    }

    public void WrapperLua(Action action)
    {
        _sb.Append("```lua\n");
        action();
        _sb.Append("\n```\n");
    }

    public void WrapperLanguage(string language, Action action)
    {
        _sb.Append("```");
        _sb.Append(language);
        _sb.Append("\n");
        action();
        _sb.Append("\n```\n");
    }

    public void WrapperLuaAppend(string text)
    {
        _sb.Append("```lua\n");
        _sb.Append(text);
        _sb.Append("\n```\n");
    }

    public void EnableAliasRender()
    {
        _allowExpandAlias = true;
    }

    public string GetText()
    {
        if (_allowExpandAlias)
        {
            RenderAliasExpand();
        }

        RenderLink();
        return _sb.ToString();
    }

    private void RenderLink()
    {
        if (Feature.ShowTypeLink && _typeLinks.Count != 0)
        {
            AddSeparator();
            var typeList = _typeLinks.ToList();
            for (var index = 0; index < typeList.Count; index++)
            {
                var type = typeList[index];
                var typeDeclaration = SearchContext.Compilation.DbManager.GetNamedType(type.Name).FirstOrDefault();
                if (typeDeclaration is {Info.Ptr: { } ptr} && ptr.ToNode(SearchContext) is { } node)
                {
                    if (index == 0)
                    {
                        Append("Go to ");
                    }
                    else if (index > 0)
                    {
                        Append('|');
                    }

                    Append($"[{type.Name}]({node.Location.ToUriLocation(1)})");
                }
            }
        }
    }

    private void RenderAliasExpand()
    {
        if (_aliasExpand.Count != 0)
        {
            foreach (var type in _aliasExpand)
            {
                var name = type.Name;
                var originType = SearchContext.Compilation.DbManager.GetAliasOriginType(name).FirstOrDefault();
                if (originType is LuaAggregateType aggregateType)
                {
                    LuaTypeRenderer.RenderAliasMember(name, aggregateType, this);
                }
            }
        }
    }

    public void AddTypeLink(LuaType type)
    {
        if (Feature.ShowTypeLink && type is LuaNamedType namedType)
        {
            _typeLinks.Add(namedType);
        }

        if (_allowExpandAlias && type is LuaNamedType namedType2)
        {
            var detailType = namedType2.GetDetailType(SearchContext);
            if (detailType.IsAlias)
            {
                _aliasExpand.Add(namedType2);
            }
        }
    }
}