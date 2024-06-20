namespace EmmyLua.LanguageServer.Server.Render;

public record LuaRenderFeature(
    // 展开alias
    bool ExpandAlias = true,
    // 显示类型链接
    bool ShowTypeLink = true,
    // Hint最大类型展示字数，超过则显示省略号
    bool InHint = false,
    // 字符串最大预览字数，超过则显示省略号
    int MaxStringPreviewLength = 100,
    bool InHover = false
);
