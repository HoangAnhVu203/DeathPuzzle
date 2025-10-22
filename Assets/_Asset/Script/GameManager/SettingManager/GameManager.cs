using System;
using UnityEngine;
using UnityEngine.Events;

public enum GameState
{
    Boot,
    MainMenu,
    Loading,
    Playing,
    Paused,
    Victory,
}

[DefaultExecutionOrder(-200)]
public class GameManager : MonoBehaviour
{
    // ===== Singleton =====
    public static GameManager Instance { get; private set; }

    [Header("External Managers (optional)")]
    public LVManager levelManager;        // Script bạn đã có để quản lý panel-level
    public GameObject pausePanel;         // Panel Pause (ẩn/hiện theo state)
    public GameObject hudPanel;           // HUD gameplay (ẩn khi paused/victory/defeat)
    public GameObject mainMenuPanel;      // Menu chính

    [Header("State")]
    public GameState initialState = GameState.MainMenu;
    public GameState CurrentState { get; private set; } = GameState.Boot;
    public GameState PreviousState { get; private set; } = GameState.Boot;

    [Header("Events (On Enter)")]
    public UnityEvent onEnterMainMenu;
    public UnityEvent onEnterPlaying;
    public UnityEvent onEnterPaused;
    public UnityEvent onEnterVictory;
    public UnityEvent onEnterDefeat;
    public UnityEvent onEnterCutscene;
    public UnityEvent onEnterLoading;

    // Sự kiện tổng quát (dễ debug/log)
    [Serializable] public class StateChangedEvent : UnityEvent<GameState, GameState> { }
    public StateChangedEvent onStateChanged;

    void Awake()
    {
        // Singleton
        if (Instance && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        // Thiết lập UI nền
        SafeSetActive(pausePanel, false);

        // Vào trạng thái khởi tạo
        SetState(initialState, force: true);
    }

    // ====== Public API ======
    public void SetState(GameState newState, bool force = false)
    {
        if (!force && newState == CurrentState) return;

        var from = CurrentState;
        PreviousState = from;
        CurrentState = newState;

        // Điều khiển time scale / UI chung
        switch (newState)
        {
            case GameState.MainMenu:
                Time.timeScale = 1f;
                SafeSetActive(mainMenuPanel, true);
                SafeSetActive(hudPanel, false);
                SafeSetActive(pausePanel, false);
                onEnterMainMenu?.Invoke();
                break;

            case GameState.Playing:
                Time.timeScale = 1f;
                SafeSetActive(mainMenuPanel, false);
                SafeSetActive(hudPanel, true);
                SafeSetActive(pausePanel, false);
                onEnterPlaying?.Invoke();
                break;

            case GameState.Paused:
                Time.timeScale = 0f;
                SafeSetActive(pausePanel, true);
                SafeSetActive(hudPanel, false);
                onEnterPaused?.Invoke();
                break;

            case GameState.Victory:
                Time.timeScale = 1f;
                SafeSetActive(pausePanel, false);
                SafeSetActive(hudPanel, false);
                onEnterVictory?.Invoke();
                break;


            case GameState.Loading:
                Time.timeScale = 1f;
                onEnterLoading?.Invoke();
                break;
        }

        onStateChanged?.Invoke(from, newState);
        // Debug.Log($"[GameManager] {from} -> {newState}");
    }

    public void StartGame()
    {
        // Ẩn menu, bật level đầu (LevelManager đã tự bật panel đầu trong Awake)
        SetState(GameState.Playing);
    }

    public void TogglePause()
    {
        if (CurrentState == GameState.Paused) Resume();
        else if (CurrentState == GameState.Playing) Pause();
    }

    public void Pause() => SetState(GameState.Paused);
    public void Resume() => SetState(GameState.Playing);

    public void Win()
    {
        // Hiện panel win ở nơi bạn gắn sự kiện onEnterVictory (hoặc gọi trực tiếp ở đây)
        SetState(GameState.Victory);
    }


    public void NextLevel()
    {
        if (levelManager) levelManager.Next();
        SetState(GameState.Playing);
    }

    public void ReloadLevel()
    {
        if (levelManager) levelManager.Reload();
        SetState(GameState.Playing);
    }

    public void GotoLevelById(string id)
    {
        if (levelManager) levelManager.GotoById(id);
        SetState(GameState.Playing);
    }

    public void BackToMenu()
    {
        // Tùy thiết kế, có thể tắt hết panel level ở đây và chỉ bật menu
        SafeSetActive(hudPanel, false);
        SafeSetActive(pausePanel, false);
        SafeSetActive(mainMenuPanel, true);
        SetState(GameState.MainMenu);
    }

    // ===== Helpers =====
    static void SafeSetActive(GameObject go, bool on)
    {
        if (go && go.activeSelf != on) go.SetActive(on);
    }
}
