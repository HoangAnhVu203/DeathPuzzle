using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Spine.Unity;
using System.Collections;

[RequireComponent(typeof(RawImage))]
public class PainZone : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    [SerializeField] RawImage targetImage;
    [SerializeField] GameObject deactivateTarget;
    [SerializeField, Range(0, 1f)] float completeThreshold = 0.30f;

    [SerializeField, Range(4, 256)] int brushRadius = 40;
    [SerializeField, Range(0, 1f)] float revealedAlphaThreshold = 0.1f;

    [SerializeField] SkeletonGraphic character;
    [SerializeField] string completeAnim = "action1";
    [SerializeField] private GameObject cucDa;
    [SerializeField] private GameObject bloodVFX;
    [SerializeField] private GameObject panelWin;

    [SerializeField] Vector2 moveOffset = new Vector2(0f, -300f);
    [SerializeField, Min(0.01f)] float moveDuration = 0.3f;
    [SerializeField] bool useUnscaledTime = true;

    [SerializeField, Range(1f, 5f)] float easePower = 2f;

    Texture2D workTex;
    Color32[] srcPixels;
    Color32[] bufPixels;
    bool[] counted;
    int w, h, totalCount, revealedCount;
    bool fired;

    void Awake()
    {
        if (!targetImage) targetImage = GetComponent<RawImage>();
        if (!deactivateTarget) deactivateTarget = targetImage.gameObject;

        var src = targetImage.texture;
        var readable = MakeReadableCopy(src);

        w = readable.width; h = readable.height;
        srcPixels = readable.GetPixels32();
        bufPixels = new Color32[w * h];
        counted = new bool[w * h];

        // Ẩn toàn bộ (alpha = 0), giữ RGB gốc
        for (int i = 0; i < bufPixels.Length; i++)
        {
            var p = srcPixels[i];
            p.a = 0;
            bufPixels[i] = p;
        }

        workTex = new Texture2D(w, h, TextureFormat.RGBA32, false, false);
        workTex.SetPixels32(bufPixels);
        workTex.Apply(false, false);

        targetImage.texture = workTex;
        targetImage.color = Color.white;

        totalCount = w * h;
        revealedCount = 0;
        fired = false;
    }

    private void Start()
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

    public void OnPointerDown(PointerEventData e) => RevealAt(e.position);
    public void OnDrag(PointerEventData e) => RevealAt(e.position);

    void RevealAt(Vector2 screenPos)
    {
        var canvas = targetImage.canvas;
        Camera uiCam = (canvas && canvas.renderMode == RenderMode.ScreenSpaceOverlay)
                       ? null : canvas.worldCamera;

        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                targetImage.rectTransform, screenPos, uiCam, out var local))
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

        int newlyRevealed = 0;

        for (int y = minY; y <= maxY; y++)
        {
            int dy = y - cy;
            int row = y * w;
            for (int x = minX; x <= maxX; x++)
            {
                int dx = x - cx;
                if (dx * dx + dy * dy > r2) continue;

                int i = row + x;

                var srcP = srcPixels[i];
                var curP = bufPixels[i];
                if (curP.a == srcP.a) continue;

                curP.r = srcP.r; curP.g = srcP.g; curP.b = srcP.b;
                curP.a = srcP.a;
                bufPixels[i] = curP;

                if (!counted[i] && curP.a / 255f >= revealedAlphaThreshold)
                {
                    counted[i] = true;
                    newlyRevealed++;
                }
            }
        }

        if (newlyRevealed > 0)
        {
            revealedCount += newlyRevealed;
            workTex.SetPixels32(bufPixels);
            workTex.Apply(false);

            float progress = (float)revealedCount / totalCount;
            if (!fired && progress >= completeThreshold)
            {
                fired = true;
                if (character)
                {
                    var entry = character.AnimationState.SetAnimation(0, completeAnim, false);
                    if (entry != null)
                    {
                        // Khi anim kết thúc → bật bloodVFX và panelWin sau 1 giây
                        entry.Complete += _ =>
                        {
                            character.StartCoroutine(ShowAfterAnim());
                        };
                    }
                }

                // Di chuyển nhân vật
                var rtChar = character ? character.GetComponent<RectTransform>() : null;
                if (rtChar) character.StartCoroutine(MoveAnchoredBy(rtChar, moveOffset, moveDuration, useUnscaledTime, easePower));

                // Bật/tắt các object khác
                if (cucDa) cucDa.SetActive(true);
                if (deactivateTarget) deactivateTarget.SetActive(false);
            }
        }
    }

    private IEnumerator ShowAfterAnim()
    {
        yield return new WaitForSeconds(0.1f);
        if (bloodVFX) bloodVFX.SetActive(true);
        if (panelWin) panelWin.SetActive(true);
    }

    static IEnumerator MoveAnchoredBy(RectTransform rt, Vector2 delta, float duration, bool unscaled, float powEase)
    {
        Vector2 from = rt.anchoredPosition;
        Vector2 to = from + delta;
        float t = 0f;
        duration = Mathf.Max(0.0001f, duration);

        while (t < duration)
        {
            t += unscaled ? Time.unscaledDeltaTime : Time.deltaTime;
            float k = Mathf.Clamp01(t / duration);
            float eased = 1f - Mathf.Pow(1f - k, Mathf.Max(1f, powEase));
            rt.anchoredPosition = Vector2.LerpUnclamped(from, to, eased);
            yield return null;
        }
        rt.anchoredPosition = to;
    }

    static Texture2D MakeReadableCopy(Texture src)
    {
        if (src == null) return null;
        int w = src.width, h = src.height;
        var rt = RenderTexture.GetTemporary(w, h, 0, RenderTextureFormat.ARGB32, RenderTextureReadWrite.sRGB);
        Graphics.Blit(src, rt);
        var prev = RenderTexture.active;
        RenderTexture.active = rt;
        var copy = new Texture2D(w, h, TextureFormat.RGBA32, false, false);
        copy.ReadPixels(new Rect(0, 0, w, h), 0, 0);
        copy.Apply(false, false);
        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(rt);
        return copy;
    }
}
