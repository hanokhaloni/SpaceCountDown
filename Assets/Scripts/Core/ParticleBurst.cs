using UnityEngine;

public class ParticleBurst : MonoBehaviour
{
    public static void Spawn(Vector3 position, Color color, int count = 10, float speed = 3f, float life = 0.4f)
    {
        for (int i = 0; i < count; i++)
        {
            var go = new GameObject("Particle");
            go.transform.position = position;
            go.transform.localScale = Vector3.one * Random.Range(0.08f, 0.18f);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = Shapes.Circle(color);
            sr.sortingOrder = 8;

            float angle = Random.Range(0f, Mathf.PI * 2f);
            Vector2 velocity = new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * Random.Range(speed * 0.4f, speed);

            var particle = go.AddComponent<ParticleBurst>();
            particle.Init(velocity, life, color);
        }
    }

    SpriteRenderer sr;
    Vector2 velocity;
    Color baseColor;
    float life;
    float age;

    void Init(Vector2 initialVelocity, float lifeSeconds, Color color)
    {
        sr = GetComponent<SpriteRenderer>();
        velocity = initialVelocity;
        life = lifeSeconds;
        baseColor = color;
    }

    void Update()
    {
        age += Time.deltaTime;
        transform.position += (Vector3)(velocity * Time.deltaTime);
        velocity *= 0.94f;

        float t = Mathf.Clamp01(age / life);
        var c = baseColor;
        c.a = Mathf.Lerp(baseColor.a, 0f, t);
        sr.color = c;

        if (age >= life)
            Destroy(gameObject);
    }
}
