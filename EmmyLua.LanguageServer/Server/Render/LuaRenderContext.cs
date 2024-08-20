using System.Text;
using EmmyLua.CodeAnalysis.Compilation.Search;
using EmmyLua.CodeAnalysis.Type;
using EmmyLua.CodeAnalysis.Type.Manager.TypeInfo;
using EmmyLua.CodeAnalysis.Type.Types;
using EmmyLua.LanguageServer.Server.Render.Renderer;

namespace EmmyLua.LanguageServer.Server.Render;

public class LuaRenderContext(SearchContext searchContext, LuaRenderFeature feature)
{
    public SearchContext SearchContext { get; } = searchContext;

    public LuaRenderFeature Feature { get; } = feature;

    private StringBuilder _sb = new StringBuilder();

    private HashSet<TypeInfo> _typeLinks = [];

    private HashSet<TypeInfo> _aliasExpand = [];

    private bool _allowExpandAlias = false;

    public bool InSignature { get; set; } = false;

    public void Append(string text)
    {
        _sb.Append(text);
    }

    public void AppendLine(string text)
    {
        _sb.Append($"{text}\n");
    }

    public void AppendLine()
    {
        _sb.Append('\n');
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
        RenderAliasExpand();

        if (Feature.ShowTypeLink)
        {
            RenderLink();
        }

        return _sb.ToString();
    }

    public void AddH1Title(string title)
    {
        _sb.Append("# ");
        _sb.Append(title);
    }

    public void AddH2Title(string title)
    {
        _sb.Append("## ");
        _sb.Append(title);
    }

    public void AddH3Title(string title)
    {
        _sb.Append("### ");
        _sb.Append(title);
    }

    private void RenderLink()
    {
        if (Feature.ShowTypeLink && _typeLinks.Count != 0)
        {
            var gotoList = new List<string>();
            var typeList = _typeLinks.ToList();
            foreach (var typeInfo in typeList)
            {
                var location = typeInfo.GetLocation(SearchContext);
                if (location is not null)
                {
                    gotoList.Add($"[{typeInfo.Name}]({location.LspLocation})");
                }
            }
            
            if (gotoList.Count != 0)
            {
                Append("\n\n");
                Append("Go to: ");
                Append(string.Join("|", gotoList));
                AppendLine();
            }
        }
    }

    private void RenderAliasExpand()
    {
        if (_aliasExpand.Count != 0)
        {
            foreach (var typeInfo in _aliasExpand)
            {
                var originType = typeInfo.BaseType;
                if (originType is LuaAggregateType aggregateType)
                {
                    LuaTypeRenderer.RenderAliasMember(typeInfo.Name, aggregateType, this);
                }
            }
        }
    }

    public void AddTypeLink(LuaType type)
    {
        if (Feature.ShowTypeLink && type is LuaNamedType namedType)
        { 
            var typeInfo = SearchContext.Compilation.TypeManager.FindTypeInfo(namedType);
            if (typeInfo is not null)
            {
                _typeLinks.Add(typeInfo);
            }
        }

        if (_allowExpandAlias && type is LuaNamedType namedType2)
        {
            AddAliasExpand(namedType2);
        }
    }

    public void AddAliasExpand(LuaNamedType type)
    {
        var typeInfo = SearchContext.Compilation.TypeManager.FindTypeInfo(type);
        if (typeInfo?.Kind == NamedTypeKind.Alias)
        {
            _aliasExpand.Add(typeInfo);
        }
    }
    
    public void WithSignature(Action action)
    {
        InSignature = true;
        action();
        InSignature = false;
    }
}
