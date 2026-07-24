using System.Collections.Generic;
using UnityEngine;

public static class Shapes
{
    static readonly Dictionary<string, Sprite> cache = new Dictionary<string, Sprite>();

    public static Sprite Circle(Color color, int pixelSize = 32)
    {
        string key = $"circle_{ColorUtility.ToHtmlStringRGBA(color)}_{pixelSize}";
        if (cache.TryGetValue(key, out var cached)) return cached;

        var tex = new Texture2D(pixelSize, pixelSize, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Vector2 center = new Vector2(pixelSize / 2f, pixelSize / 2f);
        float radius = pixelSize / 2f - 1f;

        for (int y = 0; y < pixelSize; y++)
        {
            for (int x = 0; x < pixelSize; x++)
            {
                float dist = Vector2.Distance(new Vector2(x + 0.5f, y + 0.5f), center);
                float alpha = Mathf.Clamp01(radius - dist + 1f);
                tex.SetPixel(x, y, new Color(color.r, color.g, color.b, color.a * alpha));
            }
        }
        tex.Apply();

        var sprite = Sprite.Create(tex, new Rect(0, 0, pixelSize, pixelSize), new Vector2(0.5f, 0.5f), pixelSize);
        cache[key] = sprite;
        return sprite;
    }
}
