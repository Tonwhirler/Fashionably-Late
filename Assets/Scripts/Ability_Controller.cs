using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Networking.NetworkSystem;

public class Ability_Controller : MonoBehaviour {
    public GameObject player;
    private bool abilityUsed = false;

    public enum MyAbilityType
    {
        ForceBack = 1,
        ForceForward = 2,
        Freeze = 3
    }

    public void SetPlayer(GameObject target)
    {
        player = target;
    }

    public void UseAbility(int ability)
    {
        switch (ability)
        {
            case (int) MyAbilityType.ForceBack:
                Debug.Log("Use ability ForceBack");

                //Debug.Log("ability used = " + abilityUsed + " ismoving = " + isMoving + " isMyTurn = " + isMyTurn);
                //if (abilityUsed == false && !isMoving && isMyTurn)
                //{
                //    NetworkManager.singleton.client.Send(MsgType.Highest + 1, new IntegerMessage((int)MyMessageType.ItemMoveBackwards));
                //}
                abilityUsed = true;
                break;

            case (int) MyAbilityType.ForceForward:
                Debug.Log("Use ability ForceForward");
                abilityUsed = true;
                break;

            case (int) MyAbilityType.Freeze:
                Debug.Log("Use ability Freeze");
                abilityUsed = true;
                break;

            default:
                Debug.Log("Ability enum does not exist");
                break;
        }
    }
}
