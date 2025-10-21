using UnityEngine;
using Spine;
using Spine.Unity;
using System.Collections;

public class Action2 : MonoBehaviour
{
    public SkeletonGraphic source;       
    public SkeletonGraphic target;        
    public GameObject canvasVictory;    
    public string dieAnim = "action2";

    public float showDelay = 2.5f;

    void OnEnable() { if (source) source.AnimationState.Start += OnSourceStart; }
    void OnDisable() { if (source) source.AnimationState.Start -= OnSourceStart; }

    void OnSourceStart(TrackEntry e)
    {
        if (e.TrackIndex != 0 || e.Animation == null) return;
        if (e.Animation.Name != dieAnim) return;

        StartCoroutine(ShowVictoryAndSyncAfter(showDelay));
    }

    IEnumerator ShowVictoryAndSyncAfter(float delay)
    {
        yield return new WaitForSeconds(delay);

        if (canvasVictory && !canvasVictory.activeSelf)
            //UIManager.Instance.OpenUI<VictoryManager>();
            canvasVictory.SetActive(true);

        target.Initialize(true);

        var skin = source.Skeleton.Skin;
        if (skin != null)
        {
            target.Skeleton.SetSkin(skin);
            target.Skeleton.SetSlotsToSetupPose();
        }
        target.Skeleton.ScaleX = Mathf.Sign(source.Skeleton.ScaleX) * Mathf.Abs(target.Skeleton.ScaleX);
        target.Skeleton.ScaleY = Mathf.Sign(source.Skeleton.ScaleY) * Mathf.Abs(target.Skeleton.ScaleY);

        var srcEntry = source.AnimationState.GetCurrent(0);
        if (srcEntry != null)
        {
            var name = srcEntry.Animation.Name;
            var loop = srcEntry.Loop;

            var tgtEntry = target.AnimationState.SetAnimation(0, name, loop);
            tgtEntry.TrackTime = srcEntry.TrackTime;  
            target.timeScale = source.timeScale;    
            target.LateUpdate();                      
        }
    }
}
