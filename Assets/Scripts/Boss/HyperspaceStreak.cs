using UnityEngine;

public class HyperspaceStreak : MonoBehaviour
{
    const float duration = 0.3f;

    SpriteRenderer sr;
    Vector3 bottomAnchor;
    Color baseColor;
    float startLength;
    float width;
    float age;

    public void Init(Vector3 anchorBottom, float length, float streakWidth, Color color)
    {
        bottomAnchor = anchorBottom;
        startLength = length;
        width = streakWidth;
        baseColor = color;

        sr = GetComponent<SpriteRenderer>();
        sr.color = color;

        ApplyLength(length);
    }

    void Update()
    {
        age += Time.deltaTime;
        float t = Mathf.Clamp01(age / duration);
        float eased = t * t;

        float length = Mathf.Lerp(startLength, 0.05f, eased);
        ApplyLength(length);

        var c = baseColor;
        c.a = Mathf.Lerp(baseColor.a, 0f, t);
        sr.color = c;

        if (age >= duration)
            Destroy(gameObject);
    }

    void ApplyLength(float length)
    {
        transform.position = bottomAnchor + new Vector3(0f, length / 2f, 0f);
        transform.localScale = new Vector3(width, length, 1f);
    }
}
