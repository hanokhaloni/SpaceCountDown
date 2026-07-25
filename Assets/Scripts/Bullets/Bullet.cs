using UnityEngine;
using UnityEngine.Pool;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D), typeof(SpriteRenderer))]
public class Bullet : MonoBehaviour
{
    public enum Owner { Player, Boss }

    static ObjectPool<Bullet> pool;

    static ObjectPool<Bullet> Pool => pool ??= new ObjectPool<Bullet>(
        CreatePooledBullet,
        b => b.gameObject.SetActive(true),
        b => b.gameObject.SetActive(false),
        b => Destroy(b.gameObject),
        false,
        32);

    static Bullet CreatePooledBullet()
    {
        var go = new GameObject("Bullet");

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sortingOrder = 5;

        var rb = go.AddComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        var col = go.AddComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = 0.5f;

        return go.AddComponent<Bullet>();
    }

    public Owner BulletOwner { get; private set; }
    public int Damage { get; private set; } = 1;

    Rigidbody2D rb;
    SpriteRenderer sr;
    Vector2 velocity;
    float lifetime;
    float age;
    bool isHoming;
    bool isDespawned;
    float turnRateDegPerSec;
    float explosionRadius;

    void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        sr = GetComponent<SpriteRenderer>();
    }

    public static Bullet Spawn(Vector3 position, Vector2 direction, float speed, float radius, Color color, Owner owner, int damage = 1, float lifetimeSeconds = 4f, bool homing = false, float turnRateDegPerSec = 90f, float explosionRadius = 0.6f)
    {
        var bullet = Pool.Get();
        bullet.name = owner == Owner.Player ? "PlayerBullet" : "BossBullet";
        bullet.transform.SetPositionAndRotation(position, Quaternion.identity);
        bullet.transform.localScale = Vector3.one * radius * 2f;
        bullet.sr.sprite = Shapes.Circle(color);

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
        age = 0f;
        isDespawned = false;
    }

    void FixedUpdate()
    {
        age += Time.fixedDeltaTime;

        if (isHoming && PlayerController.Instance != null && !PlayerController.Instance.IsDown)
            HomeTowardsPlayer();

        rb.MovePosition(rb.position + velocity * Time.fixedDeltaTime);

        if (isHoming && age >= lifetime)
        {
            Explode();
            return;
        }

        if (age > lifetime || ArenaBounds.IsOutside(rb.position))
            Despawn();
    }

    void HomeTowardsPlayer()
    {
        Vector2 toTarget = (Vector2)PlayerController.Instance.transform.position - rb.position;
        if (toTarget.sqrMagnitude < 0.0001f) return;

        float desiredAngle = Mathf.Atan2(toTarget.y, toTarget.x) * Mathf.Rad2Deg;
        float currentAngle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
        float newAngle = Mathf.MoveTowardsAngle(currentAngle, desiredAngle, turnRateDegPerSec * Time.fixedDeltaTime);

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
        CameraShake.Shake(0.12f, 0.06f);

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
        if (isDespawned) return;
        isDespawned = true;
        Pool.Release(this);
    }
}
