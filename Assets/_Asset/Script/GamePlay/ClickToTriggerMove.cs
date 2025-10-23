using UnityEngine;
using UnityEngine.EventSystems;

public class ClickToTriggerMove : MonoBehaviour, IPointerClickHandler
{
    
    public MoveAfterDelay[] movers;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (movers == null || movers.Length == 0) return;

        for (int i = 0; i < movers.Length; i++)
        {
            if (movers[i] == null) continue;
            movers[i].TriggerMove();
        }
    }
}
