using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Spine.Unity;
using System.Collections;

[RequireComponent(typeof(RawImage))]
public class EraseItem : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [Header("Refs")]
    [SerializeField] RawImage targetImage;
    [SerializeField] GameObject deactivateTarget;
    [SerializeField] SkeletonGraphic Character;
    [SerializeField] string animName = "action1";
    [SerializeField] GameObject ObjectToEnable;
    [SerializeField] GameObject panelWin;
    [SerializeField] float delayTime = 0f;

    [Header("Brush")]
    [SerializeField] int brushRadius = 16;
    [SerializeField, Range(0, 1f)] float alphaThreshold = 0.5f;
    [SerializeField, Range(0, 1f)] float completeThreshold = 0.80f;

    [Header("Character Fall")]
    public bool enableCharacterFall = true;
    [SerializeField] float fallDistance = 500f;
    [SerializeField] float fallDuration = 1.5f;
    [SerializeField, Min(1f)] float fallEasePower = 2f;

    Texture2D tex;
    Color32[] buffer;
    int w, h;

    HashSet<int> erasable = new();
    bool[] erased;
    int erasableCount, erasedCount;
    bool finishedOnce = false;

    void Awake()
    {
        if (!targetImage) targetImage = GetComponent<RawImage>();
        if (!deactivateTarget) deactivateTarget = targetImage.gameObject;

        
        var src = targetImage.texture as Texture2D;
        tex = new Texture2D(src.width, src.height, TextureFormat.RGBA32, false, false);
        tex.SetPixels32(src.GetPixels32());
        tex.Apply(false);
        targetImage.texture = tex;

        w = tex.width; h = tex.height;
        buffer = tex.GetPixels32();
        erased = new bool[w * h];

        for (int y = 0; y < h; y++)
        {
            int row = y * w;
            for (int x = 0; x < w; x++)
            {
                int i = row + x;
                if (buffer[i].a / 255f > alphaThreshold)
                    erasable.Add(i);
            }
        }
        erasableCount = erasable.Count;
    }

    
    void Start()
    {
        if (panelWin == null)
        {
            // tìm trong toàn bộ scene, kể cả inactive
            var allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
            foreach (var obj in allObjects)
            {
                if (obj.name == "WinPanel")
                {
                    panelWin = obj;
                    break;
                }
            }

            
        }
    }



    public void OnPointerDown(PointerEventData e) => EraseAt(e.position, e.pressEventCamera);
    public void OnDrag(PointerEventData e) => EraseAt(e.position, e.pressEventCamera);

    void EraseAt(Vector2 screenPos, Camera eventCam)
    {
        // TỰ LẤY CAMERA PHÙ HỢP CHO PREFAB
        var canvas = targetImage.canvas;
        Camera camForRT =
            (canvas && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
            ? null
            : (canvas ? canvas.worldCamera : eventCam);

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                targetImage.rectTransform, screenPos, camForRT, out var local))
            return;

        Rect r = targetImage.rectTransform.rect;
        float nx = Mathf.InverseLerp(r.xMin, r.xMax, local.x);
        float ny = Mathf.InverseLerp(r.yMin, r.yMax, local.y);

        int cx = Mathf.RoundToInt(nx * (w - 1));
        int cy = Mathf.RoundToInt(ny * (h - 1));

        int r2 = brushRadius * brushRadius;
        int minX = Mathf.Max(0, cx - brushRadius);
        int maxX = Mathf.Min(w - 1, cx + brushRadius);
        int minY = Mathf.Max(0, cy - brushRadius);
        int maxY = Mathf.Min(h - 1, cy + brushRadius);

        int added = 0;
        for (int y = minY; y <= maxY; y++)
        {
            int dy = y - cy;
            int row = y * w;
            for (int x = minX; x <= maxX; x++)
            {
                int dx = x - cx;
                if (dx * dx + dy * dy > r2) continue;

                int i = row + x;
                if (!erasable.Contains(i) || erased[i]) continue;

                var c = buffer[i]; c.a = 0; buffer[i] = c;
                erased[i] = true;
                added++;
            }
        }

        if (added <= 0) return;

        erasedCount += added;
        tex.SetPixels32(buffer);
        tex.Apply(false);

        if (!finishedOnce && erasableCount > 0 &&
            erasedCount >= completeThreshold * erasableCount)
        {
            finishedOnce = true;
            StartCoroutine(HandleCompleteSequence());
        }
    }

    IEnumerator HandleCompleteSequence()
    {
        if (delayTime > 0f) yield return new WaitForSeconds(delayTime);

        // Play anim
        if (Character)
        {
            var entry = Character.AnimationState.SetAnimation(0, animName, false);
            if (entry != null)
            {
                entry.Complete += _ =>
                {
                    Character.StartCoroutine(ShowPanelWinAfterDelay(0.6f));
                };
            }
        }

        // Rơi (nếu bật)
        if (enableCharacterFall && Character)
        {
            RectTransform rtChar = Character.GetComponent<RectTransform>();
            if (rtChar != null && rtChar.IsChildOf(targetImage.canvas.transform))
            {
                StartCoroutine(FallDownUI(rtChar, fallDistance, fallDuration, fallEasePower));
            }
            else
            {
                Debug.LogWarning("[EraseItem] Character không nằm trong Canvas hợp lệ, bỏ qua hiệu ứng rơi.");
            }
        }


        if (ObjectToEnable) ObjectToEnable.SetActive(true);
        if (deactivateTarget) deactivateTarget.SetActive(false);
    }

    IEnumerator ShowPanelWinAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (panelWin) panelWin.SetActive(true);
    }

    static IEnumerator FallDownUI(RectTransform rt, float distance, float duration, float easePower = 2f)
    {
        Vector2 from = rt.anchoredPosition;
        Vector2 to = from + new Vector2(0f, -Mathf.Abs(distance));
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);
            float eased = 1f - Mathf.Pow(1f - k, Mathf.Max(1f, easePower));
            rt.anchoredPosition = Vector2.LerpUnclamped(from, to, eased);
            yield return null;
        }
        rt.anchoredPosition = to;
    }
}
