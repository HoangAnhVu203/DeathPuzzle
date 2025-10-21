using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ToggleCheckMove : MonoBehaviour
{
    public Toggle toggle;
    public RectTransform checkmark;  

    public Vector2 leftPos = new Vector2(-50, 0);  
    public Vector2 rightPos = new Vector2(50, 0);  

    void Start()
    {
        toggle.onValueChanged.AddListener(OnToggleChanged);

       
        checkmark.anchoredPosition = toggle.isOn ? rightPos : leftPos;
    }

    void OnToggleChanged(bool isOn)
    {
        checkmark.anchoredPosition = isOn ? rightPos : leftPos;
    }
}
