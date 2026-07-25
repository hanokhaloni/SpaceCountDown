using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public class BossPart : MonoBehaviour
{
    public enum PartType { Core, Armor, Turret, MissileLauncher, RotatingArm, WeakPoint, Engine, ShieldNode, SpreadTurret }

    [SerializeField] PartType partType = PartType.Armor;
    [SerializeField] int maxHealth = 20;
    [SerializeField] float fireInterval = 1.5f;
    [SerializeField] float bulletSpeed = 5f;
    [SerializeField] Color bulletColor = new Color(1f, 0.3f, 0.3f);
    [SerializeField] float missileTurnRate = 90f;
    [SerializeField] int spreadBulletCount = 5;
    [SerializeField] float spreadAngleDegrees = 45f;
    [SerializeField] AudioClip hitSound;
    [SerializeField] AudioClip destroySound;

    int health;
    float fireTimer;

    public event Action<BossPart> Destroyed;
    public PartType Type => partType;
    public bool IsDestroyed { get; private set; }
    public int Health => health;
    public int MaxHealth => maxHealth;

    void Awake()
    {
        health = maxHealth;

        var rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        var col = GetComponent<CircleCollider2D>();
        col.isTrigger = true;
        if (col.radius <= 0f) col.radius = 0.5f;

        fireTimer = UnityEngine.Random.Range(0f, fireInterval);
    }

    void Update()
    {
        if (IsDestroyed) return;
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;
        if (BossCore.Active != null && BossCore.Active.IsEntering) return;

        if (partType == PartType.Turret || partType == PartType.MissileLauncher || partType == PartType.SpreadTurret)
        {
            fireTimer -= Time.deltaTime;

            if (fireTimer <= 0f)
            {
                fireTimer = fireInterval;
                Fire();
            }
        }
    }

    void Fire()
    {
        if (PlayerController.Instance == null) return;
        Vector2 dir = (PlayerController.Instance.transform.position - transform.position).normalized;

        if (partType == PartType.SpreadTurret)
            FireSpread(dir);
        else
            FireSingle(dir);

        if (partType == PartType.MissileLauncher) GameManager.Instance?.PlayEnemyMissileSound(0.5f);
        else GameManager.Instance?.PlayEnemyBulletSound(0.5f);
    }

    void FireSingle(Vector2 dir)
    {
        bool homing = partType == PartType.MissileLauncher;
        float lifetime = homing ? 2f : 4f;
        Bullet.Spawn(transform.position, dir, bulletSpeed, 0.1f, bulletColor, Bullet.Owner.Boss, lifetimeSeconds: lifetime, homing: homing, turnRateDegPerSec: missileTurnRate);
        MuzzleSmoke.Spawn(transform.position, dir);
    }

    void FireSpread(Vector2 aimDir)
    {
        int count = Mathf.Max(1, spreadBulletCount);
        float baseAngle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg;
        float startAngle = count > 1 ? baseAngle - spreadAngleDegrees / 2f : baseAngle;
        float step = count > 1 ? spreadAngleDegrees / (count - 1) : 0f;

        for (int i = 0; i < count; i++)
        {
            float angle = startAngle + step * i;
            Vector2 dir = new Vector2(Mathf.Cos(angle * Mathf.Deg2Rad), Mathf.Sin(angle * Mathf.Deg2Rad));
            Bullet.Spawn(transform.position, dir, bulletSpeed, 0.1f, bulletColor, Bullet.Owner.Boss, lifetimeSeconds: 4f);
            MuzzleSmoke.Spawn(transform.position, dir);
        }
    }

    public void Configure(PartType type, int newMaxHealth, float newFireInterval, Color newBulletColor)
    {
        partType = type;
        maxHealth = Mathf.Max(1, newMaxHealth);
        fireInterval = Mathf.Max(0.2f, newFireInterval);
        bulletColor = newBulletColor;
        health = maxHealth;
        IsDestroyed = false;
    }

    public void TakeDamage(int amount)
    {
        if (IsDestroyed) return;
        health -= amount;

        if (partType != PartType.Core)
            GameManager.Instance?.Profile.RegisterHit(transform.localPosition);

        if (health <= 0)
        {
            IsDestroyed = true;
            Audio.Play(destroySound);
            CameraShake.Shake(0.15f, 0.08f);
            ParticleBurst.Spawn(transform.position, bulletColor, 20, speed: 5f, life: 0.5f);
            Destroyed?.Invoke(this);
            gameObject.SetActive(false);
        }
        else
        {
            Audio.Play(hitSound, 0.5f);
            HitFlash.Flash(GetComponent<SpriteRenderer>());
        }
    }
}
