using UnityEngine;

public class PlayerProfile
{
    // Rolling bias signals in roughly [-1, 1], updated by adaptation tracking.
    // Positive FrontBias/BackBias/LeftBias/RightBias means the player deals
    // most of their damage to bosses from that side.
    public float FrontBias;
    public float BackBias;
    public float LeftBias;
    public float RightBias;

    // Positive = player prefers long range, negative = player stays close.
    public float RangePreference;

    // Positive = player prioritizes destroying Engine parts early.
    public float EngineTargetedBias;

    public static PlayerProfile Neutral() => new PlayerProfile();

    public void RegisterHit(Vector2 localOffsetFromCore)
    {
        const float rate = 0.05f;
        if (Mathf.Abs(localOffsetFromCore.y) >= Mathf.Abs(localOffsetFromCore.x))
        {
            if (localOffsetFromCore.y > 0f) FrontBias = Mathf.Clamp(FrontBias + rate, -1f, 1f);
            else BackBias = Mathf.Clamp(BackBias + rate, -1f, 1f);
        }
        else
        {
            if (localOffsetFromCore.x < 0f) LeftBias = Mathf.Clamp(LeftBias + rate, -1f, 1f);
            else RightBias = Mathf.Clamp(RightBias + rate, -1f, 1f);
        }
    }

    public void RegisterRangeSample(float distance)
    {
        const float closeThreshold = 2.5f;
        const float farThreshold = 5f;
        const float rate = 0.01f;

        if (distance > farThreshold) RangePreference = Mathf.Clamp(RangePreference + rate, -1f, 1f);
        else if (distance < closeThreshold) RangePreference = Mathf.Clamp(RangePreference - rate, -1f, 1f);
    }

    public void RegisterEarlyEngineDestruction()
    {
        EngineTargetedBias = Mathf.Clamp(EngineTargetedBias + 0.3f, -1f, 1f);
    }
}
