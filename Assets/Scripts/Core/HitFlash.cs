using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class HitFlash : MonoBehaviour
{
    SpriteRenderer sr;
    Color originalColor;
    Coroutine running;

    public static void Flash(SpriteRenderer target, float duration = 0.08f)
    {
        if (target == null) return;
        var flash = target.GetComponent<HitFlash>();
        if (flash == null) flash = target.gameObject.AddComponent<HitFlash>();
        flash.DoFlash(duration);
    }

    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
    }

    void DoFlash(float duration)
    {
        if (running != null)
        {
            StopCoroutine(running);
        }
        else
        {
            originalColor = sr.color;
        }
        running = StartCoroutine(FlashRoutine(duration));
    }

    IEnumerator FlashRoutine(float duration)
    {
        sr.color = Color.white;
        yield return new WaitForSeconds(duration);
        sr.color = originalColor;
        running = null;
    }
}
