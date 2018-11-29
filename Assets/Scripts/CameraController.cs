using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraController : MonoBehaviour {
    [HideInInspector]
    public GameObject player;       //Public variable to store a reference to the player game object

    public void SetPlayer(GameObject target)
    {
        player=target;
    }

    // LateUpdate is called after Update each frame
    void LateUpdate()
    {
        if(player == null)return;

        transform.position = player.transform.GetChild(0).position;
        transform.rotation = player.transform.GetChild(0).rotation;
        //smooth camera rotation, can modify speed
        /*transform.rotation = Quaternion.Lerp(transform.rotation,
            player.transform.GetChild(0).rotation,
            Time.deltaTime*(player.GetComponent<Player_Behavior>().rotationSpeed+1f));*/
    }
}
