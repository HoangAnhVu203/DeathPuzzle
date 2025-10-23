using UnityEngine;
using UnityEngine.EventSystems;
using Spine.Unity;
using System.Collections;

[RequireComponent(typeof(RectTransform))]
public class DragUIWithPulseControl : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Target Settings")]
    public RectTransform targetZone;
    public SkeletonGraphic player;
    public string animName = "action1";
    public string animAfterReset;
    public float resetDelay = 3f;

    [Header("Objects")]
    public GameObject winPanel;
    public GameObject obj1;
    public GameObject obj2;
    public DropZone drop;
    public GameObject obj3;
    public GameObject bloodVFX;

    [Header("Scale Pulse Settings")]
    [Min(0f)] public float minScale = 0.95f;
    [Min(0f)] public float maxScale = 1.05f;
    [Min(0.01f)] public float cycleDuration = 1.2f;
    public bool useUnscaledTime = true;
    public AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private RectTransform rectTransform;
    private Canvas canvas;
    private Vector2 startPos;
    private Vector3 baseScale;
    private bool isDragging = false;
    private bool atStartPosition = true; // chỉ scale khi object ở vị trí ban đầu

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        startPos = rectTransform.anchoredPosition;
        baseScale = rectTransform.localScale;
    }

    void OnEnable()
    {
        rectTransform.localScale = baseScale * minScale;
    }

    void Update()
    {
        if (!rectTransform) return;

        // 🔸 Chỉ scale khi đang ở vị trí ban đầu và không bị kéo
        if (isDragging || !atStartPosition) return;

        float t = (useUnscaledTime ? Time.unscaledTime : Time.time) / (cycleDuration * 0.5f);
        t = Mathf.PingPong(t, 1f);
        float k = ease.Evaluate(t);
        float s = Mathf.Lerp(minScale, maxScale, k);
        rectTransform.localScale = baseScale * s;
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        isDragging = true;
        atStartPosition = false; // 🔸 không còn ở vị trí ban đầu
    }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;

        // 🔸 nếu thả đúng vị trí mục tiêu
        if (Vector2.Distance(rectTransform.anchoredPosition, targetZone.anchoredPosition) < 50f)
        {
            var entry = player.AnimationState.SetAnimation(0, animName, false);
            if (entry != null)
            {
                entry.Complete += _ =>
                {
                    if (bloodVFX) bloodVFX.SetActive(true);
                };
            }

            if (animName == "action3")
            {
                obj1.SetActive(false);
                obj2.SetActive(true);
                drop.enabled = true;
                
            }

            StartCoroutine(ResetImage());

            if (animName == "action2" || animName == "meo4")
            {
                obj3.SetActive(false);
                StartCoroutine(WaitforVic());
            }
        }
        else
        {
            StartCoroutine(SnapBackToStart());
        }
    }

    private IEnumerator SnapBackToStart()
    {
        Vector2 start = rectTransform.anchoredPosition;
        Vector2 end = startPos;
        float duration = 0.25f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            rectTransform.anchoredPosition = Vector2.Lerp(start, end, elapsed / duration);
            yield return null;
        }

        rectTransform.anchoredPosition = end;
        atStartPosition = true; // 🔹 quay lại vị trí ban đầu → bật lại scale
    }

    private IEnumerator ResetImage()
    {
        rectTransform.localScale = Vector3.zero;
        yield return new WaitForSeconds(resetDelay);

        rectTransform.anchoredPosition = startPos;
        rectTransform.localScale = baseScale;
        atStartPosition = true; // 🔹 reset về trạng thái ban đầu → bật scale lại

        if (animName != "action2")
        {
            player.AnimationState.SetAnimation(0, animAfterReset, true);
        }
    }

    private IEnumerator WaitforVic()
    {
        yield return new WaitForSeconds(2.3f);
        winPanel.SetActive(true);
    }
}
