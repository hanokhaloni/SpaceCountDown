using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public enum GameState { Title, Playing, Paused, GameOver }

    public static GameManager Instance { get; private set; }

    [SerializeField] Transform enemySpawnPoint;
    [SerializeField] float startingTime = 30f;
    [SerializeField] float bossDefeatTimeBonus = 20f;
    [SerializeField] AudioClip stageStartSound;
    [SerializeField] AudioClip gameOverSound;

    public GameState CurrentState { get; private set; } = GameState.Title;
    public int Stage { get; private set; } = 1;
    public float TimeRemaining { get; private set; }
    public PlayerProfile Profile { get; private set; } = PlayerProfile.Neutral();

    void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        TimeRemaining = startingTime;

        if (FindObjectOfType<HUDController>() == null)
        {
            var hudGO = new GameObject("HUD");
            hudGO.AddComponent<HUDController>();
            DontDestroyOnLoad(hudGO);
        }

        if (Camera.main != null && Camera.main.GetComponent<CameraShake>() == null)
            Camera.main.gameObject.AddComponent<CameraShake>();
    }

    void Update()
    {
        if (CurrentState == GameState.Title)
        {
            if (Input.GetKeyDown(KeyCode.Space) || Input.GetMouseButtonDown(0))
                StartRun();
            return;
        }

        if (CurrentState == GameState.Playing)
        {
            TimeRemaining -= Time.deltaTime;
            if (TimeRemaining <= 0f)
            {
                TimeRemaining = 0f;
                EndGame();
            }
        }

        if (Input.GetKeyDown(KeyCode.Escape) &&
            (CurrentState == GameState.Playing || CurrentState == GameState.Paused))
        {
            TogglePause();
        }

        if (CurrentState == GameState.GameOver && Input.GetKeyDown(KeyCode.R))
            Restart();
    }

    void Restart()
    {
        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        StartRun();
    }

    public void StartRun()
    {
        Stage = 1;
        TimeRemaining = startingTime;
        Time.timeScale = 1f;
        Profile = PlayerProfile.Neutral();
        CurrentState = GameState.Playing;
    }

    public void NextStage()
    {
        Stage++;
        TimeRemaining += bossDefeatTimeBonus;

        bool activated = BossPartLibrary.Instance != null && BossPartLibrary.Instance.ActivateBossForStage(Stage);
        if (!activated)
        {
            Vector3 spawnPos = enemySpawnPoint != null ? enemySpawnPoint.position : new Vector3(0f, 1.5f, 0f);
            BossGenerator.Generate(Stage, Profile, spawnPos);
        }

        HUDController.Instance?.ShowBanner($"STAGE {Stage} — {BossGenerator.DescribeAdaptation(Profile)}");
        Audio.Play(stageStartSound);
    }

    public void TogglePause()
    {
        CurrentState = CurrentState == GameState.Playing ? GameState.Paused : GameState.Playing;
        Time.timeScale = CurrentState == GameState.Paused ? 0f : 1f;
    }

    void EndGame()
    {
        CurrentState = GameState.GameOver;
        Audio.Play(gameOverSound);
    }
}
