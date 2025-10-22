using Spine.Unity;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class VictoryManager : UICanvas
{
    public GameObject CanvasSetting;
    
    public GameObject canvasLV1;
    public GameObject canvasLV2;
    public GameObject canvasLV7;
    public GameObject canvasLV9;
    public GameObject panelWin;

    public void SettingBTN()
    {
        UIManager.Instance.OpenUI<CanvasSetting>();
        
    }

    public void CloseUI()
    {
        gameObject.SetActive (false);
    }

    public void NextLv2()
    {
        canvasLV1.SetActive(false);
        canvasLV2.SetActive(true);
        panelWin.SetActive(false);
    }

    public void NextLv7()
    {
        canvasLV2.SetActive(false);
        canvasLV7.SetActive(true);
        panelWin.SetActive(false);
    }

    public void NextLV9()
    {
        canvasLV7.SetActive(false);
        canvasLV9.SetActive(true);
        panelWin.SetActive(false);
    }
}
