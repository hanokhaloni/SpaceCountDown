using System;
using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public class BossPart : MonoBehaviour
{
    public enum PartType { Core, Armor, Turret, MissileLauncher, RotatingArm, WeakPoint, Engine, ShieldNode }

    [SerializeField] PartType partType = PartType.Armor;
    [SerializeField] int maxHealth = 20;
    [SerializeField] float fireInterval = 1.5f;
    [SerializeField] float bulletSpeed = 5f;
    [SerializeField] Color bulletColor = new Color(1f, 0.3f, 0.3f);
    [SerializeField] float telegraphDuration = 0.35f;
    [SerializeField] float missileTurnRate = 90f;
    [SerializeField] AudioClip shootSound;
    [SerializeField] AudioClip hitSound;
    [SerializeField] AudioClip destroySound;

    int health;
    float fireTimer;
    SpriteRenderer telegraphRing;

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

        var existingRing = transform.Find("TelegraphRing");
        if (existingRing != null)
        {
            telegraphRing = existingRing.GetComponent<SpriteRenderer>();
        }
        else
        {
            var ringGO = new GameObject("TelegraphRing");
            ringGO.transform.SetParent(transform, false);
            ringGO.transform.localScale = Vector3.one * 1.6f;
            telegraphRing = ringGO.AddComponent<SpriteRenderer>();
            telegraphRing.sprite = Shapes.Circle(new Color(1f, 0.9f, 0.2f, 0.55f));
            telegraphRing.sortingOrder = 2;
        }
        telegraphRing.enabled = false;
    }

    void Update()
    {
        if (IsDestroyed) return;
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;
        if (BossCore.Active != null && BossCore.Active.IsEntering) return;

        if (partType == PartType.Turret || partType == PartType.MissileLauncher)
        {
            fireTimer -= Time.deltaTime;

            float effectiveTelegraph = Mathf.Min(telegraphDuration, fireInterval * 0.6f);
            bool telegraphing = fireTimer <= effectiveTelegraph;
            telegraphRing.enabled = telegraphing && Mathf.FloorToInt(Time.time / 0.08f) % 2 == 0;

            if (fireTimer <= 0f)
            {
                fireTimer = fireInterval;
                telegraphRing.enabled = false;
                Fire();
            }
        }
        else if (telegraphRing.enabled)
        {
            telegraphRing.enabled = false;
        }
    }

    void Fire()
    {
        if (PlayerController.Instance == null) return;
        Vector2 dir = (PlayerController.Instance.transform.position - transform.position).normalized;
        bool homing = partType == PartType.MissileLauncher;
        float lifetime = homing ? 2f : 4f;
        Bullet.Spawn(transform.position, dir, bulletSpeed, 0.1f, bulletColor, Bullet.Owner.Boss, lifetimeSeconds: lifetime, homing: homing, turnRateDegPerSec: missileTurnRate);
        Audio.Play(shootSound, 0.5f);
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
            ParticleBurst.Spawn(transform.position, bulletColor, 10);
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
