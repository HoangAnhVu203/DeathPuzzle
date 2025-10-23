using UnityEngine;
using System.Collections;

public class MoveWhenDeActive : MonoBehaviour
{
    [Header("Object cần theo dõi (khi bị tắt -> di chuyển)")]
    public GameObject watchTarget;
    [Tooltip("Chu kỳ kiểm tra trạng thái của watchTarget (giây)")]
    [Min(0.02f)] public float checkInterval = 0.2f;
    [Tooltip("Chỉ kích hoạt 1 lần duy nhất")]
    public bool triggerOnce = true;
    [Tooltip("Trễ trước khi bắt đầu di chuyển sau khi phát hiện bị tắt")]
    [Min(0f)] public float delayBeforeMove = 0f;

    [Header("Di chuyển tới vị trí đích (UI - Anchored Position)")]
    public Vector2 targetAnchoredPos = Vector2.zero;

    [Header("Tween")]
    [Min(0.01f)] public float moveDuration = 0.5f;
    public AnimationCurve moveEase = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public bool useUnscaledTime = true;

    // Nội bộ
    RectTransform rt;
    bool hasTriggered = false;
    Coroutine monitorCo;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    void OnEnable()
    {
        if (monitorCo != null) StopCoroutine(monitorCo);
        monitorCo = StartCoroutine(MonitorOther());
    }

    void OnDisable()
    {
        if (monitorCo != null) StopCoroutine(monitorCo);
        monitorCo = null;
    }

    IEnumerator MonitorOther()
    {
        if (watchTarget == null)
        {
            yield break;
        }

        // Nếu target bị tắt sẵn → di chuyển luôn
        if (!watchTarget.activeInHierarchy)
        {
            TryTriggerMove();
            yield break;
        }

        var wait = new WaitForSeconds(checkInterval);
        while (true)
        {
            if (!watchTarget.activeInHierarchy)
            {
                TryTriggerMove();
                if (triggerOnce) yield break;
            }
            yield return wait;
        }
    }

    void TryTriggerMove()
    {
        if (hasTriggered && triggerOnce) return;
        hasTriggered = true;

        StartCoroutine(Co_Move());
    }

    IEnumerator Co_Move()
    {
        // delay trước khi di chuyển
        if (delayBeforeMove > 0f)
        {
            float tDelay = 0f;
            while (tDelay < delayBeforeMove)
            {
                tDelay += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
                yield return null;
            }
        }

        Vector2 from = rt.anchoredPosition;
        Vector2 to = targetAnchoredPos;
        float t = 0f;

        while (t < moveDuration)
        {
            t += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float k = Mathf.Clamp01(t / moveDuration);
            float e = moveEase.Evaluate(k);
            rt.anchoredPosition = Vector2.LerpUnclamped(from, to, e);
            yield return null;
        }

        rt.anchoredPosition = to;
    }
}
