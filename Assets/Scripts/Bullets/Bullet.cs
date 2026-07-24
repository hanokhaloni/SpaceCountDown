using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public class Bullet : MonoBehaviour
{
    public enum Owner { Player, Boss }

    public Owner BulletOwner { get; private set; }
    public int Damage { get; private set; } = 1;

    Vector2 velocity;
    float lifetime;
    float age;
    bool isHoming;
    float turnRateDegPerSec;
    float explosionRadius;

    public static Bullet Spawn(Vector3 position, Vector2 direction, float speed, float radius, Color color, Owner owner, int damage = 1, float lifetimeSeconds = 4f, bool homing = false, float turnRateDegPerSec = 90f, float explosionRadius = 0.6f)
    {
        var go = new GameObject(owner == Owner.Player ? "PlayerBullet" : "BossBullet");
        go.transform.position = position;
        go.transform.localScale = Vector3.one * radius * 2f;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Shapes.Circle(color);
        sr.sortingOrder = 5;

        var bullet = go.AddComponent<Bullet>();

        var rb = go.GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        var col = go.GetComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;

        bullet.Init(direction.normalized * speed, owner, damage, lifetimeSeconds, homing, turnRateDegPerSec, explosionRadius);
        return bullet;
    }

    void Init(Vector2 vel, Owner owner, int damage, float lifetimeSeconds, bool homing, float turnRate, float explosionRad)
    {
        velocity = vel;
        BulletOwner = owner;
        Damage = damage;
        lifetime = lifetimeSeconds;
        isHoming = homing;
        turnRateDegPerSec = turnRate;
        explosionRadius = explosionRad;
    }

    void Update()
    {
        age += Time.deltaTime;

        if (isHoming && PlayerController.Instance != null && !PlayerController.Instance.IsDown)
            HomeTowardsPlayer();

        transform.position += (Vector3)(velocity * Time.deltaTime);

        if (isHoming && age >= lifetime)
        {
            Explode();
            return;
        }

        if (age > lifetime || ArenaBounds.IsOutside(transform.position))
            Despawn();
    }

    void HomeTowardsPlayer()
    {
        Vector2 toTarget = (Vector2)PlayerController.Instance.transform.position - (Vector2)transform.position;
        if (toTarget.sqrMagnitude < 0.0001f) return;

        float desiredAngle = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg;
        float currentAngle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
        float newAngle = Mathf.MoveTowardsAngle(currentAngle, desiredAngle, turnRateDegPerSec * Time.deltaTime);

        float speed = velocity.magnitude;
        velocity = new Vector2(Mathf.Cos(newAngle * Mathf.Deg2Rad), Mathf.Sin(newAngle * Mathf.Deg2Rad)) * speed;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (BulletOwner == Owner.Player)
        {
            var part = other.GetComponent<BossPart>();
            if (part != null)
            {
                part.TakeDamage(Damage);
                Despawn();
            }
        }
        else
        {
            var player = other.GetComponent<PlayerController>();
            if (player != null)
            {
                player.TakeHit();
                if (isHoming) SpawnExplosionEffect(transform.position, explosionRadius);
                Despawn();
            }
        }
    }

    void Explode()
    {
        SpawnExplosionEffect(transform.position, explosionRadius);

        if (BulletOwner == Owner.Boss && PlayerController.Instance != null && !PlayerController.Instance.IsDown)
        {
            float dist = Vector2.Distance(transform.position, PlayerController.Instance.transform.position);
            if (dist <= explosionRadius)
                PlayerController.Instance.TakeHit();
        }

        Despawn();
    }

    static void SpawnExplosionEffect(Vector3 position, float radius)
    {
        var go = new GameObject("MissileExplosion");
        go.transform.position = position;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Shapes.Circle(new Color(1f, 0.55f, 0.15f, 0.85f));
        sr.sortingOrder = 6;

        var fx = go.AddComponent<ExplosionEffect>();
        fx.Init(radius);
    }

    public void Despawn()
    {
        Destroy(gameObject);
    }
}
