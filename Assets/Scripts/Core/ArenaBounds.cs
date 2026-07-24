using UnityEngine;

public static class ArenaBounds
{
    public static Vector2 HalfExtents()
    {
        Camera cam = Camera.main;
        if (cam == null) return new Vector2(8f, 5f);
        float height = cam.orthographicSize;
        float width = height * cam.aspect;
        return new Vector2(width, height);
    }

    public static Vector3 Clamp(Vector3 position, float margin)
    {
        Vector2 half = HalfExtents();
        position.x = Mathf.Clamp(position.x, -half.x + margin, half.x - margin);
        position.y = Mathf.Clamp(position.y, -half.y + margin, half.y - margin);
        return position;
    }

    public static bool IsOutside(Vector3 position, float margin = 1f)
    {
        Vector2 half = HalfExtents();
        return Mathf.Abs(position.x) > half.x + margin || Mathf.Abs(position.y) > half.y + margin;
    }
}
