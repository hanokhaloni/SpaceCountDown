using System.Collections.Generic;
using UnityEngine;

public class BossPartLibrary : MonoBehaviour
{
    public static BossPartLibrary Instance { get; private set; }

    [SerializeField] List<GameObject> bossSequence = new List<GameObject>();

    Dictionary<BossPart.PartType, List<GameObject>> templates;

    void Awake()
    {
        Instance = this;
        templates = new Dictionary<BossPart.PartType, List<GameObject>>();

        foreach (var part in FindObjectsOfType<BossPart>(true))
        {
            if (!templates.TryGetValue(part.Type, out var list))
            {
                list = new List<GameObject>();
                templates[part.Type] = list;
            }
            list.Add(part.gameObject);
        }

        foreach (var boss in bossSequence)
            if (boss != null) boss.SetActive(false);

        ActivateBossForStage(1);
    }

    public bool ActivateBossForStage(int stage)
    {
        if (bossSequence == null || bossSequence.Count == 0) return false;

        // Clone rather than reactivate: Awake() won't re-run on a toggled instance, so a reused boss would keep its prior (defeated) state.
        int index = (stage - 1) % bossSequence.Count;
        GameObject template = bossSequence[index];
        if (template == null) return false;

        GameObject instance = Object.Instantiate(template);
        instance.name = template.name;
        instance.SetActive(true);
        return true;
    }

    public GameObject GetTemplate(BossPart.PartType type, System.Random rng)
    {
        if (templates != null && templates.TryGetValue(type, out var list) && list.Count > 0)
            return list[rng.Next(list.Count)];
        return null;
    }
}
