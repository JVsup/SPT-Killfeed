using UnityEngine;

namespace SPT.Killfeed.Utilities;

public static class GLDrawer
{
    private static GUIStyle _style;
    private static Texture2D _texture;

    private static GUIStyle Style(int size, TextAnchor anchor, Font font)
    {
        _style ??= new GUIStyle(GUI.skin.label);
        _style.fontSize = size;
        _style.alignment = anchor;
        _style.font = font;
        _style.richText = false;
        return _style;
    }

    private static Texture2D Texture()
    {
        if (_texture)
        {
            return _texture;
        }

        _texture = new Texture2D(1, 1, TextureFormat.ARGB32, false)
        {
            filterMode = FilterMode.Bilinear
        };
        _texture.SetPixel(0, 0, Color.white);
        _texture.Apply();
        return _texture;
    }

    public static void DrawText(
        string text,
        Vector2 position,
        int size,
        Color color,
        TextAnchor anchor,
        Font font,
        bool backdrop)
    {
        var style = Style(size, anchor, font);
        var content = new GUIContent(text);
        var dimensions = style.CalcSize(content);
        var rect = new Rect(position.x, position.y, dimensions.x + 8f, dimensions.y + 2f);

        if (anchor == TextAnchor.MiddleCenter
            || anchor == TextAnchor.MiddleLeft
            || anchor == TextAnchor.MiddleRight)
        {
            rect.y -= rect.height * 0.5f;
        }

        if (anchor == TextAnchor.MiddleCenter
            || anchor == TextAnchor.UpperCenter
            || anchor == TextAnchor.LowerCenter)
        {
            rect.x -= rect.width * 0.5f;
        }

        if (anchor == TextAnchor.MiddleRight
            || anchor == TextAnchor.UpperRight
            || anchor == TextAnchor.LowerRight)
        {
            rect.x -= rect.width;
        }

        if (backdrop)
        {
            var backgroundColor = new Color(0f, 0f, 0f, color.a * 0.4f);
            var backgroundRect = new Rect(rect.x - 2f, rect.y - 1f, rect.width + 4f, rect.height + 2f);
            var previousColor = GUI.color;
            GUI.color = backgroundColor;
            GUI.DrawTexture(backgroundRect, Texture());
            GUI.color = previousColor;
        }

        GUI.color = color;
        GUI.Label(rect, text, style);
    }

    public static float MeasureTextWidth(string text, int size, Font font)
    {
        if (string.IsNullOrEmpty(text))
        {
            return 0f;
        }

        var style = new GUIStyle(GUI.skin.label)
        {
            fontSize = size,
            font = font
        };
        return style.CalcSize(new GUIContent(text)).x;
    }
}

