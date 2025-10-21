using UnityEngine;
using UnityEngine.EventSystems;
using Spine.Unity;
using System.Collections;

public class DragUI : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public RectTransform targetZone;
    public SkeletonGraphic player;
    public string animName = "action1";
    public string animAfterReset;
    public float resetDelay = 3f;
    public GameObject CanvasVictory;
    public GameObject obj1;
    public GameObject obj2;
    public DropZone drop;
    public GameObject obj3;

    private RectTransform rectTransform;
    private Canvas canvas;
    private Vector2 startPos;

    void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        canvas = GetComponentInParent<Canvas>();
        startPos = rectTransform.anchoredPosition;
    }

    public void OnBeginDrag(PointerEventData eventData) { }

    public void OnDrag(PointerEventData eventData)
    {
        rectTransform.anchoredPosition += eventData.delta / canvas.scaleFactor;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (Vector2.Distance(rectTransform.anchoredPosition, targetZone.anchoredPosition) < 50f)
        {
            player.AnimationState.SetAnimation(0, animName, false);

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
    }

    private IEnumerator ResetImage()
    {
        rectTransform.localScale = Vector3.zero;

        yield return new WaitForSeconds(resetDelay);

        rectTransform.anchoredPosition = startPos;
        rectTransform.localScale = Vector3.one;
        player.AnimationState.SetAnimation(0, animAfterReset, true);
    }

    private IEnumerator WaitforVic()
    {
        yield return new WaitForSeconds(2f);
        CanvasVictory.SetActive(true);
    }
}
