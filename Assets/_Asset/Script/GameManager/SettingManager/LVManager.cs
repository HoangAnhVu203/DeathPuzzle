using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

[DisallowMultipleComponent]
public class LVManager : MonoBehaviour
{
    [Serializable]
    public class PanelItem
    {
        public string id;           // id để gọi level theo tên
        public GameObject panel;    // panel UI
    }

    [Header("Panels (levels)")]
    public List<PanelItem> panels = new List<PanelItem>();
    public GameObject winPanel;

    [Tooltip("Tự lấy các con trực tiếp của parent làm panel theo thứ tự Hierarchy.")]
    public bool autoFromParent = true;
    public Transform panelsParent;

    [Header("Start & Save")]
    public bool saveProgress = true;
    public int defaultStartIndex = 0;

    [Tooltip("Nếu điền tên level, game sẽ bắt đầu từ id này thay vì defaultStartIndex.")]
    public string startLevelId = "";   // 🆕 Thêm biến mới

    [Header("Transition (optional)")]
    public bool useFade = true;
    [Min(0.01f)] public float fadeDuration = 0.25f;

    [Header("Events")]
    public UnityEvent onPanelChanged;

    const string kSaveKey = "panel_level_index";

    public int CurrentIndex { get; private set; } = -1;
    public GameObject CurrentPanel => (CurrentIndex >= 0 && CurrentIndex < panels.Count) ? panels[CurrentIndex].panel : null;
    public string CurrentId => (CurrentIndex >= 0 && CurrentIndex < panels.Count) ? panels[CurrentIndex].id : null;

    void Awake()
    {
        if (autoFromParent && panelsParent)
        {
            panels.Clear();
            for (int i = 0; i < panelsParent.childCount; i++)
            {
                var go = panelsParent.GetChild(i).gameObject;
                panels.Add(new PanelItem { id = go.name, panel = go });
            }
        }

        // Tắt tất cả panels
        foreach (var p in panels)
            if (p?.panel) p.panel.SetActive(false);

        // === Xác định level khởi đầu ===
        int startIndex = defaultStartIndex;

        // Nếu có id cụ thể → ưu tiên id
        if (!string.IsNullOrEmpty(startLevelId))
        {
            int found = panels.FindIndex(p => p.id == startLevelId);
            if (found >= 0)
            {
                startIndex = found;
                Debug.Log($"[LVManager] Bắt đầu từ level id = '{startLevelId}' (index {startIndex}).");
            }
            else
            {
                Debug.LogWarning($"[LVManager] Không tìm thấy level id = '{startLevelId}', fallback về defaultStartIndex = {defaultStartIndex}");
            }
        }
        else if (saveProgress && PlayerPrefs.HasKey(kSaveKey))
        {
            startIndex = Mathf.Clamp(PlayerPrefs.GetInt(kSaveKey, defaultStartIndex), 0, Mathf.Max(0, panels.Count - 1));
        }

        Goto(startIndex, save: false, invokeEvent: false, instant: true);
    }

    // ===== API =====
    public void Next()
    {
        if (panels.Count == 0) return;
        int next = (CurrentIndex + 1) % panels.Count;
        Goto(next);
        if (winPanel) winPanel.SetActive(false);
    }

    public void Prev()
    {
        if (panels.Count == 0) return;
        int prev = (CurrentIndex - 1 + panels.Count) % panels.Count;
        Goto(prev);
    }

    public void Reload()
    {
        if (CurrentIndex >= 0) Goto(CurrentIndex);
    }

    public void GotoById(string id)
    {
        int idx = panels.FindIndex(p => p != null && p.id == id);
        if (idx >= 0) Goto(idx);
        else Debug.LogWarning($"[LVManager] Không tìm thấy id='{id}'.");
    }

    public void Goto(int index) => Goto(index, save: true, invokeEvent: true, instant: false);

    // ===== Core =====
    bool _isTransitioning = false;

    void Goto(int index, bool save, bool invokeEvent, bool instant)
    {
        if (panels.Count == 0) { Debug.LogWarning("[LVManager] Chưa cấu hình panel."); return; }
        index = Mathf.Clamp(index, 0, panels.Count - 1);

        if (_isTransitioning && !instant) return;

        var oldPanel = CurrentPanel;
        CurrentIndex = index;
        var newPanel = CurrentPanel;

        if (instant || !useFade)
        {
            if (oldPanel) oldPanel.SetActive(false);
            if (newPanel) newPanel.SetActive(true);
            if (save) PlayerPrefs.SetInt(kSaveKey, CurrentIndex);
            if (invokeEvent) onPanelChanged?.Invoke();
            return;
        }

        StartCoroutine(FadeSwap(oldPanel, newPanel, save, invokeEvent));
    }

    System.Collections.IEnumerator FadeSwap(GameObject from, GameObject to, bool save, bool invoke)
    {
        _isTransitioning = true;

        var cgFrom = SetupCanvasGroup(from);
        var cgTo = SetupCanvasGroup(to);

        if (to)
        {
            to.SetActive(true);
            cgTo.alpha = 0f;
            cgTo.blocksRaycasts = false;
            cgTo.interactable = false;
        }

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
            if (to) cgTo.alpha = k;
            yield return null;
        }

        if (from) { from.SetActive(false); cgFrom.alpha = 1f; }
        if (to)
        {
            cgTo.alpha = 1f;
            cgTo.blocksRaycasts = true;
            cgTo.interactable = true;
        }

        if (save) PlayerPrefs.SetInt(kSaveKey, CurrentIndex);
        if (invoke) onPanelChanged?.Invoke();

        _isTransitioning = false;
    }

    CanvasGroup SetupCanvasGroup(GameObject go)
    {
        if (!go) return null;
        var cg = go.GetComponent<CanvasGroup>();
        if (!cg) cg = go.AddComponent<CanvasGroup>();
        return cg;
    }
}
