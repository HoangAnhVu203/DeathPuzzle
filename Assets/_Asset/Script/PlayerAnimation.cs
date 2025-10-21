using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Spine.Unity;

public class PlayerAnimation : MonoBehaviour
{
    public SkeletonGraphic skeleton;
    
    

    private void Awake()
    {
        if(skeleton == null)
        {
            skeleton = GetComponent<SkeletonGraphic>();
        }
    }

    private void Start()
    {
        skeleton.AnimationState.SetAnimation(0, "idle", true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Q))
        {
            IdleAinm();
        }

        if (Input.GetKeyDown(KeyCode.E))
        {
            Action2Anim();
            Debug.Log("action2");
        }

        if (Input.GetKeyDown(KeyCode.W))
        {
            Action1Anim();
            Debug.Log("action1");
        }

        if (Input.GetKeyDown(KeyCode.R))
        {
            Action3Anim();
            Debug.Log("action3");
        }
    }

    public void IdleAinm()
    {
        skeleton.AnimationState.SetAnimation(0, "idle", true);
    }

    public void Action2Anim()
    {
        skeleton.AnimationState.SetAnimation(0, "action2", true);
    }

    public void Action1Anim()
    {
        skeleton.AnimationState.SetAnimation(0, "action1", true);
    }

    public void Action3Anim()
    {
        skeleton.AnimationState.SetAnimation(0, "action3", true);
    }
}
