using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerBehaviour : MonoBehaviour
{


    private AnimationController animationController;

    public float speed;
    private float speedRun;
    private float speedWalk;
    private float rotateSpeed;


    // Use this for initialization
    void Start()
    {

        animationController = GetComponent<AnimationController>();
        speed = speedWalk = 4.5F;
        speed = speedRun = 9.0F;
        speed = rotateSpeed = 5.0F;
    }




    // Update is called once per frame
    void Update()
    {

        if (Input.GetKey(KeyCode.LeftShift))
        {
            speed = speedRun;
            animationController.PlayAnimation(AnimationStates.HumanoidRun);
        }

        else
        {
            speed = speedWalk;
            if (Input.GetAxis("Horizontal") != 0 || Input.GetAxis("Vertical") != 0)
                animationController.PlayAnimation(AnimationStates.HumanoidWalk);
            else
                animationController.PlayAnimation(AnimationStates.HumanoidIdle);
        }


        CharacterController controller = GetComponent<CharacterController>();
        transform.Rotate(0, Input.GetAxis("Horizontal") * rotateSpeed, 0);
        Vector3 forward = transform.TransformDirection(Vector3.forward);
        float curSpeed = speed * Input.GetAxis("Vertical");
        controller.SimpleMove(forward * curSpeed);

    }

}