using UnityEngine;
using UnityEngine.EventSystems;
using Spine.Unity;
using System.Collections;
using Unity.VisualScripting;

[RequireComponent(typeof(RectTransform))]
public class ClickUI : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Target UI")]
    public RectTransform target;              // Để trống sẽ tự lấy RectTransform trên object
    public GameObject obj;
    public GameObject winPanel;
    public GameObject waterVFX;
    public float timeEnableWinPanel;

    [Header("Press Scale")]
    [Range(0.5f, 1.2f)] public float pressScale = 0.92f;  // scale khi nhấn
    [Min(0.01f)] public float pressTime = 0.08f;          // thời gian thu nhỏ
    [Min(0.01f)] public float releaseTime = 0.12f;        // thời gian trả về

    [Header("Easing")]
    public AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Spine")]
    public SkeletonGraphic character;         // nhân vật Spine
    public string animName = "action1";       // anim sẽ chạy sau khi nhả chuột
    public bool loop = false;

    Vector3 baseScale;
    Coroutine tweenCo;

    void Reset()
    {
        target = GetComponent<RectTransform>();
    }

    void Awake()
    {
        if (!target) target = GetComponent<RectTransform>();
        baseScale = target.localScale;
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        // Thu nhỏ khi nhấn
        if (tweenCo != null) StopCoroutine(tweenCo);
        tweenCo = StartCoroutine(ScaleTo(baseScale * pressScale, pressTime));
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Trả scale về ban đầu rồi phát anim
        if (tweenCo != null) StopCoroutine(tweenCo);
        tweenCo = StartCoroutine(ReleaseThenPlay());

        obj.SetActive(false);
        waterVFX.SetActive(true);
        StartCoroutine(AcitveWinPanel());
    }

    IEnumerator ReleaseThenPlay()
    {
        yield return ScaleTo(baseScale, releaseTime);

        if (character != null && character.AnimationState != null && !string.IsNullOrEmpty(animName))
        {
            character.AnimationState.SetAnimation(0, animName, loop);
        }
    }

    IEnumerator ScaleTo(Vector3 targetScale, float duration)
    {
        Vector3 start = target.localScale;
        float t = 0f;
        duration = Mathf.Max(0.0001f, duration);

        while (t < duration)
        {
            t += Time.unscaledDeltaTime;             // dùng unscaled để không bị ảnh hưởng bởi Time.timeScale
            float k = Mathf.Clamp01(t / duration);
            float e = ease != null ? ease.Evaluate(k) : k;
            target.localScale = Vector3.LerpUnclamped(start, targetScale, e);
            yield return null;
        }
        target.localScale = targetScale;
    }

    IEnumerator AcitveWinPanel()
    {
        yield return new WaitForSeconds(timeEnableWinPanel);

        winPanel.SetActive(true);
    }
}
