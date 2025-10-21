using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CanvasGamePlay : UICanvas
{
    public GameObject canvasSetting;
    
    public void SettingBTN()
    {
        UIManager.Instance.OpenUI<CanvasSetting>();
        //canvasSetting.SetActive(true);
    }

}
