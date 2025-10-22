using UnityEngine;
using Spine.Unity;
using Spine;
using System.Collections;

public class PanelFocusCharacter : MonoBehaviour
{
    public SkeletonGraphic player;
    public string triggerAnim = "action2";

    public RectTransform panel;                // panel win
    public Vector2 targetPos = new Vector2(0f, 0f); // vị trí đích
    public float moveTime = 0.5f;              // thời gian di chuyển
    public AnimationCurve moveCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);
    public bool setActiveOnShow = true;        // bật panel nếu đang tắt

    void Start()
    {
        if (player)
            player.AnimationState.Complete += OnAnimComplete;
    }

    void OnDestroy()
    {
        if (player)
            player.AnimationState.Complete -= OnAnimComplete;
    }

    void OnAnimComplete(TrackEntry entry)
    {
        if (entry.Animation.Name == triggerAnim)
        {
            StartCoroutine(SlideToTarget());
        }
    }

    IEnumerator SlideToTarget()
    {
        if (!panel) yield break;

        if (setActiveOnShow && !panel.gameObject.activeSelf)
            panel.gameObject.SetActive(true);

        Vector2 startPos = panel.anchoredPosition;
        Vector2 endPos = targetPos;
        float t = 0f;

        while (t < 1f)
        {
            t += Time.deltaTime / moveTime;
            float eased = moveCurve.Evaluate(t);
            panel.anchoredPosition = Vector2.LerpUnclamped(startPos, endPos, eased);
            yield return null;
        }

        panel.anchoredPosition = endPos;
    }
}
