using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class HUDController : MonoBehaviour
{
    public static HUDController Instance { get; private set; }

    const float timerTweenDuration = 0.5f;
    static readonly Vector2 timerTweenOffscreenOffset = new Vector2(260f, 0f);

    Text stageText;
    Text timerText;
    Text centerMessageText;
    Text bannerText;
    Text respawnText;
    float bannerTimer;

    Vector2 timerRestAnchoredPos;
    bool timerTweening;
    float timerTweenTimer;
    GameManager.GameState previousState = GameManager.GameState.Title;

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

        Font font = Resources.Load<Font>("Fonts/PressStart2P-Regular")
            ?? Font.CreateDynamicFontFromOSFont(new[] { "Consolas", "Courier New", "Courier" }, 24);

        stageText = CreateLabel(canvasGO.transform, "StageText", font, new Vector2(0f, 1f), new Vector2(20f, -20f), TextAnchor.UpperLeft);
        timerText = CreateLabel(canvasGO.transform, "TimerText", font, new Vector2(1f, 1f), new Vector2(-20f, -20f), TextAnchor.UpperRight);
        timerRestAnchoredPos = timerText.rectTransform.anchoredPosition;

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

        bool onTitleScreen = gm.CurrentState == GameManager.GameState.Title;

        if (previousState == GameManager.GameState.Title && gm.CurrentState == GameManager.GameState.Playing)
        {
            timerTweening = true;
            timerTweenTimer = 0f;
        }
        previousState = gm.CurrentState;

        stageText.gameObject.SetActive(!onTitleScreen);
        timerText.gameObject.SetActive(!onTitleScreen);

        if (!onTitleScreen)
        {
            stageText.text = $"STAGE {gm.Stage}";
            UpdateTimer(gm);
            UpdateTimerTween();
        }

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

        float t = Mathf.Max(0f, gm.TimeRemaining);
        int wholeSeconds = Mathf.FloorToInt(t);
        int millis = Mathf.FloorToInt((t - wholeSeconds) * 100f);
        timerText.text = $"T MINUS {wholeSeconds:000}.{millis:00} ";
        timerText.fontSize = urgent ? 44 : 24;
        timerText.color = urgent ? new Color(1f, 0.15f, 0.15f) : new Color(0.7f, 0.95f, 1f);
        timerText.rectTransform.localScale = urgent
            ? Vector3.one * (1f + 0.15f * Mathf.Sin(Time.time * 8f))
            : Vector3.one;
    }

    void UpdateTimerTween()
    {
        if (!timerTweening) return;

        timerTweenTimer += Time.deltaTime;
        float t = Mathf.Clamp01(timerTweenTimer / timerTweenDuration);
        float eased = 1f - Mathf.Pow(1f - t, 3f);
        timerText.rectTransform.anchoredPosition = Vector2.Lerp(timerRestAnchoredPos + timerTweenOffscreenOffset, timerRestAnchoredPos, eased);

        if (t >= 1f) timerTweening = false;
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
        if (gm.CurrentState == GameManager.GameState.Title)
        {
            centerMessageText.text = "SPACE COUNTDOWN\n\nClick or press Space to start\n\nMove: WASD   Aim: Mouse   \n\nFire: Left Click";
            centerMessageText.gameObject.SetActive(true);
        }
        else if (gm.CurrentState == GameManager.GameState.Paused)
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
