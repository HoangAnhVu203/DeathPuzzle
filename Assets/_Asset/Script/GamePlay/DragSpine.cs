using UnityEngine;
using UnityEngine.EventSystems;
using Spine;
using Spine.Unity;
using System.Collections;

public class DragSpine : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    public RectTransform targetZone;
    public RectTransform standPoint;
    public GameObject bloodVFX;
    public GameObject winPanel;    

    [Header("Spine Settings")]
    public SkeletonGraphic cat;
    public string successAnim = "meo4";
    public bool successLoop = false;
    public string idleAnim = "meo5";
    public GameObject ClearObj;
    public bool lockRootMotionOnSuccess = true;

    public Canvas rootCanvas;

    [Header("Other Spine")]
    public SkeletonGraphic otherSpine;
    public string otherAnimName;

    float snapDuration = 0.15f;
    float returnDuration = 0.20f;

    RectTransform rect;
    Vector2 startPos, pointerOffset;
    bool successState = false;

    void Awake()
    {
        rect = GetComponent<RectTransform>();
        if (!rootCanvas) rootCanvas = GetComponentInParent<Canvas>(true);
        if (!cat) cat = GetComponent<SkeletonGraphic>();
        startPos = rect.anchoredPosition;
        rect.localScale = Vector3.one;
    }

    bool ScreenToLocalInParent(PointerEventData e, out Vector2 lp)
    {
        var parentRT = rect.parent as RectTransform;
        var cam = (rootCanvas && rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : e.pressEventCamera;
        return RectTransformUtility.ScreenPointToLocalPointInRectangle(parentRT, e.position, cam, out lp);
    }

    public void OnBeginDrag(PointerEventData e)
    {
        successState = false;
        if (ScreenToLocalInParent(e, out var lp))
            pointerOffset = rect.anchoredPosition - lp;
    }

    public void OnDrag(PointerEventData e)
    {
        if (ScreenToLocalInParent(e, out var lp))
            rect.anchoredPosition = lp + pointerOffset;
    }

    public void OnEndDrag(PointerEventData e)
    {
        if (!targetZone)
        {
            StartCoroutine(Snap(rect.anchoredPosition, startPos, returnDuration));
            return;
        }

        var cam = (rootCanvas && rootCanvas.renderMode == RenderMode.ScreenSpaceOverlay) ? null : e.pressEventCamera;
        bool inside = RectTransformUtility.RectangleContainsScreenPoint(targetZone, e.position, cam);

        if (inside)
        {
            successState = true;

            // 🔹 Khi thả đúng vị trí, phát animation thành công
            var entry = cat.AnimationState.SetAnimation(0, successAnim, successLoop);
            if (entry != null)
            {
                entry.Complete += _ =>
                {
                    // 🔸 Khi cat anim xong -> otherSpine thực hiện anim
                    if (otherSpine)
                    {
                        var otherEntry = otherSpine.AnimationState.SetAnimation(0, otherAnimName, false);

                        if (otherEntry != null)
                        {
                            otherEntry.Complete += __ =>
                            {
                                if (bloodVFX) bloodVFX.SetActive(true);
                                if (winPanel) StartCoroutine(ShowWinPanelAfterDelay(0.7f));
                            };
                        }
                    }

                    // 🔸 Sau khi cat anim xong -> trở về idle
                    cat.AnimationState.SetAnimation(0, idleAnim, true);
                };
            }

            if (ClearObj) ClearObj.SetActive(false);

            Vector2 to = (standPoint ? standPoint.anchoredPosition : targetZone.anchoredPosition);
            StartCoroutine(Snap(rect.anchoredPosition, to, snapDuration));
        }
        else
        {
            successState = false;
            StartCoroutine(Snap(rect.anchoredPosition, startPos, returnDuration));
            if (cat) cat.AnimationState.SetAnimation(0, idleAnim, true);
        }
    }

    IEnumerator Snap(Vector2 from, Vector2 to, float dur)
    {
        float t = 0f;
        while (t < dur)
        {
            t += Time.deltaTime;
            rect.anchoredPosition = Vector2.Lerp(from, to, t / dur);
            yield return null;
        }
        rect.anchoredPosition = to;
    }

    IEnumerator ShowWinPanelAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        if (winPanel) winPanel.SetActive(true);
    }

    void LateUpdate()
    {
        if (successState && lockRootMotionOnSuccess && cat && cat.Skeleton != null)
        {
            var root = cat.Skeleton.RootBone;
            if (root != null)
            {
                root.X = 0f;
                root.Y = 0f;
            }
            cat.Skeleton.UpdateWorldTransform();
        }
    }
}
