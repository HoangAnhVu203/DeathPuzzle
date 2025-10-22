using UnityEngine;

public class PulseScale : MonoBehaviour
{
    [Header("Target")]
    public RectTransform target;          
    [Header("Scale")]
    [Min(0f)] public float minScale = 1.0f;
    [Min(0f)] public float maxScale = 1.2f;

    [Header("Timing")]
    [Min(0.01f)] public float cycleDuration = 1.2f;  
    public bool useUnscaledTime = true;

    [Header("Easing")]
    public AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

    Vector3 baseScale;

    void Reset()
    {
        target = GetComponent<RectTransform>();
    }

    void Awake()
    {
        if (!target) target = GetComponent<RectTransform>();
        baseScale = target.localScale;
    }

    void OnEnable()
    {
        // đảm bảo scale ban đầu
        target.localScale = baseScale * minScale;
    }

    void Update()
    {
        if (!target) return;

        
        float t = (useUnscaledTime ? Time.unscaledTime : Time.time) / (cycleDuration * 0.5f);
        t = Mathf.PingPong(t, 1f); // 0 -> 1 -> 0
        float k = ease.Evaluate(t);

        float s = Mathf.Lerp(minScale, maxScale, k);
        target.localScale = baseScale * s;
    }

    
    public void SetScaleRange(float newMin, float newMax)
    {
        minScale = Mathf.Max(0f, newMin);
        maxScale = Mathf.Max(minScale, newMax);
    }
}
