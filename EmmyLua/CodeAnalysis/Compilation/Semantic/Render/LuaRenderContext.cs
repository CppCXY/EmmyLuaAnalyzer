using System.Text;
using EmmyLua.CodeAnalysis.Compilation.Infer;
using EmmyLua.CodeAnalysis.Compilation.Type;

namespace EmmyLua.CodeAnalysis.Compilation.Semantic.Render;

public class LuaRenderContext(SearchContext searchContext, LuaRenderFeature feature)
{
    public SearchContext SearchContext { get; } = searchContext;

    public LuaRenderFeature Feature { get; } = feature;

    private StringBuilder _sb = new StringBuilder();

    private HashSet<LuaNamedType> _typeLinks = [];

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

    public void WrapperLuaAppend(string text)
    {
        _sb.Append("```lua\n");
        _sb.Append(text);
        _sb.Append("\n```\n");
    }

    public string GetText()
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

        return _sb.ToString();
    }

    public void AddTypeLink(LuaType type)
    {
        if (Feature.ShowTypeLink && type is LuaNamedType namedType)
        {
            _typeLinks.Add(namedType);
        }
    }
}
