using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Spine.Unity;

[RequireComponent(typeof(RawImage))]
public class EraseItem : MonoBehaviour, IPointerDownHandler, IDragHandler
{
    
    [SerializeField] RawImage targetImage;          
    [SerializeField] Camera uiCamera;               
    [SerializeField] GameObject deactivateTarget;   
    [SerializeField] SkeletonGraphic Character;
    [SerializeField] string animName;
    [SerializeField] GameObject Object;

    
    [SerializeField] int brushRadius = 16;          
    [SerializeField, Range(0, 1f)] float alphaThreshold = 0.5f; 
    [SerializeField, Range(0, 1f)] float completeThreshold = 0.80f;
    

    Texture2D tex;                  
    Color32[] buffer; 
    int w, h;

    
    HashSet<int> erasable = new HashSet<int>();
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
                {
                    erasable.Add(i);
                }
            }
        }
        erasableCount = erasable.Count;
    }

    public void OnPointerDown(PointerEventData eventData) => EraseAt(eventData.position);
    public void OnDrag(PointerEventData eventData) => EraseAt(eventData.position);

    void EraseAt(Vector2 screenPos)
    {
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
                targetImage.rectTransform, screenPos, uiCamera, out var local))
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
                if (!erasable.Contains(i)) continue;   
                if (erased[i]) continue;              

                var c = buffer[i]; c.a = 0; buffer[i] = c;
                erased[i] = true;
                added++;
            }
        }

        if (added > 0)
        {
            erasedCount += added;
            tex.SetPixels32(buffer);
            tex.Apply(false);

            // đạt % -> tắt object
            if (!finishedOnce && erasableCount > 0 && erasedCount >= completeThreshold * erasableCount)
            {
                finishedOnce = true;

                if (Character) Character.AnimationState.SetAnimation(0, animName, false);

                var rectChar = Character ? Character.GetComponent<RectTransform>() : null;
                if (rectChar) Character.StartCoroutine(FallDownUI(rectChar, 500f, 0.3f)); 
                Object.SetActive(true);

                deactivateTarget.SetActive(false);
            }
        }
    }

    static System.Collections.IEnumerator FallDownUI(RectTransform rt, float distance, float duration)
    {
        Vector2 from = rt.anchoredPosition;
        Vector2 to = from + new Vector2(0f, -Mathf.Abs(distance));
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float k = t / duration;
            float eased = 1f - (1f - k) * (1f - k);
            rt.anchoredPosition = Vector2.LerpUnclamped(from, to, eased);
            yield return null;
        }
        rt.anchoredPosition = to;
    }


}
