using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(BossCore))]
public class BossMovement : MonoBehaviour
{
    [SerializeField] float baseSpeed = 1.6f;
    [SerializeField] float arenaTopMargin = 1.2f;
    [SerializeField] float arenaSideMargin = 1.5f;
    [SerializeField] float waypointArriveDistance = 0.4f;
    [SerializeField] float rotationSpeedDegPerSec = 12f;

    BossCore core;
    Vector2 target;
    bool hasTarget;
    List<BossPart> engineParts;

    void Awake()
    {
        core = GetComponent<BossCore>();
    }

    void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.CurrentState != GameManager.GameState.Playing)
            return;
        if (core.IsEntering) return;

        if (!hasTarget)
        {
            target = core.RestPosition;
            hasTarget = true;
        }

        RotateTowardsPlayer();

        Vector2 current = transform.position;
        Vector2 toTarget = target - current;

        if (toTarget.magnitude <= waypointArriveDistance)
        {
            PickNewTarget();
            return;
        }

        float speed = baseSpeed * EngineSpeedMultiplier();
        transform.position += (Vector3)(toTarget.normalized * speed * Time.deltaTime);
    }

    void RotateTowardsPlayer()
    {
        if (PlayerController.Instance == null) return;

        Vector2 toPlayer = (Vector2)PlayerController.Instance.transform.position - (Vector2)transform.position;
        if (toPlayer.sqrMagnitude < 0.0001f) return;

        float desiredAngle = Mathf.Atan2(toPlayer.y, toPlayer.x) * Mathf.Rad2Deg - 90f;
        float currentAngle = transform.eulerAngles.z;
        float newAngle = Mathf.MoveTowardsAngle(currentAngle, desiredAngle, rotationSpeedDegPerSec * Time.deltaTime);
        transform.rotation = Quaternion.Euler(0f, 0f, newAngle);
    }

    float EngineSpeedMultiplier()
    {
        if (engineParts == null)
        {
            engineParts = new List<BossPart>();
            foreach (var p in core.Parts)
                if (p.Type == BossPart.PartType.Engine)
                    engineParts.Add(p);
        }

        if (engineParts.Count == 0) return 1f;

        int intact = 0;
        for (int i = 0; i < engineParts.Count; i++)
            if (!engineParts[i].IsDestroyed) intact++;

        float ratio = (float)intact / engineParts.Count;
        return Mathf.Lerp(0.3f, 1f, ratio);
    }

    void PickNewTarget()
    {
        Vector2 half = ArenaBounds.HalfExtents();
        float x = Random.Range(-half.x + arenaSideMargin, half.x - arenaSideMargin);
        float y = Random.Range(0f, half.y - arenaTopMargin);
        target = new Vector2(x, y);
    }
}
