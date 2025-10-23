using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using UnityEngine.Events;

public interface ILevelInject
{
    void OnLevelSpawned(LVContext ctx);
    void OnLevelDespawned();
}

[Serializable]
public class LVContext
{
    public Transform parent;
    public GameObject winPanel;
    public Canvas canvas;
}

/// <summary>
/// Quản lý level bằng prefab: chỉ giữ 1 level đang hoạt động.
/// </summary>
[DisallowMultipleComponent]
public class LevelManager : MonoBehaviour
{
    [Serializable]
    public class LevelEntry
    {
        public string id;            // định danh (VD: "LV37")
        public GameObject prefab;    // prefab của level
    }

    [Header("Catalog (prefabs)")]
    public List<LevelEntry> levels = new List<LevelEntry>();

    [Header("Hierarchy Targets")]
    public RectTransform panelsParent;   // nơi sẽ Instantiate level
    public GameObject winPanel;          // panel Victory dùng chung

    [Header("Start & Save")]
    public bool saveProgress = true;
    public int defaultStartIndex = 0;
    public string startLevelId = "";     // ưu tiên nếu có
    const string kSaveKey = "lv_prefab_index";

    [Header("Transition (optional)")]
    public bool useFade = true;
    [Min(0.01f)] public float fadeDuration = 0.25f;

    [Header("Events")]
    public UnityEvent onLevelChanged;

    // Runtime
    public int CurrentIndex { get; private set; } = -1;
    public string CurrentId => (CurrentIndex >= 0 && CurrentIndex < levels.Count) ? levels[CurrentIndex].id : null;
    public GameObject CurrentGO { get; private set; }

    Canvas _canvas;
    bool _isTransitioning;
    LVContext _ctx = new LVContext();

    void Awake()
    {
        if (!panelsParent)
        {
            Debug.LogError("[LVPrefabManager] Panels Parent chưa gán!");
            enabled = false; return;
        }
        _canvas = panelsParent.GetComponentInParent<Canvas>();

        // Ẩn win panel ban đầu
        if (winPanel) winPanel.SetActive(false);

        // Tính level start
        int start = defaultStartIndex;
        if (!string.IsNullOrEmpty(startLevelId))
        {
            int f = levels.FindIndex(l => l.id == startLevelId);
            if (f >= 0) start = f;
            else Debug.LogWarning($"[LVPrefabManager] Không tìm thấy startLevelId '{startLevelId}', dùng defaultStartIndex={defaultStartIndex}");
        }
        else if (saveProgress && PlayerPrefs.HasKey(kSaveKey))
        {
            start = Mathf.Clamp(PlayerPrefs.GetInt(kSaveKey, defaultStartIndex), 0, Mathf.Max(0, levels.Count - 1));
        }

        // Spawn level đầu
        Goto(start, save: false, invoke: false, instant: true);
    }

    void OnDestroy()
    {
        KillCurrentLevel();
    }

    // ===== Public API =====
    public void Next()
    {
        if (levels.Count == 0) return;
        int next = (CurrentIndex + 1) % levels.Count;
        Goto(next);
        if (winPanel) winPanel.SetActive(false);
    }

    public void Prev()
    {
        if (levels.Count == 0) return;
        int prev = (CurrentIndex - 1 + levels.Count) % levels.Count;
        Goto(prev);
    }

    public void Reload()
    {
        if (CurrentIndex >= 0) Goto(CurrentIndex, save: false);
    }

    public void GotoById(string id)
    {
        int idx = levels.FindIndex(l => l.id == id);
        if (idx >= 0) Goto(idx);
        else Debug.LogWarning($"[LVPrefabManager] Không tìm thấy id '{id}'.");
    }

    public void Goto(int index) => Goto(index, save: true, invoke: true, instant: false);

    // ===== Core =====
    void Goto(int index, bool save, bool invoke = true, bool instant = false)
    {
        if (levels.Count == 0) { Debug.LogWarning("[LVPrefabManager] Chưa có level prefab nào."); return; }
        index = Mathf.Clamp(index, 0, levels.Count - 1);
        if (_isTransitioning && !instant) return;

        var fromGO = CurrentGO;
        CurrentIndex = index;

        if (instant || !useFade)
        {
            // swap thẳng
            KillCurrentLevel();
            SpawnLevel(levels[CurrentIndex]);
            if (save) PlayerPrefs.SetInt(kSaveKey, CurrentIndex);
            if (invoke) onLevelChanged?.Invoke();
            return;
        }

        StartCoroutine(FadeSwap(fromGO, levels[CurrentIndex], save, invoke));
    }

