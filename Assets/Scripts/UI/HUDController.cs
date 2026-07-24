using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    public static HUDController Instance { get; private set; }

    Text stageText;
    Text timerText;
    Text centerMessageText;
    Text bannerText;
    Text respawnText;
    float bannerTimer;

    void Awake()
    {
        Instance = this;

        var canvasGO = new GameObject("HUDCanvas");
        canvasGO.transform.SetParent(transform, false);

        var canvas = canvasGO.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.ScreenSpaceOverlay;

        var scaler = canvasGO.AddComponent<CanvasScaler>();
        scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution = new Vector2(1280f, 720f);

        canvasGO.AddComponent<GraphicRaycaster>();

        if (FindObjectOfType<EventSystem>() == null)
        {
            var esGO = new GameObject("EventSystem");
            esGO.AddComponent<EventSystem>();
            esGO.AddComponent<StandaloneInputModule>();
        }

        Font font = Resources.GetBuiltinResource<Font>("Arial.ttf");

        stageText = CreateLabel(canvasGO.transform, "StageText", font, new Vector2(0.5f, 1f), new Vector2(0f, -20f), TextAnchor.UpperCenter);
        timerText = CreateLabel(canvasGO.transform, "TimerText", font, new Vector2(1f, 1f), new Vector2(-20f, -20f), TextAnchor.UpperRight);

        centerMessageText = CreateLabel(canvasGO.transform, "CenterMessage", font, new Vector2(0.5f, 0.5f), Vector2.zero, TextAnchor.MiddleCenter);
        centerMessageText.fontSize = 36;
        centerMessageText.rectTransform.sizeDelta = new Vector2(700f, 200f);
        centerMessageText.gameObject.SetActive(false);

        bannerText = CreateLabel(canvasGO.transform, "BannerText", font, new Vector2(0.5f, 1f), new Vector2(0f, -80f), TextAnchor.UpperCenter);
        bannerText.fontSize = 22;
        bannerText.color = new Color(1f, 0.85f, 0.4f);
        bannerText.rectTransform.sizeDelta = new Vector2(900f, 60f);
        bannerText.gameObject.SetActive(false);

        respawnText = CreateLabel(canvasGO.transform, "RespawnText", font, new Vector2(0.5f, 0.5f), new Vector2(0f, -80f), TextAnchor.MiddleCenter);
        respawnText.fontSize = 28;
        respawnText.color = new Color(1f, 0.4f, 0.4f);
        respawnText.rectTransform.sizeDelta = new Vector2(500f, 50f);
        respawnText.gameObject.SetActive(false);
    }

    public void ShowBanner(string message, float duration = 3f)
    {
        bannerText.text = message;
        bannerText.gameObject.SetActive(true);
        bannerTimer = duration;
    }

    Text CreateLabel(Transform parent, string name, Font font, Vector2 anchor, Vector2 anchoredPos, TextAnchor alignment)
    {
        var go = new GameObject(name);
        go.transform.SetParent(parent, false);

        var rect = go.AddComponent<RectTransform>();
        rect.anchorMin = anchor;
        rect.anchorMax = anchor;
        rect.pivot = anchor;
        rect.anchoredPosition = anchoredPos;
        rect.sizeDelta = new Vector2(400f, 40f);

        var text = go.AddComponent<Text>();
        text.font = font;
        text.fontSize = 24;
        text.color = new Color(0.7f, 0.95f, 1f);
        text.alignment = alignment;
        text.text = string.Empty;
        text.horizontalOverflow = HorizontalWrapMode.Overflow;
        text.verticalOverflow = VerticalWrapMode.Overflow;
        return text;
    }

    void Update()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;

        stageText.text = $"STAGE {gm.Stage}";
        UpdateTimer(gm);

        UpdateCenterMessage(gm);
        UpdateRespawnMessage();

        if (bannerTimer > 0f)
        {
            bannerTimer -= Time.deltaTime;
            if (bannerTimer <= 0f)
                bannerText.gameObject.SetActive(false);
        }
    }

    void UpdateTimer(GameManager gm)
    {
        bool urgent = gm.TimeRemaining <= 10f;

        timerText.text = $"TIME {Mathf.CeilToInt(gm.TimeRemaining)}";
        timerText.fontSize = urgent ? 44 : 24;
        timerText.color = urgent ? new Color(1f, 0.15f, 0.15f) : new Color(0.7f, 0.95f, 1f);
        timerText.rectTransform.localScale = urgent
            ? Vector3.one * (1f + 0.15f * Mathf.Sin(Time.time * 8f))
            : Vector3.one;
    }

    void UpdateRespawnMessage()
    {
        if (PlayerController.Instance != null && PlayerController.Instance.IsDown)
        {
            respawnText.text = $"RESPAWNING IN {Mathf.CeilToInt(PlayerController.Instance.RespawnCountdown)}";
            respawnText.gameObject.SetActive(true);
        }
        else
        {
            respawnText.gameObject.SetActive(false);
        }
    }

    void UpdateCenterMessage(GameManager gm)
    {
        if (gm.CurrentState == GameManager.GameState.Paused)
        {
            centerMessageText.text = "PAUSED\n(Esc to resume)";
            centerMessageText.gameObject.SetActive(true);
        }
        else if (gm.CurrentState == GameManager.GameState.GameOver)
        {
            centerMessageText.text = $"GAME OVER\nReached Stage {gm.Stage}\n(R to restart)";
            centerMessageText.gameObject.SetActive(true);
        }
        else
        {
            centerMessageText.gameObject.SetActive(false);
        }
    }

}
