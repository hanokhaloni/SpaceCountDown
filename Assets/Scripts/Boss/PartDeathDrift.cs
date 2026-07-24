using UnityEngine;

public class PartDeathDrift : MonoBehaviour
{
    Vector2 velocity;
    float angularVelocity;
    float fadeDuration;
    float age;
    SpriteRenderer[] renderers;
    Color[] baseColors;

    public void Init(Vector2 direction, float speed, float spinDegPerSec, float duration)
    {
        velocity = direction * speed;
        angularVelocity = spinDegPerSec;
        fadeDuration = duration;

        renderers = GetComponentsInChildren<SpriteRenderer>(true);
        baseColors = new Color[renderers.Length];
        for (int i = 0; i < renderers.Length; i++)
            baseColors[i] = renderers[i].color;
    }

    void Update()
    {
        age += Time.deltaTime;

        transform.position += (Vector3)(velocity * Time.deltaTime);
        transform.Rotate(0f, 0f, angularVelocity * Time.deltaTime);
        velocity = Vector2.Lerp(velocity, Vector2.zero, 1.5f * Time.deltaTime);

        float t = Mathf.Clamp01(age / fadeDuration);
        for (int i = 0; i < renderers.Length; i++)
        {
            if (renderers[i] == null) continue;
            var c = baseColors[i];
            c.a = Mathf.Lerp(baseColors[i].a, 0f, t);
            renderers[i].color = c;
        }

        if (age >= fadeDuration)
            Destroy(gameObject);
    }
}