    IEnumerator FadeSwap(GameObject from, LevelEntry toEntry, bool save, bool invoke)
    {
        _isTransitioning = true;

        CanvasGroup cgFrom = SetupCanvasGroup(from);
        // Fade out current
        if (from)
        {
            cgFrom.blocksRaycasts = false;
            cgFrom.interactable = false;
        }

        float t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / fadeDuration);
            if (from) cgFrom.alpha = 1f - k;
            yield return null;
        }

        // Kill current & fade in new
        KillCurrentLevel();
        var toGO = SpawnLevel(toEntry);
        CanvasGroup cgTo = SetupCanvasGroup(toGO);
        cgTo.alpha = 0f; cgTo.blocksRaycasts = false; cgTo.interactable = false;

        t = 0f;
        while (t < fadeDuration)
        {
            t += Time.unscaledDeltaTime;
            float k = Mathf.Clamp01(t / fadeDuration);
            cgTo.alpha = k;
            yield return null;
        }
        cgTo.alpha = 1f; cgTo.blocksRaycasts = true; cgTo.interactable = true;

        if (save) PlayerPrefs.SetInt(kSaveKey, CurrentIndex);
        if (invoke) onLevelChanged?.Invoke();

        _isTransitioning = false;
    }

    // ===== Spawn / Kill =====
    GameObject SpawnLevel(LevelEntry entry)
    {
        if (entry == null || !entry.prefab)
        {
            Debug.LogError("[LVPrefabManager] Prefab rỗng!");
            return null;
        }

        var go = Instantiate(entry.prefab, panelsParent);
        var rt = go.GetComponent<RectTransform>();
        if (rt)
        {
            // căn về giữa / fit parent
            rt.anchorMin = Vector2.zero;
            rt.anchorMax = Vector2.one;
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.localScale = Vector3.one;
            rt.anchoredPosition = Vector2.zero;
        }

        CurrentGO = go;

        // Chuẩn bị context
        _ctx.parent = panelsParent;
        _ctx.winPanel = winPanel;
        _ctx.canvas = _canvas;

        // 1) Gọi inject nếu level có implement ILevelInject
        foreach (var inj in go.GetComponentsInChildren<ILevelInject>(true))
            inj.OnLevelSpawned(_ctx);

        // 2) Auto-inject winPanel cho các script có field "winPanel" nhưng quên gán (phòng lỗi Prefab)
        AutoInjectCommonRefs(go);

        return go;
    }

    void KillCurrentLevel()
    {
        if (!CurrentGO) return;

        foreach (var inj in CurrentGO.GetComponentsInChildren<ILevelInject>(true))
            inj.OnLevelDespawned();

        Destroy(CurrentGO);
        CurrentGO = null;
    }

    // ===== Helpers =====
    CanvasGroup SetupCanvasGroup(GameObject go)
    {
        if (!go) return null;
        var cg = go.GetComponent<CanvasGroup>();
        if (!cg) cg = go.AddComponent<CanvasGroup>();
        return cg;
    }

    /// <summary>
    /// Tự bơm winPanel cho các component có field "winPanel" (public hoặc [SerializeField]) nếu đang null.
    /// Tránh crash kiểu "UnassignedReferenceException" khi đưa script vào prefab.
    /// </summary>
    void AutoInjectCommonRefs(GameObject root)
    {
        if (!winPanel || !root) return;

        var monos = root.GetComponentsInChildren<MonoBehaviour>(true);
        foreach (var m in monos)
        {
            if (!m) continue;
            var tp = m.GetType();

            var field = tp.GetField("winPanel",
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);

            if (field != null && field.FieldType == typeof(GameObject))
            {
                var cur = field.GetValue(m) as GameObject;
                if (cur == null)
                {
                    field.SetValue(m, winPanel);
                    // Debug.Log($"[LVPrefabManager] Inject winPanel → {tp.Name}");
                }
            }
        }
    }
}
