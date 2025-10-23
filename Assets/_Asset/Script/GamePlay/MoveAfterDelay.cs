using UnityEngine;
using System.Collections;
using Spine.Unity;

public class MoveAfterDelay : MonoBehaviour
{
    [Header("Target Movement")]
    public RectTransform target;              // nếu để trống → lấy RectTransform của chính object
    public Vector2 targetPosition;            // vị trí đích (UI anchoredPosition)
    [Min(0.01f)] public float moveDuration = 1.0f;
    public float delayBeforeMove = 0.5f;
    public AnimationCurve moveEase = AnimationCurve.EaseInOut(0, 0, 1, 1);


    [Header("Spine Animation (optional)")]
    public SkeletonGraphic character;
    public string animName = "action1";
    public bool loopAnim = false;

    [Header("One-shot")]
    public bool disableAfterTriggered = true; // nếu true → chỉ chạy 1 lần

    bool isRunning = false;
    bool triggered = false;

    void Awake()
    {
        if (!target) target = GetComponent<RectTransform>();
    }

    // Hàm này sẽ được gọi từ object khác (Button, EventTrigger, script…)
    public void TriggerMove()
    {
        if (disableAfterTriggered && triggered) return;
        if (isRunning) return;

        triggered = true;
        StartCoroutine(Co_MoveSequence());
    }

    IEnumerator Co_MoveSequence()
    {
        isRunning = true;

        // chờ delay
        if (delayBeforeMove > 0f)
            yield return new WaitForSeconds(delayBeforeMove);

        // tween vị trí
        Vector2 from = target.anchoredPosition;
        Vector2 to = targetPosition;
        float t = 0f;
        while (t < moveDuration)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / moveDuration);
            float eased = moveEase.Evaluate(k);
            target.anchoredPosition = Vector2.LerpUnclamped(from, to, eased);
            yield return null;
        }
        target.anchoredPosition = to;

        // phát anim khi tới nơi
        if (character && !string.IsNullOrEmpty(animName))
            character.AnimationState.SetAnimation(0, animName, loopAnim);

        isRunning = false;
    }
}
