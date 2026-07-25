using UnityEngine;

public class CameraShake : MonoBehaviour
{
    public static CameraShake Instance { get; private set; }

    Vector3 basePosition;
    float duration;
    float magnitude;

    void Awake()
    {
        Instance = this;
        basePosition = transform.localPosition;
    }

    void OnDestroy()
    {
        if (Instance == this) Instance = null;
    }

    public static void Shake(float shakeDuration, float shakeMagnitude)
    {
        // Unity's fake-null equality means this correctly re-fetches after a scene reload destroys the old camera (see ArenaBounds).
        if (Instance == null && Camera.main != null)
            Instance = Camera.main.GetComponent<CameraShake>() ?? Camera.main.gameObject.AddComponent<CameraShake>();

        if (Instance != null) Instance.BeginShake(shakeDuration, shakeMagnitude);
    }

    void BeginShake(float shakeDuration, float shakeMagnitude)
    {
        // Re-triggering keeps the larger/longer of the two so overlapping hits don't cut a bigger shake short.
        duration = Mathf.Max(duration, shakeDuration);
        magnitude = Mathf.Max(magnitude, shakeMagnitude);
    }

    void LateUpdate()
    {
        if (duration > 0f)
        {
            duration -= Time.unscaledDeltaTime;
            float falloff = Mathf.Clamp01(duration * 6f);
            Vector2 offset = Random.insideUnitCircle * magnitude * falloff;
            transform.localPosition = basePosition + (Vector3)offset;
        }
        else
        {
            duration = 0f;
            magnitude = 0f;
            transform.localPosition = basePosition;
        }
    }
}
