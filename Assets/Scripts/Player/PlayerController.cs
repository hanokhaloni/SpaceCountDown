using UnityEngine;

[RequireComponent(typeof(Rigidbody2D), typeof(CircleCollider2D))]
public class PlayerController : MonoBehaviour
{
    public static PlayerController Instance { get; private set; }

    [SerializeField] float moveSpeed = 10f;
    [SerializeField] float fireRate = 8f;
    [SerializeField] float bulletSpeed = 14f;
    [SerializeField] float bulletRadius = 0.08f;
    [SerializeField] Color bulletColor = new Color(0.3f, 0.9f, 1f);
    [SerializeField] float invulnDuration = 1.5f;
    [SerializeField] float hitboxRadius = 0.2f;
    [SerializeField] float arenaMargin = 0.3f;
    [SerializeField] float respawnDelay = 10f;

    Rigidbody2D rb;
    CircleCollider2D col;
    SpriteRenderer visual;
    Camera cam;
    Vector3 spawnPosition;
    Vector2 moveInput;
    bool canMove;
    float fireCooldown;
    float invulnTimer;
    float respawnTimer;

    public bool IsInvulnerable => invulnTimer > 0f;
    public bool IsDown { get; private set; }
    public float RespawnCountdown => Mathf.Max(0f, respawnTimer);

    void Awake()
    {
        Instance = this;
        rb = GetComponent<Rigidbody2D>();
        rb.bodyType = RigidbodyType2D.Kinematic;
        rb.gravityScale = 0f;

        col = GetComponent<CircleCollider2D>();
        col.isTrigger = true;
        col.radius = hitboxRadius;

        visual = GetComponentInChildren<SpriteRenderer>();
        spawnPosition = transform.position;
        cam = Camera.main;
    }

    void Update()
    {
        bool isPlaying = GameManager.Instance == null || GameManager.Instance.CurrentState == GameManager.GameState.Playing;

        if (IsDown)
        {
            canMove = false;
            if (isPlaying)
            {
                respawnTimer -= Time.deltaTime;
                if (respawnTimer <= 0f)
                    Respawn();
            }
            return;
        }

        canMove = isPlaying;
        if (!isPlaying) return;

        ReadMoveInput();
        HandleAim();
        HandleFire();
        HandleRangeTracking();

        if (invulnTimer > 0f)
            invulnTimer -= Time.deltaTime;
    }

    void FixedUpdate()
    {
        if (!canMove) return;

        Vector3 next = transform.position + (Vector3)(moveInput * moveSpeed * Time.fixedDeltaTime);
        next = ArenaBounds.Clamp(next, arenaMargin);
        rb.MovePosition(next);
    }

    void HandleRangeTracking()
    {
        if (BossCore.Active == null || GameManager.Instance == null) return;
        float distance = Vector2.Distance(transform.position, BossCore.Active.transform.position);
        GameManager.Instance.Profile.RegisterRangeSample(distance);
    }

    void ReadMoveInput()
    {
        float h = Input.GetAxisRaw("Horizontal");
        float v = Input.GetAxisRaw("Vertical");
        moveInput = new Vector2(h, v);
        if (moveInput.sqrMagnitude > 1f) moveInput.Normalize();
    }

    void HandleAim()
    {
        if (cam == null) return;
        Vector3 mouseWorld = cam.ScreenToWorldPoint(Input.mousePosition);
        mouseWorld.z = transform.position.z;
        Vector2 aimDir = mouseWorld - transform.position;
        if (aimDir.sqrMagnitude < 0.0001f) return;

        float angle = Mathf.Atan2(aimDir.y, aimDir.x) * Mathf.Rad2Deg - 90f;
        transform.rotation = Quaternion.Euler(0f, 0f, angle);
    }

    void HandleFire()
    {
        fireCooldown -= Time.deltaTime;
        if (Input.GetButton("Fire1") && fireCooldown <= 0f)
        {
            fireCooldown = 1f / fireRate;
            Bullet.Spawn(transform.position, transform.up, bulletSpeed, bulletRadius, bulletColor, Bullet.Owner.Player);
        }
    }

    public void TakeHit()
    {
        if (invulnTimer > 0f || IsDown) return;
        Die();
    }

    void Die()
    {
        IsDown = true;
        respawnTimer = respawnDelay;
        SetVisible(false);
    }

    void Respawn()
    {
        IsDown = false;
        transform.position = spawnPosition;
        transform.rotation = Quaternion.identity;
        SetVisible(true);
        invulnTimer = invulnDuration;
    }

    void SetVisible(bool isVisible)
    {
        if (visual != null) visual.enabled = isVisible;
        if (col != null) col.enabled = isVisible;
    }
}
