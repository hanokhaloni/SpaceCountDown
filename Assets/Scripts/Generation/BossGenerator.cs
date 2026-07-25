using System.Collections.Generic;
using UnityEngine;

public static class BossGenerator
{
    enum Slot { Front, Back, Left, Right, Orbit }

    static readonly BossPart.PartType[] AttackTypes =
    {
        BossPart.PartType.Turret,
        BossPart.PartType.MissileLauncher,
        BossPart.PartType.RotatingArm,
        BossPart.PartType.SpreadTurret,
    };

    static readonly BossPart.PartType[] DefenseTypes =
    {
        BossPart.PartType.Armor,
        BossPart.PartType.ShieldNode,
    };

    public static GameObject Generate(int stage, PlayerProfile profile, Vector3 position)
    {
        var rng = new System.Random(stage * 7919 + 13);

        var root = new GameObject($"Boss_Stage{stage}");
        root.transform.position = position;

        SpawnPart(root.transform, BossPart.PartType.Core, Vector2.zero,
            BaseHealth(stage, 40f), 0f, new Color(1f, 0.15f, 0.15f), 0.5f, rng);

        int extraPartCount = Mathf.Clamp(2 + stage / 2, 2, 8);
        var slotWeights = BuildSlotWeights(profile);

        for (int i = 0; i < extraPartCount; i++)
        {
            Slot slot = PickSlot(rng, slotWeights);
            Vector2 offset = SlotOffset(slot, rng);
            BossPart.PartType type = PickPartType(rng, slot, profile);

            float health = BaseHealth(stage, 15f) * (type == BossPart.PartType.Armor ? 1.5f : 1f);
            if (type == BossPart.PartType.Engine && profile.EngineTargetedBias > 0.3f)
                health *= 1.8f;

            float fireInterval = Mathf.Max(0.5f, 1.6f - stage * 0.05f);
            Color color = ColorForType(type);

            SpawnPart(root.transform, type, offset, health, fireInterval, color, 0.35f, rng);
        }

        root.AddComponent<BossCore>();
        return root;
    }

    public static string DescribeAdaptation(PlayerProfile profile)
    {
        var notes = new List<string>();
        if (profile.FrontBias > 0.3f) notes.Add("reinforced FRONT");
        if (profile.BackBias > 0.3f) notes.Add("reinforced BACK");
        if (profile.LeftBias > 0.3f) notes.Add("reinforced LEFT");
        if (profile.RightBias > 0.3f) notes.Add("reinforced RIGHT");
        if (profile.RangePreference > 0.3f) notes.Add("added long-range weapons");
        if (profile.RangePreference < -0.3f) notes.Add("added close-defense weapons");
        if (profile.EngineTargetedBias > 0.3f) notes.Add("shielded its engines");

        return notes.Count == 0 ? "Boss assembled." : "Boss adapted: " + string.Join(", ", notes) + ".";
    }

    static float BaseHealth(int stage, float baseValue) => baseValue + stage * 6f;

    static BossPart SpawnPart(Transform parent, BossPart.PartType type, Vector2 offset, float health, float fireInterval, Color fallbackColor, float radius, System.Random rng)
    {
        GameObject template = BossPartLibrary.Instance != null ? BossPartLibrary.Instance.GetTemplate(type, rng) : null;
        GameObject go;
        Color projectileColor = fallbackColor;

        if (template != null)
        {
            go = Object.Instantiate(template, parent);
            go.SetActive(true);
            go.name = template.name;

            var templateSr = template.GetComponent<SpriteRenderer>();
            if (templateSr != null) projectileColor = templateSr.color;
        }
        else
        {
            go = new GameObject(type.ToString());
            go.transform.SetParent(parent, false);

            var sr = go.AddComponent<SpriteRenderer>();
            sr.sprite = Shapes.Circle(fallbackColor);
            sr.sortingOrder = 3;
        }

        go.transform.localPosition = offset;
        go.transform.localRotation = Quaternion.identity;
        go.transform.localScale = Vector3.one * radius * 2f;

        var part = go.GetComponent<BossPart>();
        if (part == null) part = go.AddComponent<BossPart>();
        part.Configure(type, Mathf.RoundToInt(health), fireInterval, projectileColor);
        return part;
    }

