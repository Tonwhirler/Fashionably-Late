using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
    [HideInInspector]
    public GameObject player;       //Public variable to store a reference to the player game object

    private float rotationSpeed = 1.5;

    public void SetPlayer(GameObject target)
    {
        player=target;
    }

    // LateUpdate is called after Update each frame
    void LateUpdate()
    {
        if(player == null)return;

        transform.position = player.transform.GetChild(0).position;
        
        //smooth camera rotation, can modify speed
        transform.rotation = Quaternion.Lerp(transform.rotation,
            player.transform.GetChild(0).rotation,
            Time.deltaTime*rotationSpeed);
    }
}
