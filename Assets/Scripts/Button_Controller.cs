using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Button_Controller : MonoBehaviour {
	private GameObject player;

    public GameObject ability;
    public GameObject dice;
    public GameObject left;
    public GameObject right;
	
	public void SetPlayer(GameObject target)
    {
        player = target;
    }

    public void DiceClicked(){
        player.GetComponent<Player_Behavior>().DiceClicked(dice,ability);
    }

    public void LeftClicked(){
        player.GetComponent<Player_Behavior>().LeftClicked(left,right);
    }

    public void RightClicked(){
        player.GetComponent<Player_Behavior>().RightClicked(left,right);
    }
}
