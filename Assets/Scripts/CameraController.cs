using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
    [HideInInspector]
    public GameObject player;       //Public variable to store a reference to the player game object

    //private float rotationSpeed = 2;

    public void SetPlayer(GameObject target)
    {
        player=target;
    }

    // LateUpdate is called after Update each frame
    void LateUpdate()
    {
        if(player == null)return;

        transform.position = player.transform.GetChild(0).position;
                //Slerp is smooth rotation, but target is messed up somehow
                //transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(player.transform.position), rotationSpeed * Time.deltaTime);
        transform.LookAt(player.transform.position); //instant rotation, doesn't look good but works
    }
}
