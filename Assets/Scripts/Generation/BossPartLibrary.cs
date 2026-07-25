using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using UnityEngine;

public class BossPartLibrary : MonoBehaviour
{
    public static BossPartLibrary Instance { get; private set; }

    static readonly Regex NumberPattern = new Regex(@"\((\d+)\)");

    Dictionary<BossPart.PartType, List<GameObject>> templates;
    List<GameObject> bossSequence;

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

        var allBosses = FindObjectsOfType<BossCore>(true).Select(c => c.gameObject).ToList();

        bossSequence = allBosses
            .Where(go => go.name != "Enemy")
            .OrderBy(go => ParseNumber(go.name))
            .ToList();

        foreach (var boss in allBosses)
            boss.SetActive(false);

        ActivateBossForStage(1);
    }

    static int ParseNumber(string name)
    {
        var match = NumberPattern.Match(name);
        return match.Success && int.TryParse(match.Groups[1].Value, out int n) ? n : int.MaxValue;
    }

    public bool ActivateBossForStage(int stage)
    {
        if (bossSequence == null || bossSequence.Count == 0) return false;

        // Clone rather than reactivate: Awake() won't re-run on a toggled instance, so a reused boss would keep its prior (defeated) state.
        int index = (stage - 1) % bossSequence.Count;
        GameObject template = bossSequence[index];

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
