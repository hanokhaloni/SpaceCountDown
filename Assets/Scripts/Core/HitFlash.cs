using System.Collections;
using UnityEngine;

[RequireComponent(typeof(SpriteRenderer))]
public class HitFlash : MonoBehaviour
{
    SpriteRenderer sr;
    SpriteRenderer overlay;
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

        var overlayGO = new GameObject("HitFlashOverlay");
        overlayGO.transform.SetParent(transform, false);
        overlay = overlayGO.AddComponent<SpriteRenderer>();
        overlay.color = Color.white;
        overlay.enabled = false;
    }

    void DoFlash(float duration)
    {
        if (running != null) StopCoroutine(running);
        running = StartCoroutine(FlashRoutine(duration));
    }

    IEnumerator FlashRoutine(float duration)
    {
        // Reuses the target's own sprite (whatever shape it is) tinted white, layered on top, instead of swapping in a hardcoded shape.
        overlay.sprite = sr.sprite;
        overlay.sortingOrder = sr.sortingOrder + 1;
        overlay.enabled = true;
        yield return new WaitForSeconds(duration);
        overlay.enabled = false;
        running = null;
    }
}
