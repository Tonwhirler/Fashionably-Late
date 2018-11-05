using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;
using UnityEngine.Networking.NetworkSystem;


public class Ability_Controller : MonoBehaviour {
    public GameObject player;
    [HideInInspector]
    public bool abilityUsed = false;
    private int currentAbility = 0;

    private Text text_ability;

    public void GiveStartingAbilities()
    {
        text_ability = GameObject.Find("AbilityButton").GetComponentInChildren<Text>();
        //give player ability to start with
        RandomAbility();
    }

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

    public void UseAbility()
    {
        Debug.Log("Use ability called");
        if (currentAbility == 0)
        {
            GiveStartingAbilities();
        }
        switch (currentAbility)
        {
            case (int) MyAbilityType.ForceBack:
                
                if (abilityUsed == false && !player.GetComponent<Player_Behavior>().isMoving && player.GetComponent<Player_Behavior>().isMyTurn)
                {
                    Debug.Log("Use ability ForceBack");
                    //move player 1 space back, the 1 space is temporary
                    player.GetComponent<Player_Behavior>().numSpacesToMove = 1;
                    NetworkManager.singleton.client.Send(MsgType.Highest + 1, new IntegerMessage((int)MyMessageType.ItemMoveBackwards));
                    abilityUsed = true;
                    //give new ability when old one is used
                    RandomAbility();
                }            
                break;

            case (int) MyAbilityType.ForceForward:

                if (abilityUsed == false && !player.GetComponent<Player_Behavior>().isMoving && player.GetComponent<Player_Behavior>().isMyTurn)
                {
                    Debug.Log("Use ability ForceForward");
                    //move player 1 space forward, the 1 space is temporary
                    player.GetComponent<Player_Behavior>().numSpacesToMove = 1;
                    NetworkManager.singleton.client.Send(MsgType.Highest + 1, new IntegerMessage((int)MyMessageType.PlayerMoveForwards));
                    abilityUsed = true;
                    //give new ability when old one is used
                    RandomAbility();
                }
                break;

            case (int) MyAbilityType.Freeze:

                if (abilityUsed == false && !player.GetComponent<Player_Behavior>().isMoving && player.GetComponent<Player_Behavior>().isMyTurn)
                {
                    Debug.Log("Use ability Freeze");
                    //set frozen bool so player cannot take turn
                    player.GetComponent<Player_Behavior>().isFrozen = true;
                    abilityUsed = true;
                    //give new ability when old one is used
                    RandomAbility();
                }              
                break;

            default:
                Debug.Log("Ability enum does not exist");
                break;
        }
    }

    public void RandomAbility()
    {
        currentAbility = Random.Range(1,4); // creates a number between 1 and 3
        Debug.Log("Current Ability = " + currentAbility);
        UpdateUI();
    }

    private void UpdateUI()
    {
        //update the text on the button
        switch (currentAbility)
        {
            case (int)MyAbilityType.ForceBack:
                text_ability.text = "Force Back";
                break;

            case (int)MyAbilityType.ForceForward:
                text_ability.text = "Force Forward";
                break;

            case (int)MyAbilityType.Freeze:
                text_ability.text = "Freeze";
                break;

            default:
                Debug.Log("Ability button text not set");
                break;
        }
    }
}
