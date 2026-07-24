using UnityEngine;

public class ExplosionEffect : MonoBehaviour
{
    const float duration = 0.25f;

    SpriteRenderer sr;
    Color baseColor;
    float baseRadius;
    float age;

    public void Init(float radius)
    {
        baseRadius = radius;
        sr = GetComponent<SpriteRenderer>();
        baseColor = sr.color;
        transform.localScale = Vector3.one * baseRadius * 2f;
    }

    void Update()
    {
        age += Time.deltaTime;
        float t = Mathf.Clamp01(age / duration);

        float scale = Mathf.Lerp(baseRadius * 2f, baseRadius * 3.2f, t);
        transform.localScale = Vector3.one * scale;

        var c = baseColor;
        c.a = Mathf.Lerp(baseColor.a, 0f, t);
        sr.color = c;

        if (age >= duration)
            Destroy(gameObject);
    }
}
