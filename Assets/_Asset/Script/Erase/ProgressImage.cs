using UnityEngine;
using UnityEngine.UI;

public class ProgressImage : MonoBehaviour
{
    [SerializeField] private Image fill;
    public float currentProgress { get; private set; }

    public void UpdateProgress(float value)
    {
        currentProgress = Mathf.Clamp01(value);
        if (fill != null) fill.fillAmount = currentProgress;
    }
}
