using UnityEngine;
using System.Collections;

[RequireComponent(typeof(RectTransform))]
public class TransUI : MonoBehaviour
{
    public Vector2 targetAnchoredPos = Vector2.zero;  

    public bool useCustomStartPos = false;            
    public Vector2 startAnchoredPos = new Vector2(0, -1500f);

    [Min(0.01f)] public float duration = 0.25f;
    public AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public bool useUnscaledTime = true;               
    public float startDelay = 0f;                     

    RectTransform rt;
    Coroutine moveCo;

    void Awake()
    {
        rt = GetComponent<RectTransform>();
    }

    void OnEnable()
    {
        // Set điểm bắt đầu
        if (useCustomStartPos)
            rt.anchoredPosition = startAnchoredPos;

        // Bắt đầu tween
        if (moveCo != null) StopCoroutine(moveCo);
        moveCo = StartCoroutine(MoveTo(targetAnchoredPos, duration, startDelay));
    }

    void OnDisable()
    {
        if (moveCo != null)
        {
            StopCoroutine(moveCo);
            moveCo = null;
        }
    }

    IEnumerator MoveTo(Vector2 to, float time, float delay)
    {
        if (delay > 0f)
        {
            if (useUnscaledTime)
            {
                float t = 0f;
                while (t < delay) { t += Time.unscaledDeltaTime; yield return null; }
            }
            else yield return new WaitForSeconds(delay);
        }

        Vector2 from = rt.anchoredPosition;
        float t2 = 0f;
        while (t2 < time)
        {
            t2 += useUnscaledTime ? Time.unscaledDeltaTime : Time.deltaTime;
            float k = Mathf.Clamp01(t2 / time);
            float e = ease.Evaluate(k);
            rt.anchoredPosition = Vector2.LerpUnclamped(from, to, e);
            yield return null;
        }
        rt.anchoredPosition = to;
        moveCo = null;
    }
}