    static Dictionary<Slot, float> BuildSlotWeights(PlayerProfile profile)
    {
        return new Dictionary<Slot, float>
        {
            { Slot.Front, 1f + Mathf.Max(0f, profile.FrontBias) * 2f },
            { Slot.Back, 1f + Mathf.Max(0f, profile.BackBias) * 2f },
            { Slot.Left, 1f + Mathf.Max(0f, profile.LeftBias) * 2f },
            { Slot.Right, 1f + Mathf.Max(0f, profile.RightBias) * 2f },
            { Slot.Orbit, 1f },
        };
    }

    static Slot PickSlot(System.Random rng, Dictionary<Slot, float> weights)
    {
        float total = 0f;
        foreach (var v in weights.Values) total += v;

        float roll = (float)rng.NextDouble() * total;
        float cumulative = 0f;
        foreach (var kv in weights)
        {
            cumulative += kv.Value;
            if (roll <= cumulative) return kv.Key;
        }
        return Slot.Orbit;
    }

    static Vector2 SlotOffset(Slot slot, System.Random rng)
    {
        float dist = 0.9f + (float)rng.NextDouble() * 0.3f;
        switch (slot)
        {
            case Slot.Front: return new Vector2(0f, dist);
            case Slot.Back: return new Vector2(0f, -dist);
            case Slot.Left: return new Vector2(-dist, 0f);
            case Slot.Right: return new Vector2(dist, 0f);
            default:
                float angle = (float)(rng.NextDouble() * Mathf.PI * 2f);
                float orbitDist = dist + 0.6f;
                return new Vector2(Mathf.Cos(angle) * orbitDist, Mathf.Sin(angle) * orbitDist);
        }
    }

    static BossPart.PartType PickPartType(System.Random rng, Slot slot, PlayerProfile profile)
    {
        if (slot == Slot.Back && rng.NextDouble() < 0.25)
            return BossPart.PartType.Engine;

        if (profile.RangePreference > 0.3f && rng.NextDouble() < 0.5)
            return BossPart.PartType.MissileLauncher;
        if (profile.RangePreference < -0.3f && rng.NextDouble() < 0.5)
            return BossPart.PartType.RotatingArm;

        bool sideIsFavored =
            (slot == Slot.Front && profile.FrontBias > 0.3f) ||
            (slot == Slot.Back && profile.BackBias > 0.3f) ||
            (slot == Slot.Left && profile.LeftBias > 0.3f) ||
            (slot == Slot.Right && profile.RightBias > 0.3f);

        bool defensive = sideIsFavored && rng.NextDouble() < 0.7;
        var pool = defensive ? DefenseTypes : AttackTypes;
        return pool[rng.Next(pool.Length)];
    }

    static Color ColorForType(BossPart.PartType type)
    {
        switch (type)
        {
            case BossPart.PartType.Turret: return new Color(1f, 0.4f, 0.2f);
            case BossPart.PartType.MissileLauncher: return new Color(1f, 0.65f, 0.15f);
            case BossPart.PartType.RotatingArm: return new Color(1f, 0.85f, 0.2f);
            case BossPart.PartType.SpreadTurret: return new Color(1f, 0.5f, 0.6f);
            case BossPart.PartType.Armor: return new Color(0.4f, 0.4f, 0.5f);
            case BossPart.PartType.ShieldNode: return new Color(0.3f, 0.7f, 1f);
            case BossPart.PartType.Engine: return new Color(0.6f, 0.3f, 1f);
            default: return new Color(0.9f, 0.9f, 0.9f);
        }
    }
}
