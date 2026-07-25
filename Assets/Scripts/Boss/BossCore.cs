using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(BossMovement))]
public class BossCore : MonoBehaviour
{
    [SerializeField] float introDuration = 0.9f;
    [SerializeField] float introTopMargin = 1.5f;
    [SerializeField] Color warpFlashColor = new Color(0.4f, 0.9f, 1f, 0.9f);
    [SerializeField] AudioClip warpSound;
    [SerializeField] AudioClip defeatSound;

    public static BossCore Active { get; private set; }
    public bool IsEntering { get; private set; }
    public Vector3 RestPosition => restPosition;

    List<BossPart> parts;
    BossPart corePart;
    Vector3 entryStartPosition;
    Vector3 restPosition;
    float introTimer;

    void Awake()
    {
        Active = this;

        restPosition = transform.position;
        entryStartPosition = restPosition;
        entryStartPosition.y = ArenaBounds.HalfExtents().y + introTopMargin;
        transform.position = entryStartPosition;
        IsEntering = true;

        SpawnHyperspaceStreak(entryStartPosition);
        Audio.Play(warpSound);

        parts = GetComponentsInChildren<BossPart>(true).ToList();
        corePart = parts.FirstOrDefault(p => p.Type == BossPart.PartType.Core);

        foreach (var part in parts)
            part.Destroyed += OnPartDestroyed;
    }

    void Update()
    {
        if (!IsEntering) return;
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;

        introTimer += Time.deltaTime;
        float t = Mathf.Clamp01(introTimer / introDuration);
        float inverse = 1f - t;
        float eased = 1f - inverse * inverse * inverse;
        transform.position = Vector3.Lerp(entryStartPosition, restPosition, eased);

        if (t >= 1f)
        {
            transform.position = restPosition;
            IsEntering = false;
            SpawnWarpFlash(restPosition, 0.9f);
        }
    }

    void SpawnWarpFlash(Vector3 position, float radius)
    {
        var go = new GameObject("WarpFlash");
        go.transform.position = position;

        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Shapes.Circle(warpFlashColor);
        sr.sortingOrder = 7;

        var fx = go.AddComponent<ExplosionEffect>();
        fx.Init(radius);
    }

    void SpawnHyperspaceStreak(Vector3 anchorBottom)
    {
        var go = new GameObject("HyperspaceStreak");
        var sr = go.AddComponent<SpriteRenderer>();
        sr.sprite = Shapes.Circle(Color.white);
        sr.sortingOrder = 7;

        var streak = go.AddComponent<HyperspaceStreak>();
        streak.Init(anchorBottom, 5f, 0.22f, warpFlashColor);
    }

    void OnDestroy()
    {
        if (Active == this) Active = null;
    }

    void OnPartDestroyed(BossPart part)
    {
        if (part.Type == BossPart.PartType.Engine)
        {
            int destroyedSoFar = parts.Count(p => p.IsDestroyed);
            if (destroyedSoFar <= Mathf.Max(1, parts.Count / 3))
                GameManager.Instance?.Profile.RegisterEarlyEngineDestruction();
        }

        bool bossDefeated = corePart != null
            ? part == corePart
            : parts.All(p => p.IsDestroyed);

        if (bossDefeated)
            Defeat();
    }

    void Defeat()
    {
        Audio.Play(defeatSound);
        CameraShake.Shake(0.5f, 0.25f);
        ParticleBurst.Spawn(restPosition, warpFlashColor, 24);

        ScatterSurvivingParts();
        GameManager.Instance?.NextStage();
        if (Active == this) Active = null;
        Destroy(gameObject);
    }

    void ScatterSurvivingParts()
    {
        foreach (var part in parts)
        {
            if (part == null || part == corePart || part.IsDestroyed) continue;

            part.enabled = false;

            var col = part.GetComponent<Collider2D>();
            if (col != null) col.enabled = false;

            Vector2 dir = (Vector2)part.transform.position - (Vector2)transform.position;
            if (dir.sqrMagnitude < 0.0001f) dir = Random.insideUnitCircle;
            dir.Normalize();

            part.transform.SetParent(null, true);

            var drift = part.gameObject.AddComponent<PartDeathDrift>();
            float speed = Random.Range(1.5f, 3f);
            float spin = Random.Range(-180f, 180f);
            drift.Init(dir, speed, spin, 1.5f);
        }
    }

    public IReadOnlyList<BossPart> Parts => parts;
}
