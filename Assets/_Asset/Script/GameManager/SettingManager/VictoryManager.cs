using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VictoryManager : UICanvas
{
    public GameObject CanvasSetting;
    public GameObject window;          
    public SkeletonGraphic previewSpine;
    
    public GameObject canvasLV1;
    public GameObject canvasLV2;
    public GameObject canvasLV3;

    public void SettingBTN()
    {
        UIManager.Instance.OpenUI<CanvasSetting>();
        //CanvasSetting.SetActive(true);
    }

    public void Show(Sprite deathSprite = null, string spineAnim = null, bool loop = false)
    {
        window.SetActive(true);

        if (previewSpine)
        {
            bool useSpine = !string.IsNullOrEmpty(spineAnim);
            previewSpine.gameObject.SetActive(useSpine);
            if (useSpine)
                previewSpine.AnimationState.SetAnimation(0, spineAnim, loop);
        }
    }

    public void CloseUI()
    {
        gameObject.SetActive (false);
    }

    public void NextLv2()
    {
        canvasLV1.SetActive(false);
        canvasLV2.SetActive(true);
    }

    public void NextLv3()
    {
        canvasLV2.SetActive(false);
        canvasLV3.SetActive(true);
    }
}
