using UnityEngine;
using UnityEngine.EventSystems;
using System.Collections;
using Spine.Unity;

public class ScaleAndMove : MonoBehaviour, IPointerClickHandler
{
    [Header("Character Settings")]
    public RectTransform character;           // nhân vật (UI RectTransform)
    public SkeletonGraphic spineCharacter;    // Spine Graphic (nếu có)
    public string animName = "action1";       // Anim sẽ chạy sau khi kết thúc scale
    public bool loopAnim = false;

    [Header("Target Settings")]
    public RectTransform targetPoint;         // vị trí di chuyển đến
    [Min(0.01f)] public float moveTime = 1.5f; // thời gian di chuyển + scale
    public AnimationCurve ease = AnimationCurve.EaseInOut(0, 0, 1, 1);

    [Header("Optional")]
    public bool disableAfterClick = true; // Tắt khả năng click sau khi nhấn

    private Vector3 startScale;
    private Vector2 startPos;
    private bool isRunning = false;
    private bool canClick = true;

    void Awake()
    {
        if (!character)
            character = GetComponent<RectTransform>();

        if (character)
        {
            startScale = character.localScale;
            startPos = character.anchoredPosition;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (!canClick || isRunning || !character || !targetPoint)
            return;

        StartCoroutine(ScaleAndMove_Co());
        if (disableAfterClick)
            canClick = false;
    }

    IEnumerator ScaleAndMove_Co()
    {
        isRunning = true;

        Vector2 fromPos = startPos;
        Vector2 toPos = targetPoint.anchoredPosition;

        Vector3 fromScale = startScale;
        Vector3 toScale = Vector3.zero;

        float t = 0f;

        while (t < moveTime)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / moveTime);
            float eased = ease.Evaluate(k);

            // scale & move cùng lúc
            character.anchoredPosition = Vector2.LerpUnclamped(fromPos, toPos, eased);
            character.localScale = Vector3.LerpUnclamped(fromScale, toScale, eased);

            yield return null;
        }

        // đảm bảo kết thúc đúng vị trí
        character.anchoredPosition = toPos;
        character.localScale = toScale;

        // phát anim sau khi hoàn tất scale + move
        if (spineCharacter != null && !string.IsNullOrEmpty(animName))
        {
            spineCharacter.AnimationState.SetAnimation(0, animName, loopAnim);
        }

        isRunning = false;
    }

    [ContextMenu("Reset Character")]
    public void ResetCharacter()
    {
        if (!character) return;
        character.localScale = startScale;
        character.anchoredPosition = startPos;
        canClick = true;
        isRunning = false;
    }
}
