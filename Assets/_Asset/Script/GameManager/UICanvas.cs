using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;

public class UICanvas : Singleton<UICanvas>
{
    [SerializeField] bool isDestroyOnClose = false;

  
    //goi truoc khi canvas active
    public virtual void SetUp()
    {

    }

    //goi sau khi duoc active
    public virtual void Open()
    {
        gameObject.SetActive(true);
    }

    //tat canvas sau n time(s)
    public virtual void Close(float time)
    {
        Invoke(nameof(CloseDirectly), time);
    }

    //tat luon canvas
    public virtual void CloseDirectly()
    {
        if (isDestroyOnClose)
        {
            Destroy(gameObject);
        }
        else
        {
            gameObject.SetActive(false);

        }
    }
}
