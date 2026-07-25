using UnityEngine;

public class MuzzleSmoke : MonoBehaviour
{
    const int particleCount = 10;
    const float minPixels = 50f;
    const float maxPixels = 100f;
    const float pixelsPerUnit = 32f; // matches Shapes.Circle's world-unit convention
    const float lifetime = 0.4f;

    public static void Spawn(Vector3 position, Vector2 direction)
    {
        if (direction.sqrMagnitude < 0.0001f) direction = Vector2.up;
        direction.Normalize();

        for (int i = 0; i < particleCount; i++)
        {
            var go = new GameObject("MuzzleSmoke");
            go.transform.position = position;
            go.transform.localScale = Vector3.one * Random.Range(0.08f, 0.16f);

            float grey = Random.Range(0.4f, 0.8f);
            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = Shapes.Circle(new Color(grey, grey, grey, 0.8f));
            sr.sortingOrder = 4;

            float spreadDeg = Random.Range(-20f, 20f);
            Vector2 particleDir = Quaternion.Euler(0f, 0f, spreadDeg) * direction;
            float distanceUnits = Random.Range(minPixels, maxPixels) / pixelsPerUnit;

            go.AddComponent<MuzzleSmoke>().Init(particleDir, distanceUnits);
        }
    }

    Vector2 velocity;
    SpriteRenderer sr;
    Color baseColor;
    float age;

    void Init(Vector2 direction, float distance)
    {
        sr = GetComponent<SpriteRenderer>();
        baseColor = sr.color;
        velocity = direction * (distance / lifetime);
    }

    void Update()
    {
        age += Time.deltaTime;
        transform.position += (Vector3)(velocity * Time.deltaTime);

        float t = Mathf.Clamp01(age / lifetime);
        var c = baseColor;
        c.a = Mathf.Lerp(baseColor.a, 0f, t);
        sr.color = c;

        if (age >= lifetime)
            Destroy(gameObject);
    }
}
