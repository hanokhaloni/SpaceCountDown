using UnityEngine;

public class BackgroundGrid : MonoBehaviour
{
    const int textureSize = 64;
    const int sortingOrder = -1000;

    float scrollSpeed;
    float rotationSpeedDegPerSec;

    Material material;
    Vector2 scrollOffset;

    public void Init(Color background, Color line, float cellWorldSize, float lineThickness01, float scroll, float rotationSpeed)
    {
        float cellSize = Mathf.Max(0.1f, cellWorldSize);
        scrollSpeed = scroll;
        rotationSpeedDegPerSec = rotationSpeed;

        BuildSprite(background, line, cellSize, Mathf.Clamp01(lineThickness01));
    }

    void BuildSprite(Color background, Color line, float cellSize, float lineThickness01)
    {
        var go = new GameObject("BackgroundGridSprite");
        go.transform.SetParent(transform, false);

        Vector2 half = ArenaBounds.HalfExtents();
        float diagonal = half.magnitude * 2.5f;

        // pixelsPerUnit is chosen so one texture tile (textureSize px) maps to exactly cellSize world units.
        float pixelsPerUnit = textureSize / cellSize;
        var sprite = Sprite.Create(BuildTexture(background, line, lineThickness01), new Rect(0f, 0f, textureSize, textureSize), new Vector2(0.5f, 0.5f), pixelsPerUnit);

        var sr = go.AddComponent<SpriteRenderer>();
        sr.drawMode = SpriteDrawMode.Tiled;
        sr.sprite = sprite;
        sr.size = new Vector2(diagonal, diagonal);
        sr.sortingOrder = sortingOrder;

        material = sr.material;
    }

    Texture2D BuildTexture(Color background, Color line, float lineThickness01)
    {
        var tex = new Texture2D(textureSize, textureSize, TextureFormat.RGBA32, false);
        tex.wrapMode = TextureWrapMode.Repeat;
        tex.filterMode = FilterMode.Bilinear;

        int thicknessPx = Mathf.Max(1, Mathf.RoundToInt(textureSize * lineThickness01));
        for (int y = 0; y < textureSize; y++)
        {
            for (int x = 0; x < textureSize; x++)
            {
                bool onLine = x < thicknessPx || y < thicknessPx;
                tex.SetPixel(x, y, onLine ? line : background);
            }
        }
        tex.Apply();
        return tex;
    }

    void Update()
    {
        if (material == null) return;

        scrollOffset += Vector2.down * (scrollSpeed * Time.deltaTime);
        material.mainTextureOffset = scrollOffset;

        transform.Rotate(0f, 0f, rotationSpeedDegPerSec * Time.deltaTime);
    }
}
