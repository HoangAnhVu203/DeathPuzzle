using UnityEngine;
using Spine.Unity;

public class DropZone : MonoBehaviour
{
    public RectTransform zone3;        // vùng nước (RectTransform trên Canvas)
    public float gravity = -2500f;     // px/giây^2 (tuỳ Canvas Scale)
    public float maxFallSpeed = -3500f;

    public SkeletonGraphic spine;      // SkeletonGraphic của nhân vật
    public string fallAnim = "fall";   // anim khi đang rơi (nếu có)
    public string hitWaterAnim = "action2"; // anim khi chạm zone3
    public bool loopWhenHit = false;
    public GameObject casau;


    private RectTransform rt;
    private float vy = 0f;
    private bool isFalling = true;
    private bool playedHit = false;

    void Awake()
    {
        rt = transform as RectTransform;
        if (spine != null && !spine.IsValid) spine.Initialize(true);
        // phát anim rơi nếu có
        TryPlay(spine, fallAnim, true);
    }

    void Update()
    {
        if (!isFalling || rt == null) return;

        // áp “gravity”
        vy += gravity * Time.unscaledDeltaTime;
        if (vy < maxFallSpeed) vy = maxFallSpeed;

        // cập nhật vị trí
        Vector2 p = rt.anchoredPosition;
        p.y += vy * Time.unscaledDeltaTime;
        rt.anchoredPosition = p;

        // kiểm tra chạm Zone3
        if (IsOverlap(rt, zone3))
        {
            isFalling = false;
            vy = 0f;

            if (!playedHit)
            {
                TryPlay(spine, hitWaterAnim, loopWhenHit);
                playedHit = true;
                casau.SetActive(false);
            }
        }
    }

    bool IsOverlap(RectTransform a, RectTransform b)
    {
        if (a == null || b == null) return false;
        Vector3[] ca = new Vector3[4]; Vector3[] cb = new Vector3[4];
        a.GetWorldCorners(ca); b.GetWorldCorners(cb);

        Rect ra = new Rect(ca[0].x, ca[0].y, ca[2].x - ca[0].x, ca[2].y - ca[0].y);
        Rect rb = new Rect(cb[0].x, cb[0].y, cb[2].x - cb[0].x, cb[2].y - cb[0].y);
        return ra.Overlaps(rb);
    }

    void TryPlay(SkeletonGraphic sg, string anim, bool loop)
    {
        if (sg == null || string.IsNullOrEmpty(anim)) return;
        if (!sg.IsValid) sg.Initialize(true);
        var a = sg.Skeleton?.Data?.FindAnimation(anim);
        if (a != null)
        {
            sg.AnimationState.SetAnimation(0, a, loop);
            
        }
    }
}
