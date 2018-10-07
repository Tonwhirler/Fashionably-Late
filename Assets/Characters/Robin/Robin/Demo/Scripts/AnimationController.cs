using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AnimationStates
{
    HumanoidWalk,
    HumanoidRun,
    HumanoidIdle,
   
}

public class AnimationController : MonoBehaviour
{

    public Animator animator;

    public void PlayAnimation(AnimationStates statesAnimation)
    {

        switch (statesAnimation)
        {
            case AnimationStates.HumanoidIdle:
                {
                    StopAnimations();
                    animator.SetBool("inIdle", true);
                }
                break;
            case AnimationStates.HumanoidWalk:
                {
                    StopAnimations();
                    animator.SetBool("inWalk", true);
                }
                break;
            case AnimationStates.HumanoidRun:
                {
                    StopAnimations();
                    animator.SetBool("inRun", true);
                }
                break;
           
        }


    }

    void StopAnimations()
    {
        animator.SetBool("inRun", false);
        animator.SetBool("inWalk", false);
        animator.SetBool("inIdle", false);
    }

}

