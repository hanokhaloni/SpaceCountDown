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
    [SerializeField] AudioClip stressSound;
    [SerializeField] AudioClip[] playerBulletSounds;
    [SerializeField] AudioClip[] enemyBulletSounds;
    [SerializeField] AudioClip[] enemyMissileSounds;

    [Header("Volume")]
    [SerializeField, Range(0f, 1f)] float musicVolume = 1f;
    [SerializeField, Range(0f, 1f)] float sfxVolume = 1f;
    [SerializeField, Range(0f, 1f)] float stressVolume = 1f;

    [Header("Background Grid")]
    [SerializeField] Color gridBackgroundColor = Color.black;
    [SerializeField] Color gridLineColor = new Color(0.1f, 1f, 0.3f);
    [SerializeField] float gridCellSize = 1f;
    [SerializeField, Range(0.01f, 0.5f)] float gridLineThickness = 0.05f;
    [SerializeField] float gridScrollSpeed = 0.15f;
    [SerializeField] float gridRotationSpeedDegPerSec = 1f;

    const float stressThreshold = 10f;

    public GameState CurrentState { get; private set; } = GameState.Title;
    public int Stage { get; private set; } = 1;
    public float TimeRemaining { get; private set; }
    public PlayerProfile Profile { get; private set; } = PlayerProfile.Neutral();

    AudioSource stageAudioSource;
    AudioSource stressAudioSource;

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

        stageAudioSource = gameObject.AddComponent<AudioSource>();
        stageAudioSource.playOnAwake = false;
        stageAudioSource.spatialBlend = 0f;

        stressAudioSource = gameObject.AddComponent<AudioSource>();
        stressAudioSource.playOnAwake = false;
        stressAudioSource.spatialBlend = 0f;
        stressAudioSource.loop = true;
        stressAudioSource.clip = stressSound;

        ApplyVolumeSettings();

        if (FindObjectOfType<HUDController>() == null)
        {
            var hudGO = new GameObject("HUD");
            hudGO.AddComponent<HUDController>();
            DontDestroyOnLoad(hudGO);
        }

        if (Camera.main != null && Camera.main.GetComponent<CameraShake>() == null)
            Camera.main.gameObject.AddComponent<CameraShake>();

        if (FindObjectOfType<BackgroundGrid>() == null)
        {
            var gridGO = new GameObject("BackgroundGrid");
            var grid = gridGO.AddComponent<BackgroundGrid>();
            grid.Init(gridBackgroundColor, gridLineColor, gridCellSize, gridLineThickness, gridScrollSpeed, gridRotationSpeedDegPerSec);
            DontDestroyOnLoad(gridGO);
        }
    }

    void Update()
    {
        ApplyVolumeSettings();

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

        UpdateStressSound();

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
        PlayStageStartSound();
        PlayerController.Instance?.ResetForNewRun();
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

        HUDController.Instance?.ShowBanner($"STAGE {Stage}\n{BossGenerator.DescribeAdaptation(Profile)}");
        PlayStageStartSound();
    }

    void PlayStageStartSound()
    {
        if (stageStartSound == null) return;
        stageAudioSource.Stop();
        stageAudioSource.clip = stageStartSound;
        stageAudioSource.loop = true;
        stageAudioSource.volume = musicVolume;
        stageAudioSource.Play();
    }

    void UpdateStressSound()
    {
        bool shouldPlay = CurrentState == GameState.Playing && TimeRemaining > 0f && TimeRemaining <= stressThreshold && stressSound != null;

        if (shouldPlay && !stressAudioSource.isPlaying)
            stressAudioSource.Play();
        else if (!shouldPlay && stressAudioSource.isPlaying)
            stressAudioSource.Stop();
    }

    void ApplyVolumeSettings()
    {
        Audio.MusicVolume = musicVolume;
        Audio.SfxVolume = sfxVolume;
        if (stageAudioSource != null) stageAudioSource.volume = musicVolume;
        if (stressAudioSource != null) stressAudioSource.volume = stressVolume;
    }

    void OnValidate() => ApplyVolumeSettings();

    public void PlayPlayerBulletSound(float volume = 1f) => PlayRandomClip(playerBulletSounds, volume);
    public void PlayEnemyBulletSound(float volume = 1f) => PlayRandomClip(enemyBulletSounds, volume);
    public void PlayEnemyMissileSound(float volume = 1f) => PlayRandomClip(enemyMissileSounds, volume);

    static void PlayRandomClip(AudioClip[] clips, float volume)
    {
        if (clips == null || clips.Length == 0) return;
        Audio.Play(clips[Random.Range(0, clips.Length)], volume);
    }

    public void TogglePause()
    {
        CurrentState = CurrentState == GameState.Playing ? GameState.Paused : GameState.Playing;
        Time.timeScale = CurrentState == GameState.Paused ? 0f : 1f;
    }

    void EndGame()
    {
        CurrentState = GameState.GameOver;
        stageAudioSource.Stop();
        stressAudioSource.Stop();
        Audio.PlayMusic(gameOverSound);
        PlayerController.Instance?.Hide();
    }
}
